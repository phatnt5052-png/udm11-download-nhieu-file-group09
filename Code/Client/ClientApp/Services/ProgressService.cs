using System;
using ClientApp.Models;

namespace ClientApp.Services
{
    public class ProgressService
    {
        private readonly DownloadItem _item;
        private DateTime _lastTime;
        private long _lastBytes;

        public ProgressService(DownloadItem item)
        {
            _item = item;
            _lastTime = DateTime.Now;
            _lastBytes = 0;
        }

        public void UpdateProgress(long bytesRead)
        {
            if (_item.FileSize <= 0) return;

            int progressPercent = (int)((bytesRead * 100) / _item.FileSize);
            _item.Progress = Math.Min(progressPercent, 100);

            var now = DateTime.Now;
            var elapsed = (now - _lastTime).TotalSeconds;

            if (elapsed >= 0.5) // Cập nhật tốc độ mỗi 0.5s để tránh nhảy số liên tục
            {
                long currentDelta = bytesRead - _lastBytes;
                double speedKb = (currentDelta / 1024.0) / elapsed;
                _item.Speed = Math.Round(speedKb, 2);

                _lastTime = now;
                _lastBytes = bytesRead;
            }
        }
    }
}
