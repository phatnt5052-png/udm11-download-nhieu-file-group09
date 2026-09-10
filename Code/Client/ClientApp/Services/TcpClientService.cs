using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Services
{
    public class TcpClientService
    {
        private readonly string _ip;
        private readonly int _port;

        // Cờ trạng thái "đang kết nối" theo góc nhìn logic của Client.
        // Vì mỗi request (LIST/GET) tự mở 1 TcpClient riêng rồi đóng lại,
        // đây KHÔNG phải một socket đang mở liên tục — mà là kết quả của
        // lần giao tiếp thành công/thất bại GẦN NHẤT với Server:
        //   - true  ngay sau khi 1 request hoàn tất thành công
        //   - false ngay khi 1 request thất bại (mất kết nối / server tắt)
        // Nhờ vậy MainForm.IsClientConnected() phát hiện đúng tình trạng
        // mất kết nối thay vì giữ mãi trạng thái "đã kết nối" đã lỗi thời.
        public bool IsConnected { get; private set; }

        public TcpClientService(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        // Server không phản hồi trong khoảng thời gian này thì coi như mất kết nối
        // và báo lỗi ngay, thay vì để hệ điều hành tự chờ TCP timeout (có thể rất lâu).
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

        public async Task<List<FileItem>> GetFileListAsync()
        {
            try
            {
                var list = new List<FileItem>();

                using var client = new TcpClient();
                using var connectCts = new CancellationTokenSource(ConnectTimeout);

                try
                {
                    await client.ConnectAsync(_ip, _port, connectCts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new IOException($"Không thể kết nối tới Server {_ip}:{_port} (hết thời gian chờ).");
                }

                using NetworkStream stream = client.GetStream();

                await WriteStringAsync(stream, "LIST");

                int count = await ReadInt32Async(stream);

                if (count < 0)
                {
                    throw new IOException("Số lượng file từ Server không hợp lệ.");
                }

                for (int i = 0; i < count; i++)
                {
                    string fileName = await ReadStringAsync(stream);
                    long fileSize = await ReadInt64Async(stream);

                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        list.Add(new FileItem(fileName, fileSize));
                    }
                }

                IsConnected = true;
                return list;
            }
            catch
            {
                IsConnected = false;
                throw;
            }
        }

        public async Task DownloadFileFromServerAsync(
            string fileName,
            Func<Stream, long, Task> dataHandler)
        {
            try
            {
                using var client = new TcpClient();
                using var connectCts = new CancellationTokenSource(ConnectTimeout);

                try
                {
                    await client.ConnectAsync(_ip, _port, connectCts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new IOException($"Không thể kết nối tới Server {_ip}:{_port} (hết thời gian chờ).");
                }

                using NetworkStream stream = client.GetStream();

                await WriteStringAsync(stream, "GET");
                await WriteStringAsync(stream, fileName);

                string response = await ReadStringAsync(stream);

                if (response == "OK")
                {
                    string serverFileName = await ReadStringAsync(stream);
                    long fileSize = await ReadInt64Async(stream);

                    if (fileSize < 0)
                    {
                        throw new IOException("Kích thước file từ Server không hợp lệ.");
                    }

                    await dataHandler(stream, fileSize);
                    IsConnected = true;
                }
                else if (response == "ERROR")
                {
                    string errorMessage = await ReadStringAsync(stream);

                    // Lỗi "không tìm thấy file" không có nghĩa là mất kết nối
                    // tới Server — Server vẫn đang phản hồi bình thường.
                    IsConnected = true;
                    throw new FileNotFoundException(errorMessage);
                }
                else
                {
                    throw new IOException($"Server trả về phản hồi không hợp lệ: {response}");
                }
            }
            catch (FileNotFoundException)
            {
                throw; // Đã set IsConnected = true ở trên, không phải lỗi mất kết nối
            }
            catch
            {
                IsConnected = false;
                throw;
            }
        }

        // Đóng "phiên" logic hiện tại. Vì Client không giữ 1 socket mở liên tục,
        // hàm này chỉ cần hạ cờ trạng thái để UI cập nhật đúng ngay lập tức.
        public void Disconnect()
        {
            IsConnected = false;
        }

        private static async Task WriteStringAsync(NetworkStream stream, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            await WriteInt32Async(stream, data.Length);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<string> ReadStringAsync(NetworkStream stream)
        {
            int length = await ReadInt32Async(stream);

            if (length < 0 || length > 10_000_000)
            {
                throw new IOException($"Độ dài chuỗi không hợp lệ: {length}");
            }

            byte[] data = new byte[length];
            await ReadExactAsync(stream, data, 0, length);
            return Encoding.UTF8.GetString(data);
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
                {
                    throw new IOException("Kết nối Server đã bị đóng.");
                }

                totalRead += read;
            }
        }
    }
}