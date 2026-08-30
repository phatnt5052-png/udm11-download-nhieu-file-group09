using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    public partial class ServerMainForm : Form
    {
        private TcpListener? server;
        private CancellationTokenSource? cancellationTokenSource;

        private bool isRunning = false;

        public ServerMainForm()
        {
            InitializeComponent();
        }

        // FORM LOAD
        private void ServerMainForm_Load(object sender, EventArgs e)
        {
            txtFolder.Text = Path.Combine(Application.StartupPath, "ServerFiles");

            if (!Directory.Exists(txtFolder.Text))
            {
                Directory.CreateDirectory(txtFolder.Text);
            }

            RefreshFileList();

            lblStatus.Text = "Server Offline";
            lblStatus.ForeColor = Color.Red;

            btnStart.Enabled = true;
            btnStop.Enabled = false;

            AddLog("Server đã sẵn sàng.");
        }

        // CHỌN THƯ MỤC
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "Chọn thư mục chứa file trên Server";

            if (Directory.Exists(txtFolder.Text))
            {
                dialog.SelectedPath = txtFolder.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dialog.SelectedPath;

                RefreshFileList();

                AddLog("Đã chọn thư mục: " + txtFolder.Text);
            }
        }

        // LÀM MỚI DANH SÁCH FILE
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshFileList();
        }

        private void RefreshFileList()
        {
            try
            {
                lvFiles.Items.Clear();

                string folder = txtFolder.Text.Trim();

                if (string.IsNullOrWhiteSpace(folder))
                {
                    return;
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string[] files = Directory.GetFiles(
                    folder,
                    "*",
                    SearchOption.TopDirectoryOnly
                );

                foreach (string file in files)
                {
                    FileInfo info = new FileInfo(file);

                    ListViewItem item = new ListViewItem(info.Name);

                    item.SubItems.Add(FormatFileSize(info.Length));
                    item.SubItems.Add(info.FullName);

                    item.Tag = info.FullName;

                    lvFiles.Items.Add(item);
                }

                AddLog($"Đã cập nhật danh sách: {files.Length} file.");
            }
            catch (Exception ex)
            {
                AddLog("Lỗi đọc danh sách file: " + ex.Message);
            }
        }

        // START SERVER
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                return;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show(
                    "Port không hợp lệ!",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            if (port < 1 || port > 65535)
            {
                MessageBox.Show(
                    "Port phải nằm trong khoảng 1 - 65535!",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            try
            {
                string folder = txtFolder.Text.Trim();

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                RefreshFileList();

                cancellationTokenSource = new CancellationTokenSource();

                server = new TcpListener(
                    IPAddress.Any,
                    port
                );

                server.Start();

                isRunning = true;

                lblStatus.Text = $"Server Online - Port {port}";
                lblStatus.ForeColor = Color.Green;

                btnStart.Enabled = false;
                btnStop.Enabled = true;

                txtPort.Enabled = false;
                txtFolder.Enabled = false;
                btnBrowse.Enabled = false;

                AddLog($"Server đã chạy tại port {port}.");
                AddLog("Đang chờ Client kết nối...");

                await AcceptClientsAsync(cancellationTokenSource.Token);
            }
            catch (SocketException ex)
            {
                AddLog("Lỗi Socket: " + ex.Message);

                StopServer();
            }
            catch (Exception ex)
            {
                AddLog("Lỗi Server: " + ex.Message);

                StopServer();
            }
        }

        // ACCEPT CLIENT
        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && server != null)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync(token);

                    _ = Task.Run(
                        () => HandleClientAsync(client),
                        token
                    );
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        AddLog("Lỗi nhận Client: " + ex.Message);
                    }
                }
            }
        }

        // XỬ LÝ CLIENT
        private async Task HandleClientAsync(TcpClient client)
        {
            string clientName = "Unknown";

            try
            {
                clientName = client.Client.RemoteEndPoint?.ToString()
                             ?? "Unknown";

                AddLog($"Client kết nối: {clientName}");

                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        string? command;

                        try
                        {
                            command = await ReadStringAsync(stream);
                        }
                        catch
                        {
                            break;
                        }

                        if (command == null)
                        {
                            break;
                        }

                        command = command.ToUpperInvariant();

                        // CLIENT YÊU CẦU DANH SÁCH FILE
                        if (command == "LIST")
                        {
                            await SendFileListAsync(stream);
                        }

                        // CLIENT YÊU CẦU DOWNLOAD
                        else if (command == "GET")
                        {
                            string? fileName =
                                await ReadStringAsync(stream);

                            if (string.IsNullOrWhiteSpace(fileName))
                            {
                                await WriteStringAsync(
stream,
                                    "ERROR"
                                );

                                await WriteStringAsync(
                                    stream,
                                    "Tên file không hợp lệ."
                                );

                                continue;
                            }

                            await SendFileAsync(
                                stream,
                                fileName,
                                clientName
                            );
                        }

                        // CLIENT GỬI QUIT
                        else if (command == "QUIT")
                        {
                            break;
                        }

                        // COMMAND KHÔNG HỢP LỆ
                        else
                        {
                            await WriteStringAsync(
                                stream,
                                "ERROR"
                            );

                            await WriteStringAsync(
                                stream,
                                "Command không hợp lệ."
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog(
                    $"Client {clientName} lỗi: {ex.Message}"
                );
            }
            finally
            {
                AddLog(
                    $"Client ngắt kết nối: {clientName}"
                );
            }
        }

        // GỬI DANH SÁCH FILE
        private async Task SendFileListAsync(
            NetworkStream stream)
        {
            try
            {
                string folder = txtFolder.Text.Trim();

                if (!Directory.Exists(folder))
                {
                    await WriteInt32Async(stream, 0);
                    return;
                }

                FileInfo[] files = new DirectoryInfo(folder)
                    .GetFiles("*", SearchOption.TopDirectoryOnly);

                await WriteInt32Async(
                    stream,
                    files.Length
                );

                foreach (FileInfo file in files)
                {
                    await WriteStringAsync(
                        stream,
                        file.Name
                    );

                    await WriteInt64Async(
                        stream,
                        file.Length
);
                }

                AddLog(
                    $"Đã gửi danh sách {files.Length} file cho Client."
                );
            }
            catch (Exception ex)
            {
                AddLog(
                    "Lỗi gửi danh sách file: " + ex.Message
                );
            }
        }

        // GỬI FILE
        private async Task SendFileAsync(
            NetworkStream stream,
            string fileName,
            string clientName)
        {
            try
            {
                string folder = Path.GetFullPath(
                    txtFolder.Text.Trim()
                );

                string safeFileName = Path.GetFileName(
                    fileName
                );

                string fullPath = Path.Combine(
                    folder,
                    safeFileName
                );

                // Chống truy cập ra ngoài thư mục Server
                string fullPathNormalized =
                    Path.GetFullPath(fullPath);

                if (!fullPathNormalized.StartsWith(
                    folder,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await WriteStringAsync(
                        stream,
                        "ERROR"
                    );

                    await WriteStringAsync(
                        stream,
                        "File không hợp lệ."
                    );

                    return;
                }

                if (!File.Exists(fullPath))
                {
                    await WriteStringAsync(
                        stream,
                        "ERROR"
                    );

                    await WriteStringAsync(
                        stream,
                        "Không tìm thấy file."
                    );

                    AddLog(
                        $"Client {clientName} yêu cầu file không tồn tại: {fileName}"
                    );

                    return;
                }

                FileInfo fileInfo = new FileInfo(fullPath);

                // Gửi trạng thái OK
                await WriteStringAsync(
                    stream,
                    "OK"
                );

                // Gửi tên file
                await WriteStringAsync(
                    stream,
                    fileInfo.Name
                );

                // Gửi kích thước
                await WriteInt64Async(
                    stream,
                    fileInfo.Length
                );

                AddLog(
                    $"Bắt đầu gửi {fileInfo.Name} cho {clientName}."
                );

                byte[] buffer = new byte[64 * 1024];

                long totalSent = 0;

                using FileStream fileStream =
                    new FileStream(
fullPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 64 * 1024,
                        useAsync: true
                    );

                int bytesRead;

                while (
                    (bytesRead = await fileStream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length
                    )) > 0)
                {
                    await stream.WriteAsync(
                        buffer,
                        0,
                        bytesRead
                    );

                    totalSent += bytesRead;
                }

                await stream.FlushAsync();

                AddLog(
                    $"Đã gửi xong {fileInfo.Name} " +
                    $"({FormatFileSize(totalSent)}) " +
                    $"cho {clientName}."
                );
            }
            catch (Exception ex)
            {
                AddLog(
                    $"Lỗi gửi file {fileName}: {ex.Message}"
                );
            }
        }

        // STOP SERVER
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StopServer()
        {
            try
            {
                isRunning = false;

                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;

                server?.Stop();
                server = null;

                lblStatus.Text = "Server Offline";
                lblStatus.ForeColor = Color.Red;

                btnStart.Enabled = true;
                btnStop.Enabled = false;

                txtPort.Enabled = true;
                txtFolder.Enabled = true;
                btnBrowse.Enabled = true;

                AddLog("Server đã dừng.");
            }
            catch (Exception ex)
            {
                AddLog(
                    "Lỗi khi dừng Server: " + ex.Message
                );
            }
        }

        // FORM CLOSING
        private void ServerMainForm_FormClosing(
            object? sender,
            FormClosingEventArgs e)
        {
            StopServer();
        }

        // NETWORK STRING
        private static async Task WriteStringAsync(
            NetworkStream stream,
            string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            await WriteInt32Async(
stream,
                data.Length
            );

            await stream.WriteAsync(
                data,
                0,
                data.Length
            );
        }

        private static async Task<string?> ReadStringAsync(
            NetworkStream stream)
        {
            int length = await ReadInt32Async(stream);

            if (length < 0 || length > 10_000_000)
            {
                return null;
            }

            byte[] data = new byte[length];

            await ReadExactAsync(
                stream,
                data,
                0,
                length
            );

            return Encoding.UTF8.GetString(data);
        }

        // NETWORK INT32
        private static async Task WriteInt32Async(
            NetworkStream stream,
            int value)
        {
            byte[] data = BitConverter.GetBytes(value);

            await stream.WriteAsync(
                data,
                0,
                data.Length
            );
        }

        private static async Task<int> ReadInt32Async(
            NetworkStream stream)
        {
            byte[] data = new byte[4];

            await ReadExactAsync(
                stream,
                data,
                0,
                4
            );

            return BitConverter.ToInt32(data, 0);
        }

        // NETWORK INT64
        private static async Task WriteInt64Async(
            NetworkStream stream,
            long value)
        {
            byte[] data = BitConverter.GetBytes(value);

            await stream.WriteAsync(
                data,
                0,
                data.Length
            );
        }

        private static async Task<long> ReadInt64Async(
            NetworkStream stream)
        {
            byte[] data = new byte[8];

            await ReadExactAsync(
                stream,
                data,
                0,
                8
            );

            return BitConverter.ToInt64(data, 0);
        }

        // READ ĐỦ SỐ BYTE
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
                    count - totalRead
                );

                if (read == 0)
                {
                    throw new IOException(
"Connection đã bị đóng."
                    );
                }

                totalRead += read;
            }
        }

        // FORMAT FILE SIZE
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes + " B";
            }

            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024.0:F2} KB";
            }

            if (bytes < 1024L * 1024L * 1024L)
            {
                return $"{bytes / (1024.0 * 1024.0):F2} MB";
            }

            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        // LOG
        private void AddLog(string message)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    Invoke(
                        new Action<string>(AddLog),
                        message
                    );
                }
                catch
                {
                    // Form đã đóng
                }

                return;
            }

            string log =
                $"[{DateTime.Now:HH:mm:ss}] {message}";

            txtLog.AppendText(
                log + Environment.NewLine
            );
        }
    }
}
