namespace ClientApp.Models
{
    public class DownloadItem
    {
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public long DownloadedBytes { get; set; }

        public double Progress { get; set; }

        public string Status { get; set; }

        public double SpeedMbps { get; set; }

        // Backward-compatible alias for older code using Speed
        public double Speed
        {
            get => SpeedMbps;
            set => SpeedMbps = value;
        }

        public DownloadItem()
        {
            FileName = string.Empty;
            Status = "Waiting";
        }

        public DownloadItem(string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Tên file không hợp lệ.");

            if (fileSize < 0)
                throw new ArgumentException("Kích thước file không hợp lệ.");

            FileName = fileName;
            FileSize = fileSize;
            DownloadedBytes = 0;
            Progress = 0;
            SpeedMbps = 0;
            Status = "Waiting";
        }

        public override string ToString()
        {
            return $"{FileName} - {Progress:F1}% - {Status}";
        }
    }
}