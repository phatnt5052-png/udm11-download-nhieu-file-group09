using System;
using System.Collections.Generic;
using System.IO;
using ServerApp.Models;

namespace ServerApp.Services
{
    public class FileService
    {
        private readonly ServerConfig _config;

        public FileService(ServerConfig config)
        {
            _config = config ?? new ServerConfig();
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_config.SharedFolder))
            {
                Directory.CreateDirectory(_config.SharedFolder);
            }
        }

        // Lấy danh sách file
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

        // Kiểm tra file tồn tại
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string filePath = Path.Combine(_config.SharedFolder, fileName);
            return File.Exists(filePath);
        }

        // Lấy kích thước hiện tại của file trên Server — dùng để Server tự tính
        // số byte còn lại cần gửi khi Client yêu cầu tiếp tục (resume) tải dở.
        public long GetFileSize(string fileName)
        {
            if (!FileExists(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' không tồn tại.");
            }

            string filePath = Path.Combine(_config.SharedFolder, fileName);
            return new FileInfo(filePath).Length;
        }

        // Mở stream đọc file, có thể bắt đầu đọc từ một vị trí byte (offset) chỉ định
        // để phục vụ tính năng tiếp tục tải dở (resume) thay vì luôn đọc lại từ đầu.
        public FileStream OpenReadStream(string fileName, long offset = 0)
        {
            if (!FileExists(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' không tồn tại.");
            }

            string filePath = Path.Combine(_config.SharedFolder, fileName);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (offset > 0)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            return stream;
        }
    }
}