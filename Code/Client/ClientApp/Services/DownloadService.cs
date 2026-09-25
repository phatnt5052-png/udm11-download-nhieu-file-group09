using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Services
{
    public class DownloadService
    {
        private readonly TcpClientService _clientService; // Giao tiếp Client - Server
        private readonly string _downloadFolder; // Đường dẫn thư mục dùng để lưu file tải xuống
        private readonly SemaphoreSlim _semaphore; // Giới hạn số lượng file tải xuống cùng lúc

        // Quy tắc xử lý khi file trùng tên
        public enum OverwriteRule { Overwrite, Rename }
        public OverwriteRule TargetRule { get; set; } = OverwriteRule.Rename;

        private const string PartialSuffix = ".partial"; // File sẽ có đuôi .partial

        // Nếu không nhận thêm được byte dữ liệu nào trong khoảng thời gian này khi đang
        // tải, coi như kết nối đã "chết" (ví dụ rút dây mạng vật lý mà không có gói
        // FIN/RST nào được gửi) và chủ động báo lỗi ngay, thay vì để tiến trình tải bị
        // treo vô thời hạn ở trạng thái "Đang tải" — đúng hiện tượng "dừng trạng thái
        // tải file" khi mất kết nối giữa chừng mà không bao giờ chuyển sang "Lỗi".
        private static readonly TimeSpan InactivityTimeout = TimeSpan.FromSeconds(15);

        public DownloadService(TcpClientService clientService, int maxConcurrentDownloads)
        {
            _clientService = clientService;
            _semaphore = new SemaphoreSlim(maxConcurrentDownloads);
            //Kiểm tra thư mục Download
            _downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }

        public async Task ExecuteDownloadAsync(DownloadItem item, Action<DownloadItem>? onProgress = null, CancellationToken cancellationToken = default)
        {
            // Chờ Semaphore
            await _semaphore.WaitAsync(cancellationToken);
            item.Status = DownloadStatus.Downloading;
            var progressService = new ProgressService(item);
            string? targetFilePath = null;
            string? partialFilePath = null;

            try
            {
                // QUAN TRỌNG: "tên gửi lên Server" và "tên file lưu cục bộ" PHẢI tách biệt.
                // SourceFileName là tên thật, bất biến, dùng để xin Server — còn FileName có
                // thể bị đổi (xem nhánh Rename bên dưới) khi trùng tên với file đã tải trước
                // đó. Nếu dùng chung 1 biến, sau khi bị đổi tên cục bộ 1 lần thì mọi lần "Thử
                // lại" sau sẽ gửi NHẦM tên đã đổi lên Server (tên không tồn tại trên Server)
                // → Server luôn báo lỗi "không tồn tại" → tải thất bại vĩnh viễn.
                string sourceFileName = item.SourceFileName;
                string localFileName = item.FileName;
                targetFilePath = Path.Combine(_downloadFolder, localFileName);
                partialFilePath = targetFilePath + PartialSuffix; // File.partial dùng cho trường hợp file đang tải dở/lỗi


                bool isResuming = File.Exists(partialFilePath);
                long resumeOffset = 0;

                if (isResuming)
                {
                    resumeOffset = new FileInfo(partialFilePath).Length;
                }
                else if (File.Exists(targetFilePath))
                {
                    if (TargetRule == OverwriteRule.Rename)
                    {
                        string ext = Path.GetExtension(localFileName);
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(localFileName);
                        string uniqueName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        targetFilePath = Path.Combine(_downloadFolder, uniqueName);
                        partialFilePath = targetFilePath + PartialSuffix;
                        item.FileName = uniqueName; // Chỉ đổi tên HIỂN THỊ/LƯU CỤC BỘ, không đụng tới SourceFileName
                    }
                    else if (TargetRule == OverwriteRule.Overwrite)
                    {
                        File.Delete(targetFilePath);
                    }
                }

                string thisPartialFilePath = partialFilePath;
                string thisTargetFilePath = targetFilePath;
                long thisResumeOffset = resumeOffset;

                // Tải file từ Server bằng ĐÚNG TÊN THẬT trên Server (sourceFileName), kèm vị
                // trí byte muốn tiếp tục — bất kể tên hiển thị/lưu cục bộ đã từng bị đổi hay chưa.
                await _clientService.DownloadFileFromServerAsync(sourceFileName, async (networkStream, remainingSize) =>
                {
                    // Server luôn trả về đúng số byte CÒN LẠI dựa trên kích thước thật hiện
                    // tại của file trên Server. Tự đồng bộ lại FileSize hiển thị theo giá trị
                    // này (thay vì tin tưởng tuyệt đối kích thước lấy được lúc LIST trước đó)
                    // để thanh tiến trình luôn tính đúng %, kể cả khi resume.
                    item.FileSize = thisResumeOffset + remainingSize;

                    using var fileStream = new FileStream(
                        thisPartialFilePath,
                        thisResumeOffset > 0 ? FileMode.Append : FileMode.Create,
                        FileAccess.Write,
                        FileShare.None);

                    byte[] buffer = new byte[8192];
                    long sessionBytesRead = 0;

                    while (sessionBytesRead < remainingSize)
                    {
                        // Kiểm tra CancellationToken
                        cancellationToken.ThrowIfCancellationRequested();

                        int bytesToRead = (int)Math.Min(buffer.Length, remainingSize - sessionBytesRead);

                        // LƯU Ý: Task.Delay ở đây KHÔNG được gắn cancellationToken của người
                        // dùng — nếu gắn, khi người dùng bấm "Hủy" thì delayTask cũng lập tức
                        // chuyển sang trạng thái Canceled và có thể "thắng" Task.WhenAny, khiến
                        // một lượt hủy chủ động của người dùng bị báo nhầm thành "mất kết nối do
                        // timeout". readTask (có gắn cancellationToken) đã tự đảm nhiệm việc phản
                        // ứng với hủy: khi người dùng bấm Hủy, chính readTask sẽ chuyển sang
                        // Canceled và "thắng" Task.WhenAny, sau đó `await readTask` bên dưới sẽ
                        // ném đúng OperationCanceledException.
                        Task<int> readTask = networkStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);
                        Task delayTask = Task.Delay(InactivityTimeout);
                        Task completedTask = await Task.WhenAny(readTask, delayTask);

                        if (completedTask == delayTask)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            throw new IOException($"Mất kết nối tới Server: không nhận được dữ liệu trong {InactivityTimeout.TotalSeconds:F0} giây.");
                        }

                        int bytesRead = await readTask;

                        if (bytesRead == 0)
                        {
                            throw new IOException("Server ngắt kết nối đột ngột khi chưa gửi đủ file.");
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        sessionBytesRead += bytesRead;

                        // Cập nhật Process
                        long totalDownloaded = thisResumeOffset + sessionBytesRead;
                        progressService.UpdateProgress(totalDownloaded);

                        // Báo UI cập nhật tiến trình realtime
                        onProgress?.Invoke(item);
                    }
                }, cancellationToken, resumeOffset);

                // Tải xong trọn vẹn — đổi tên .partial thành tên file thật.
                if (File.Exists(thisTargetFilePath))
                {
                    File.Delete(thisTargetFilePath);
                }
                File.Move(thisPartialFilePath, thisTargetFilePath);

                item.Status = DownloadStatus.Completed;
                item.Progress = 100;
                item.SpeedMbps = 0;
                onProgress?.Invoke(item);
            }
            catch (ResumeOffsetInvalidException ex)
            {
                // File .partial hiện có không còn khớp với file thật trên Server (đã bị
                // thay thế/co lại). Xoá file .partial hỏng ngay để lần "Thử lại" tiếp theo
                // của người dùng tự động tải lại TỪ ĐẦU thay vì lặp lại lỗi này mãi mãi.
                try
                {
                    if (partialFilePath != null && File.Exists(partialFilePath))
                    {
                        File.Delete(partialFilePath);
                    }
                }
                catch
                {
                    // Bỏ qua lỗi xóa file phụ — không để ảnh hưởng luồng chính.
                }

                item.Status = DownloadStatus.Failed;
                item.SpeedMbps = 0;
                item.LastError = ex.Message;
                onProgress?.Invoke(item);
            }
            catch (Exception ex)
            {
                item.Status = DownloadStatus.Failed;
                item.SpeedMbps = 0;
                item.LastError = ex.Message;
                onProgress?.Invoke(item);
            }

            finally
            {
                _semaphore.Release();
            }
        }

        // Xóa file .partial (nếu có) của 1 item — dùng khi người dùng chủ động Xóa
        // file đó khỏi hàng đợi, để không để lại rác trong thư mục Downloads.
        public void DeletePartialFile(DownloadItem item)
        {
            try
            {
                string targetFilePath = Path.Combine(_downloadFolder, item.FileName);
                string partialFilePath = targetFilePath + PartialSuffix;

                if (File.Exists(partialFilePath))
                {
                    File.Delete(partialFilePath);
                }
            }
            catch
            {
                // Bỏ qua lỗi xóa file phụ — không để ảnh hưởng luồng chính.
            }
        }

        public async Task StartAllAsync(DownloadQueueService queueService, Action<DownloadItem>? onProgress = null)
        {
            var items = queueService.GetQueue();
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                tasks.Add(ExecuteDownloadAsync(item, onProgress));
            }
            await Task.WhenAll(tasks);
        }
    }
}