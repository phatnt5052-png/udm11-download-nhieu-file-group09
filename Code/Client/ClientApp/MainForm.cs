using ClientApp.Models;
using ClientApp.Services;
using ClientApp.Helpers;

namespace ClientApp
{
    public partial class MainForm : Form
    {
        // ── State ──────────────────────────────────────────────────────
        private readonly DownloadQueueService _queueService = new();
        private TcpClientService? _clientService;
        private DownloadService? _downloadService;
        private bool _isConnected = false;

        // ── Status row colors ──────────────────────────────────────────
        private static readonly Color ClrPending = Color.FromArgb(250, 251, 252);
        private static readonly Color ClrDownloading = Color.FromArgb(214, 234, 248);
        private static readonly Color ClrCompleted = Color.FromArgb(213, 245, 227);
        private static readonly Color ClrError = Color.FromArgb(250, 219, 216);

        // ── Constructor ────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Cho phép chọn nhiều file Server
            lstServerFiles.SelectionMode = SelectionMode.MultiExtended;

            // Cho phép chọn nhiều file trong hàng đợi
            lvDownloads.MultiSelect = true;
            lvDownloads.HideSelection = false;

            // === Tạo nút "Mở thư mục" ===
            Button btnOpenFolder = new Button
            {
                Text = "📁  Mở",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(70, 32),
                Location = new Point(5, 8),
                Cursor = Cursors.Hand
            };
            btnOpenFolder.FlatAppearance.BorderColor = Color.FromArgb(155, 89, 182);
            btnOpenFolder.Click += (s, e) => FolderHelper.OpenDownloadsFolder();
            pnlServerBtns.Controls.Add(btnOpenFolder);

            // Cập nhật vị trí btnRefresh để không bị đè
            btnRefresh.Location = new Point(80, 8);

            btnAdd.Location = new Point(
                pnlServerBtns.Width - btnAdd.Width,
                8);

            btnDownload.Location = new Point(
                pnlQueueBtns.Width - btnDownload.Width,
                8);

            UpdateButtonStates();
            UpdateStatusBar();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                _clientService = null;
                _downloadService = null;

                SetConnectionState(false);

                lstServerFiles.Items.Clear();

                UpdateButtonStates();
                UpdateStatusBar();

                return;
            }

            string ip = txtServerIp.Text.Trim();
            string portText = txtPort.Text.Trim();

            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show(
                    "Vui lòng nhập địa chỉ IP của Server.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                txtServerIp.Focus();
                return;
            }

            if (!int.TryParse(portText, out int port))
            {
                MessageBox.Show(
                    "Port không hợp lệ.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPort.Focus();
                return;
            }

            if (port < 1 || port > 65535)
            {
                MessageBox.Show(
                    "Port phải nằm trong khoảng 1 - 65535.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPort.Focus();
                return;
            }

            try
            {
                btnConnect.Enabled = false;

                _clientService = new TcpClientService(ip, port);

                List<FileItem> files =
                    await _clientService.GetFileListAsync();

                _downloadService =
                    new DownloadService(_clientService, 3);

                lstServerFiles.Items.Clear();

                foreach (FileItem file in files)
                {
                    lstServerFiles.Items.Add(file);
                }

                SetConnectionState(true);

                UpdateButtonStates();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                _clientService = null;
                _downloadService = null;

                SetConnectionState(false);

                lstServerFiles.Items.Clear();

                MessageBox.Show(
                    "Không thể kết nối đến Server.\n\n" +
                    $"Địa chỉ: {ip}:{port}\n" +
                    $"Chi tiết: {ex.Message}",
                    "Kết nối thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnConnect.Enabled = true;
                UpdateButtonStates();
            }
        }

        private void SetConnectionState(bool connected)
        {
            _isConnected = connected;

            if (connected)
            {
                lblStatusDot.ForeColor = Color.FromArgb(39, 174, 96);   // Green
                lblStatusText.Text = $"Đã kết nối: {txtServerIp.Text}:{txtPort.Text}";
                lblStatusText.ForeColor = Color.FromArgb(174, 214, 241);
                lblStatusText.Font = new Font("Segoe UI", 9f);
                btnConnect.Text = "⏏  Ngắt kết nối";
                btnConnect.BackColor = Color.FromArgb(192, 57, 43);
                btnConnect.FlatAppearance.BorderColor = Color.FromArgb(192, 57, 43);
                tsslStatus.Text = "🟢  Đã kết nối";
                tsslStatus.ForeColor = Color.FromArgb(88, 214, 141);
                txtServerIp.Enabled = false;
                txtPort.Enabled = false;
            }
            else
            {
                lblStatusDot.ForeColor = Color.FromArgb(100, 100, 100); // Gray
                lblStatusText.Text = "Chưa kết nối";
                lblStatusText.ForeColor = Color.FromArgb(127, 140, 141);
                lblStatusText.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
                btnConnect.Text = "🔌  Kết nối";
                btnConnect.BackColor = Color.FromArgb(52, 152, 219);
                btnConnect.FlatAppearance.BorderColor = Color.FromArgb(52, 152, 219);
                tsslStatus.Text = "⚫  Chưa kết nối";
                tsslStatus.ForeColor = Color.FromArgb(127, 140, 141);
                txtServerIp.Enabled = true;
                txtPort.Enabled = true;
            }
        }

        // ── Refresh server file list ───────────────────────────────────
            private async void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!_isConnected || _clientService == null)
            {
                MessageBox.Show(
                    "Vui lòng kết nối đến server trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                btnRefresh.Enabled = false;

                List<FileItem> files =
                    await _clientService.GetFileListAsync();

                lstServerFiles.Items.Clear();

                foreach (FileItem file in files)
                {
                    lstServerFiles.Items.Add(file);
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể cập nhật danh sách file.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _clientService = null;
                _downloadService = null;

                SetConnectionState(false);

                lstServerFiles.Items.Clear();
            }
            finally
            {
                btnRefresh.Enabled = _isConnected;
                UpdateButtonStates();
            }
        }

        // ── Add to queue ───────────────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lstServerFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn ít nhất một file để thêm vào hàng đợi.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            int addedCount = 0;
            int duplicateCount = 0;

            foreach (var selectedItem in lstServerFiles.SelectedItems)
            {
                if (selectedItem is not FileItem file) continue;

                var item = new DownloadItem(file.FileName, file.FileSize);

                if (_queueService.AddToQueue(item))
                    addedCount++;
                else
                    duplicateCount++;
            }

            RefreshDownloadView();

            string msg = $"✅ Đã thêm {addedCount} file vào hàng đợi.";
            if (duplicateCount > 0)
                msg += $"\n⚠️ {duplicateCount} file đã có trong hàng đợi.";

            MessageBox.Show(msg, "Hàng đợi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateButtonStates();
            UpdateStatusBar();
        }

        // ── Remove from queue ──────────────────────────────────────────
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lvDownloads.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn file cần xóa.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var toRemove = lvDownloads.SelectedItems
                .Cast<ListViewItem>()
                .Select(lvi => lvi.Tag as DownloadItem)
                .Where(d => d != null)
                .ToList();

            foreach (var item in toRemove)
                _queueService.RemoveFromQueue(item!.FileName);

            RefreshDownloadView();
            UpdateButtonStates();
            UpdateStatusBar();
        }

        // ── Start download ─────────────────────────────────────────────
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (!_isConnected || _downloadService == null)
            {
                MessageBox.Show(
                    "Vui lòng kết nối đến Server trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            List<DownloadItem> queue =
                _queueService.GetQueue();

            if (queue.Count == 0)
            {
                MessageBox.Show(
                    "Hàng đợi đang trống.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                // Khóa các nút trong lúc download
                btnDownload.Enabled = false;
                btnAdd.Enabled = false;
                btnRemove.Enabled = false;
                btnRefresh.Enabled = false;

                RefreshDownloadView();

                // TẢI TUẦN TỰ TỪNG FILE
                foreach (DownloadItem item in queue)
                {
                    await _downloadService.ExecuteDownloadAsync(item);

                    // Cập nhật UI sau mỗi file
                    RefreshDownloadView();
                    UpdateStatusBar();
                }

                MessageBox.Show(
                    "Đã xử lý xong hàng đợi tải xuống.",
                    "Download",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi trong quá trình tải xuống.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnDownload.Enabled =
                    _isConnected &&
                    _queueService.GetQueue().Count > 0;

                btnAdd.Enabled =
                    _isConnected &&
                    lstServerFiles.SelectedItems.Count > 0;

                btnRemove.Enabled =
                    lvDownloads.SelectedItems.Count > 0;

                btnRefresh.Enabled =
                    _isConnected;

                UpdateButtonStates();
                UpdateStatusBar();
            }
        }
        // ── Refresh ListView ───────────────────────────────────────────
        private void RefreshDownloadView()
        {
            // Sync hidden lstDownloadQueue (để tương thích với _queueService)
            lstDownloadQueue.Items.Clear();

            lvDownloads.BeginUpdate();
            lvDownloads.Items.Clear();

            foreach (var item in _queueService.GetQueue())
            {
                lstDownloadQueue.Items.Add(item);

                var row = new ListViewItem(item.FileName);
                row.SubItems.Add(item.Status);
                row.SubItems.Add($"{item.Progress:F1}%");
                row.SubItems.Add($"{item.SpeedMbps:F2} MB/s");
                row.BackColor = StatusColor(item.Status);
                row.Tag = item;

                lvDownloads.Items.Add(row);
            }

            lvDownloads.EndUpdate();
        }

        private static Color StatusColor(string status) => status switch
        {
            "Downloading" => ClrDownloading,
            "Completed" => ClrCompleted,
            "Failed" => ClrError,
            "Waiting" => ClrPending,
            "Đang tải" => ClrDownloading,
            "Hoàn thành" => ClrCompleted,
            "Lỗi" => ClrError,

            _ => ClrPending
        };

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1_024) return $"{bytes / 1_024.0:F1} KB";
            return $"{bytes} B";
        }

        // ── UI state helpers ───────────────────────────────────────────
        private void UpdateButtonStates()
        {
            int queueCount = _queueService.GetQueue().Count;

            btnAdd.Enabled = _isConnected && lstServerFiles.SelectedItems.Count > 0;
            btnRefresh.Enabled = _isConnected;
            btnRemove.Enabled = lvDownloads.SelectedItems.Count > 0;
            btnDownload.Enabled = _isConnected && queueCount > 0;
        }

        private void UpdateStatusBar()
        {
            int count = _queueService.GetQueue().Count;
            tsslQueue.Text = $"  |  Hàng đợi: {count} file";
        }

        // ── Event handlers ─────────────────────────────────────────────
        private void lstServerFiles_SelectedIndexChanged(object sender, EventArgs e)
            => UpdateButtonStates();

        private void lvDownloads_SelectedIndexChanged(object sender, EventArgs e)
            => UpdateButtonStates();

        // Double-click on server file → add to queue
        private void lstServerFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstServerFiles.SelectedItems.Count > 0)
                btnAdd_Click(sender, e);
        }

        // Keyboard shortcuts
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    btnRefresh_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.Delete when lvDownloads.Focused:
                    btnRemove_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.Enter when lstServerFiles.Focused:
                    btnAdd_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }

        // Resize → keep right-aligned buttons flush to panel edge
        private void pnlServerBtns_Resize(object sender, EventArgs e)
            => btnAdd.Location = new Point(pnlServerBtns.Width - btnAdd.Width, 8);

        private void pnlQueueBtns_Resize(object sender, EventArgs e)
            => btnDownload.Location = new Point(pnlQueueBtns.Width - btnDownload.Width, 8);

        // Legacy stubs (kept for compatibility)
        private void label1_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) => btnAdd_Click(sender, e);
    }
}