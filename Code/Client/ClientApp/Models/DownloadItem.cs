namespace ClientApp.Models
{
    public class DownloadItem
    {
        public string FileName { get; set; }

        // Tên THẬT của file trên Server — dùng để gửi lệnh GET, KHÔNG BAO GIỜ thay đổi
        // trong suốt vòng đời của item, kể cả khi FileName (tên hiển thị/lưu cục bộ) bị
        // đổi do trùng tên với file đã tải trước đó. Nếu không tách riêng, sau khi bị đổi
        // tên cục bộ 1 lần thì mọi lần "Thử lại"/"Tải" sau đó sẽ gửi NHẦM tên đã đổi lên
        // Server (tên này không tồn tại trên Server) → Server luôn báo lỗi "không tồn
        // tại" → tải thất bại vĩnh viễn dù bấm lại bao nhiêu lần.
        public string SourceFileName { get; set; }

        public long FileSize { get; set; }

        public long DownloadedBytes { get; set; }

        public double Progress { get; set; }

        public string Status { get; set; }

        // Lý do thất bại gần nhất (thông điệp exception thật) — trước đây mọi lỗi đều bị
        // "nuốt" hoàn toàn (catch rỗng), khiến người dùng chỉ thấy trạng thái "Lỗi" chung
        // chung mà không biết lý do kỹ thuật là gì để báo lại hoặc tự khắc phục.
        public string? LastError { get; set; }

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
            SourceFileName = string.Empty;
            Status = "Waiting";
        }

        public DownloadItem(string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Tên file không hợp lệ.");

            if (fileSize < 0)
                throw new ArgumentException("Kích thước file không hợp lệ.");

            FileName = fileName;
            SourceFileName = fileName; // Tên thật trên Server tại thời điểm khởi tạo
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