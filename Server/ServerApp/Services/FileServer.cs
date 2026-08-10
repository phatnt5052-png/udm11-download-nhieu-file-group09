using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
                    await Task.Delay(100, cancellationToken);
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
    }
}