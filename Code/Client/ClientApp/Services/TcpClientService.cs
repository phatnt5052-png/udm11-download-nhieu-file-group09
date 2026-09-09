using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Services
{
    public class TcpClientService
    {
        private readonly string _ip;
        private readonly int _port;

        public TcpClientService(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

       
        public async Task<List<FileItem>> GetFileListAsync()
        {
            var list = new List<FileItem>();

            using var client = new TcpClient();

            await client.ConnectAsync(_ip, _port);

            using NetworkStream stream = client.GetStream();

            // Gửi command LIST theo protocol binary
            await WriteStringAsync(stream, "LIST");

            // Server trả về số lượng file
            int count = await ReadInt32Async(stream);

            if (count < 0)
            {
                throw new IOException("Số lượng file từ Server không hợp lệ.");
            }

            // Đọc từng file
            for (int i = 0; i < count; i++)
            {
                string fileName = await ReadStringAsync(stream);

                long fileSize = await ReadInt64Async(stream);

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    list.Add(new FileItem(fileName, fileSize));
                }
            }

            return list;
        }

        
        public async Task DownloadFileFromServerAsync(
            string fileName,
            Func<Stream, long, Task> dataHandler)
        {
            using var client = new TcpClient();

            await client.ConnectAsync(_ip, _port);

            using NetworkStream stream = client.GetStream();

            // Gửi GET
            await WriteStringAsync(stream, "GET");

            // Gửi tên file riêng biệt
            await WriteStringAsync(stream, fileName);

            // Đọc response
            string response = await ReadStringAsync(stream);

            if (response == "OK")
            {
                // Server gửi lại tên file
                string serverFileName = await ReadStringAsync(stream);

                // Server gửi kích thước file
                long fileSize = await ReadInt64Async(stream);

                if (fileSize < 0)
                {
                    throw new IOException(
                        "Kích thước file từ Server không hợp lệ.");
                }

                // Đưa stream + kích thước cho DownloadService xử lý
                await dataHandler(stream, fileSize);
            }
            else if (response == "ERROR")
            {
                string errorMessage = await ReadStringAsync(stream);

                throw new FileNotFoundException(
                    errorMessage);
            }
            else
            {
                throw new IOException(
                    $"Server trả về phản hồi không hợp lệ: {response}");
            }
        }

        
        private static async Task WriteStringAsync(
            NetworkStream stream,
            string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            await WriteInt32Async(stream, data.Length);

            await stream.WriteAsync(
                data,
                0,
                data.Length);
        }

        
        private static async Task<string> ReadStringAsync(
            NetworkStream stream)
        {
            int length = await ReadInt32Async(stream);

            if (length < 0 || length > 10_000_000)
            {
                throw new IOException(
                    $"Độ dài chuỗi không hợp lệ: {length}");
            }

            byte[] data = new byte[length];

            await ReadExactAsync(
                stream,
                data,
                0,
                length);

            return Encoding.UTF8.GetString(data);
        }

       
        private static async Task WriteInt32Async(
            NetworkStream stream,
            int value)
        {
            byte[] data = BitConverter.GetBytes(value);

            await stream.WriteAsync(
                data,
                0,
                data.Length);
        }

        
        private static async Task<int> ReadInt32Async(
            NetworkStream stream)
        {
            byte[] data = new byte[4];

            await ReadExactAsync(
                stream,
                data,
                0,
                4);

            return BitConverter.ToInt32(data, 0);
        }

        
        private static async Task<long> ReadInt64Async(
            NetworkStream stream)
        {
            byte[] data = new byte[8];

            await ReadExactAsync(
                stream,
                data,
                0,
                8);

            return BitConverter.ToInt64(data, 0);
        }

        
        private static async Task ReadExactAsync(
            NetworkStream stream,
            byte[] buffer,
            int offset,
            int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = await stream.ReadAsync(
                    buffer,
                    offset + totalRead,
                    count - totalRead);

                if (read == 0)
                {
                    throw new IOException(
                        "Kết nối Server đã bị đóng.");
                }

                totalRead += read;
            }
        }
    }
}