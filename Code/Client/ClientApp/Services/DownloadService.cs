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
                //Tạo đường dẫn file
                string originalFileName = item.FileName;
                targetFilePath = Path.Combine(_downloadFolder, originalFileName);
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
                        string ext = Path.GetExtension(originalFileName);
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                        string uniqueName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        targetFilePath = Path.Combine(_downloadFolder, uniqueName);
                        partialFilePath = targetFilePath + PartialSuffix;
                        item.FileName = uniqueName; // Cập nhật tên hiển thị mới trên giao diện
                    }
                    else if (TargetRule == OverwriteRule.Overwrite)
                    {
                        File.Delete(targetFilePath);
                    }
                }

                string thisPartialFilePath = partialFilePath;
                string thisTargetFilePath = targetFilePath;
                long thisResumeOffset = resumeOffset;

                // Tải file từ Server bằng tên file gốc, kèm vị trí byte muốn tiếp tục
                await _clientService.DownloadFileFromServerAsync(originalFileName, async (networkStream, remainingSize) =>
                {
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
                        int bytesRead = await networkStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);

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
            catch
            {
                item.Status = DownloadStatus.Failed;
                item.SpeedMbps = 0;
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