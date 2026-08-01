namespace ClientApp.Models
{
    public class DownloadItem
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long DownloadedBytes { get; set; }
        public double Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public double SpeedMbps { get; set; }
    }
}
