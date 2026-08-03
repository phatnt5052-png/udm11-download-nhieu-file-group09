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
                throw new ArgumentException("Ten file khong duoc de trong.");

            if (fileSize < 0)
                throw new ArgumentException("Kich thuoc file khong hop le.");

            FileName = fileName;
            FileSize = fileSize;
        }
    }
}
