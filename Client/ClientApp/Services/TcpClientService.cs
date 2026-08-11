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

            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8);

            await writer.WriteLineAsync("LIST");
            string countStr = await reader.ReadLineAsync();

            if (int.TryParse(countStr, out int count))
            {
                for (int i = 0; i < count; i++)
                {
                    string fileLine = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(fileLine))
                    {
                        string[] parts = fileLine.Split('|');
                        if (parts.Length == 2 && long.TryParse(parts[1], out long size))
                        {
                            list.Add(new FileItem(parts[0], size));
                        }
                    }
                }
            }
            return list;
        }

        public async Task DownloadFileFromServerAsync(string fileName, Func<Stream, long, Task> dataHandler)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_ip, _port);

            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8);

            await writer.WriteLineAsync($"GET {fileName}");
            string response = await reader.ReadLineAsync();

            if (response != null && response.StartsWith("OK|"))
            {
                long fileSize = long.Parse(response.Split('|')[1]);
                await dataHandler(stream, fileSize);
            }
            else
            {
                throw new FileNotFoundException(response ?? "Không có phản hồi từ máy chủ");
            }
        }
    }
}
