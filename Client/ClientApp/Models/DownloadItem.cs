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

        public DownloadItem()
        {
            FileName = string.Empty;
            Status = "Waiting";
        }

        public DownloadItem(string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Ten file không hop le.");

            if (fileSize < 0)
                throw new ArgumentException("Kich thuoc file khong hop le.");

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