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
    }
}
