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

        // =========================================================
        // FORM LOAD
        // =========================================================
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

        // =========================================================
        // CHỌN THƯ MỤC
        // =========================================================
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

        // =========================================================
        // LÀM MỚI DANH SÁCH FILE
        // =========================================================
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

        // =========================================================
        // START SERVER
        // =========================================================
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

        // =========================================================
        // ACCEPT CLIENT
        // =========================================================
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
