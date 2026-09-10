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
        private readonly TcpClientService _clientService;
        private readonly string _downloadFolder;
        private readonly SemaphoreSlim _semaphore;

        // Quy tắc xử lý khi file trùng tên
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

        public async Task ExecuteDownloadAsync(DownloadItem item, Action<DownloadItem>? onProgress = null)
        {
            await _semaphore.WaitAsync();
            item.Status = DownloadStatus.Downloading;
            var progressService = new ProgressService(item);

            try
            {
                // Lưu tên file gốc để yêu cầu đúng file từ Server
                string originalFileName = item.FileName;
                string targetFilePath = Path.Combine(_downloadFolder, originalFileName);

                if (File.Exists(targetFilePath))
                {
                    if (TargetRule == OverwriteRule.Rename)
                    {
                        string ext = Path.GetExtension(originalFileName);
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                        string uniqueName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        targetFilePath = Path.Combine(_downloadFolder, uniqueName);
                        item.FileName = uniqueName; // Cập nhật tên hiển thị mới trên giao diện
                    }
                    else if (TargetRule == OverwriteRule.Overwrite)
                    {
                        File.Delete(targetFilePath);
                    }
                }

                // Tải file từ Server bằng tên file gốc
                await _clientService.DownloadFileFromServerAsync(originalFileName, async (networkStream, size) =>
                {
                    using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    byte[] buffer = new byte[8192];
                    long totalBytesRead = 0;

                    // ĐỌC ĐÚNG SỐ BYTE CỦA FILE (KHÔNG CHỜ EOF ĐỂ TRÁNH TREO)
                    while (totalBytesRead < size)
                    {
                        int bytesToRead = (int)Math.Min(buffer.Length, size - totalBytesRead);
                        int bytesRead = await networkStream.ReadAsync(buffer, 0, bytesToRead);

                        if (bytesRead == 0)
                        {
                            throw new IOException("Server ngắt kết nối đột ngột khi chưa gửi đủ file.");
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        // Cập nhật tiến độ dữ liệu
                        progressService.UpdateProgress(totalBytesRead);

                        // Báo UI cập nhật tiến trình realtime
                        onProgress?.Invoke(item);
                    }
                });

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