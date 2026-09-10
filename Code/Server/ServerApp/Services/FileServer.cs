using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using ServerApp.Models;

namespace ServerApp.Services
{
    public class FileServer
    {
        private readonly ServerConfig _config;
        private readonly FileService _fileService;
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public event Action<string> OnLog;

        public bool IsRunning => _isRunning;

        public FileServer(ServerConfig config)
        {
            _config = config ?? new ServerConfig();
            _fileService = new FileService(_config);
        }

        // Quản lý listener & StartServer()
        public async Task StartServerAsync()
        {
            if (_isRunning) return;

            try
            {
                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Any, _config.Port);
                _listener.Start();
                _isRunning = true;

                Log($"Server đã khởi chạy tại Port {_config.Port}. Thư mục: {_config.SharedFolder}");

                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Log($"Client kết nối từ: {client.Client.RemoteEndPoint}");

                    _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
                }
            }
            catch (ObjectDisposedException)
            {
                // Listener bị ngắt khi dừng server
            }
            catch (Exception ex)
            {
                Log($"Lỗi Server: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                try
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Đọc lệnh từ client
                        string command = await ReadStringAsync(stream);

                        if (command == "LIST")
                        {
                            // Gửi danh sách file
                            var fileList = _fileService.GetFileList();
                            await WriteInt32Async(stream, fileList.Count);

                            foreach (var fileName in fileList)
                            {
                                await WriteStringAsync(stream, fileName);
                                var fileInfo = new FileInfo(Path.Combine(_config.SharedFolder, fileName));
                                await WriteInt64Async(stream, fileInfo.Length);
                            }

                            Log($"Client đã yêu cầu danh sách file. Gửi {fileList.Count} file.");
                        }
                        else if (command == "GET")
                        {
                            // Đọc tên file mà client yêu cầu
                            string fileName = await ReadStringAsync(stream);

                            if (_fileService.FileExists(fileName))
                            {
                                // Gửi OK + tên file + kích thước
                                await WriteStringAsync(stream, "OK");
                                await WriteStringAsync(stream, fileName);

                                using (var fileStream = _fileService.OpenReadStream(fileName))
                                {
                                    long fileSize = fileStream.Length;
                                    await WriteInt64Async(stream, fileSize);

                                    // Gửi nội dung file
                                    byte[] buffer = new byte[8192];
                                    int bytesRead;

                                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                    {
                                        await stream.WriteAsync(buffer, 0, bytesRead);
                                    }

                                    Log($"Client yêu cầu file '{fileName}' ({fileSize} bytes). Đã gửi xong.");
                                }
                            }
                            else
                            {
                                // Gửi ERROR
                                await WriteStringAsync(stream, "ERROR");
                                string errorMsg = $"File '{fileName}' không tồn tại.";
                                await WriteStringAsync(stream, errorMsg);

                                Log($"Client yêu cầu file '{fileName}' không tồn tại.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Lỗi xử lý Client: {ex.Message}");
                }
            }
        }

        // StopServer()
        public void StopServer()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts?.Cancel();
            _listener?.Stop();

            Log("Server đã dừng.");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        // Helper methods để đọc/ghi dữ liệu theo protocol binary
        private static async Task WriteStringAsync(NetworkStream stream, string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            await WriteInt32Async(stream, data.Length);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<string> ReadStringAsync(NetworkStream stream)
        {
            int length = await ReadInt32Async(stream);

            if (length < 0 || length > 10_000_000)
                throw new IOException($"Độ dài chuỗi không hợp lệ: {length}");

            byte[] data = new byte[length];
            await ReadExactAsync(stream, data, 0, length);

            return System.Text.Encoding.UTF8.GetString(data);
        }

        private static async Task WriteInt32Async(NetworkStream stream, int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<int> ReadInt32Async(NetworkStream stream)
        {
            byte[] data = new byte[4];
            await ReadExactAsync(stream, data, 0, 4);
            return BitConverter.ToInt32(data, 0);
        }

        private static async Task WriteInt64Async(NetworkStream stream, long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<long> ReadInt64Async(NetworkStream stream)
        {
            byte[] data = new byte[8];
            await ReadExactAsync(stream, data, 0, 8);
            return BitConverter.ToInt64(data, 0);
        }

        private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead);

                if (read == 0)
                    throw new IOException("Kết nối Server đã bị đóng.");

                totalRead += read;
            }
        }
    }
}