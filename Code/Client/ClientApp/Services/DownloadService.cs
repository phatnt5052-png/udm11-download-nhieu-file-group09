using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Services
{
    public class DownloadService
    {
        private readonly TcpClientService _clientService;
        private readonly string _downloadFolder;
        private readonly SemaphoreSlim _semaphore;

        // Quy tắc xử lý khi file trùng tên (Có thể mở rộng tùy chọn)
        public enum OverwriteRule { Overwrite, Rename }
        public OverwriteRule TargetRule { get; set; } = OverwriteRule.Rename;

        public DownloadService(TcpClientService clientService, int maxConcurrentDownloads)
        {
            _clientService = clientService;
            _semaphore = new SemaphoreSlim(maxConcurrentDownloads);

            _downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }

        public async Task ExecuteDownloadAsync(DownloadItem item)
        {
            await _semaphore.WaitAsync();
            item.Status = DownloadStatus.Downloading;
            var progressService = new ProgressService(item);

            try
            {
                // Giữ tên file gốc để gửi cho server
                string originalFileName = item.FileName;

                string targetFilePath = Path.Combine(_downloadFolder, item.FileName);
                if (File.Exists(targetFilePath))
                {
                    if (TargetRule == OverwriteRule.Rename)
                    {
                        string ext = Path.GetExtension(item.FileName);
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(item.FileName);
                        string uniqueName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        targetFilePath = Path.Combine(_downloadFolder, uniqueName);
                        // NƠI NÀY CHỈ ĐỔI LOCAL PATH, KHÔNG ĐỔI item.FileName
                        // item.FileName vẫn giữ tên gốc để gửi cho server

                    }
                    else if (TargetRule == OverwriteRule.Overwrite)
                    {
                        File.Delete(targetFilePath);
                    }
                }

                // Download file từ server - GỬI TÊN FILE GỐC
                await _clientService.DownloadFileFromServerAsync(originalFileName, async (networkStream, size) =>
                {
                    using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    byte[] buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        progressService.UpdateProgress(totalBytesRead);
                    }

                    // Verify file size matches
                    if (totalBytesRead != size)
                    {
                        throw new IOException($"Kích thước file không khớp. Nhận: {totalBytesRead}, Mong đợi: {size}");
                    }
                });

                // Update UI với tên file được lưu
                item.FileName = Path.GetFileName(targetFilePath);
                item.Status = DownloadStatus.Completed;
                item.Progress = 100;
                item.SpeedMbps = 0;
            }
            catch (Exception ex)
            {
                item.Status = DownloadStatus.Failed;
                item.SpeedMbps = 0;
                // Log error nếu cần
                System.Diagnostics.Debug.WriteLine($"Download Failed: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StartAllAsync(DownloadQueueService queueService)
        {
            var items = queueService.GetQueue();
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                tasks.Add(ExecuteDownloadAsync(item));
            }
            await Task.WhenAll(tasks);
        }
    }
}
