using System;
using System.Collections.Generic;
using System.IO;
using ServerApp.Models;

namespace ServerApp.Services
{
    /// <summary>
    /// Dịch vụ quản lý và truy xuất tập tin trên Server.
    /// </summary>
    public class FileService
    {
        private readonly ServerConfig _config;

        public FileService(ServerConfig config)
        {
            _config = config ?? new ServerConfig();
            EnsureDirectoryExists();
        }

        /// <summary>
        /// Kiểm tra và tạo thư mục chia sẻ nếu nó chưa tồn tại.
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_config.SharedFolder))
            {
                Directory.CreateDirectory(_config.SharedFolder);
            }
        }

        /// <summary>
        /// Lấy danh sách tên tất cả các file có trong thư mục chia sẻ.
        /// </summary>
        /// <returns>Danh sách tên các file.</returns>
        public List<string> GetFileList()
        {
            EnsureDirectoryExists();
            List<string> fileList = new List<string>();
            string[] files = Directory.GetFiles(_config.SharedFolder);

            foreach (string file in files)
            {
                fileList.Add(Path.GetFileName(file));
            }

            return fileList;
        }

        /// <summary>
        /// Kiểm tra xem một file có tồn tại trong thư mục chia sẻ hay không.
        /// </summary>
        /// <param name="fileName">Tên file cần kiểm tra.</param>
        /// <returns>True nếu file tồn tại, ngược lại là False.</returns>
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string filePath = Path.Combine(_config.SharedFolder, fileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Mở một luồng dữ liệu (FileStream) để đọc file, hỗ trợ chia sẻ quyền đọc.
        /// </summary>
        /// <param name="fileName">Tên file cần mở.</param>
        /// <returns>Một FileStream để đọc dữ liệu file.</returns>
        /// <exception cref="FileNotFoundException">Bắn ra khi file không tồn tại.</exception>
        public FileStream OpenReadStream(string fileName)
        {
            if (!FileExists(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' không tồn tại.");
            }

            string filePath = Path.Combine(_config.SharedFolder, fileName);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}