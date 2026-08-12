using ClientApp.Models;
namespace ClientApp.Services
{
    public class ProgressService
    {
        public event EventHandler<DownloadItem>? ProgressChanged;
        private readonly Dictionary<string, (long bytes, DateTime time)> _lastUpdate = new();
        
        public void UpdateProgress(DownloadItem item, long downloadedBytes)
        {
            item.DownloadedBytes = downloadedBytes;
            if (item.FileSize > 0)
            {
                item.Progress = (double)downloadedBytes / item.FileSize * 100;
            }

            DateTime now = DateTime.Now;
            if (_lastUpdate.TryGetValue(item.FileName, out var last))
            {
                double secondsElapsed = (now - last.time).TotalSeconds;
                if (secondsElapsed > 0)
                {
                    long bytesSinceLast = downloadedBytes - last.bytes;
                    double mbSinceLast = bytesSinceLast / 1024.0 / 1024.0;
                    item.SpeedMbps = mbSinceLast / secondsElapsed;
                }
            }
            _lastUpdate[item.FileName] = (downloadedBytes, now);
            ProgressChanged?.Invoke(this, item);
        }
        
    }
}
