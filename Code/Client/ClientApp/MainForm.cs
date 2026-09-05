using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp
{
    public partial class MainForm : Form
    {
        // ── State ──────────────────────────────────────────────────────
        private readonly DownloadQueueService _queueService = new();
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

            // Right-align buttons inside fixed-height panels at runtime
            btnAdd.Location = new Point(pnlServerBtns.Width - btnAdd.Width, 8);
            btnDownload.Location = new Point(pnlQueueBtns.Width - btnDownload.Width, 8);

            UpdateButtonStates();
            UpdateStatusBar();

            // SplitterDistance được tính tự động dựa trên Panel1MinSize, Panel2MinSize
            // và kích thước form. Không cần gán thủ công.
        }

        // ── Connection ─────────────────────────────────────────────────
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                // TODO: Thay thế bằng kết nối TCP thực
                SetConnectionState(true);

                // Giả lập danh sách file server (thay bằng lệnh gọi thực tế)
                lstServerFiles.Items.Clear();
                lstServerFiles.Items.Add(new FileItem("document.pdf", 1_048_576));
                lstServerFiles.Items.Add(new FileItem("video.mp4", 52_428_800));
                lstServerFiles.Items.Add(new FileItem("archive.zip", 10_485_760));
                lstServerFiles.Items.Add(new FileItem("image.png", 524_288));
                lstServerFiles.Items.Add(new FileItem("data.csv", 204_800));
            }
            else
            {
                SetConnectionState(false);
                lstServerFiles.Items.Clear();
            }

            UpdateButtonStates();
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
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show(
                    "Vui lòng kết nối đến server trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // TODO: Gọi service thực tế để lấy danh sách file
            MessageBox.Show("Đã tải lại danh sách file.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (_queueService.GetQueue().Count == 0)
            {
                MessageBox.Show("Hàng đợi đang trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: Triển khai logic tải TCP thực tế ở đây
            MessageBox.Show("▶ Bắt đầu tải xuống!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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