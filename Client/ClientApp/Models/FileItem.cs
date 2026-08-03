namespace ClientApp.Models
{
    public class FileItem
    {
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public FileItem()
        {
            FileName = string.Empty;
            FileSize = 0;
        }

        public FileItem(string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Tên file không được để trống.");

            if (fileSize < 0)
                throw new ArgumentException("Kích thước file không hợp lệ.");

            FileName = fileName;
            FileSize = fileSize;
        }
    }
}
