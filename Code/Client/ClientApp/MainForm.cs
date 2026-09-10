using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
        private bool _isDownloadInProgress = false;

        // Kiểm tra định kỳ xem Server có còn phản hồi không, để phát hiện
        // chủ động việc mất kết nối (vd. Server bị Stop) thay vì phải đợi
        // người dùng bấm Refresh/Tải mới biết.
        private readonly System.Windows.Forms.Timer _connectionMonitorTimer = new()
        {
            Interval = 5000 // 5 giây
        };
        private bool _isMonitorTicking = false;

        // ── Drag-select (quét khối) state cho lvDownloads ────────────────
        private Point _dragStartPoint;
        private bool _isDragSelecting = false;

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

            // Quét khối (rubber-band select) ngay cả khi bắt đầu kéo từ trên 1 dòng,
            // không bắt buộc phải giữ Ctrl rồi click từng file.
            lvDownloads.MouseDown += lvDownloads_MouseDown;
            lvDownloads.MouseMove += lvDownloads_MouseMove;
            lvDownloads.MouseUp += lvDownloads_MouseUp;

            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;

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
            // Nếu đang kết nối -> Thực hiện ngắt kết nối
            if (_isConnected)
            {
                DisconnectClient();
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

            if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show(
                    "Port không hợp lệ. Vui lòng nhập trong khoảng 1 - 65535.",
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

                List<FileItem> files = await _clientService.GetFileListAsync();

                _downloadService = new DownloadService(_clientService, 3);

                lstServerFiles.Items.Clear();

                foreach (FileItem file in files)
                {
                    lstServerFiles.Items.Add(file);
                }

                SetConnectionState(true);
                _connectionMonitorTimer.Start();
            }
            catch (Exception ex)
            {
                DisconnectClient();

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
                UpdateStatusBar();
            }
        }

        // Hàm hỗ trợ dọn dẹp kết nối an toàn
        private void DisconnectClient()
        {
            _connectionMonitorTimer.Stop();

            try
            {
                _clientService?.Disconnect();
            }
            catch { }

            _clientService = null;
            _downloadService = null;

            SetConnectionState(false);
            lstServerFiles.Items.Clear();

            UpdateButtonStates();
            UpdateStatusBar();
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

        // ── Connection monitor (phát hiện chủ động mất kết nối) ──────────
        private static bool FileListsEqual(IEnumerable<FileItem> a, IEnumerable<FileItem> b)
        {
            var listA = a.Select(f => (f.FileName, f.FileSize)).OrderBy(x => x.FileName).ToList();
            var listB = b.Select(f => (f.FileName, f.FileSize)).OrderBy(x => x.FileName).ToList();
            return listA.SequenceEqual(listB);
        }

        private async void ConnectionMonitorTimer_Tick(object? sender, EventArgs e)
        {
            // Bỏ qua nếu: đang không kết nối, đang tải file, hoặc lần kiểm tra
            // trước vẫn chưa xong (tránh chồng chéo nhiều request cùng lúc).
            if (!_isConnected || _isDownloadInProgress || _isMonitorTicking || _clientService == null)
            {
                return;
            }

            _isMonitorTicking = true;

            try
            {
                List<FileItem> files = await _clientService.GetFileListAsync();

                // Chỉ cập nhật UI khi danh sách file THỰC SỰ thay đổi — tránh
                // Clear() + add lại liên tục mỗi 5 giây gây nhấp nháy dù
                // không có gì thay đổi trên Server.
                if (!FileListsEqual(lstServerFiles.Items.Cast<FileItem>(), files))
                {
                    // Ghi nhớ các file đang được chọn để chọn lại sau khi refresh
                    var selectedNames = lstServerFiles.SelectedItems
                        .Cast<FileItem>()
                        .Select(f => f.FileName)
                        .ToHashSet();

                    lstServerFiles.BeginUpdate();
                    lstServerFiles.Items.Clear();

                    foreach (FileItem file in files)
                    {
                        int index = lstServerFiles.Items.Add(file);

                        if (selectedNames.Contains(file.FileName))
                        {
                            lstServerFiles.SetSelected(index, true);
                        }
                    }

                    lstServerFiles.EndUpdate();
                }
            }
            catch
            {
                // Server không còn phản hồi -> chủ động ngắt kết nối phía UI
                // để không hiển thị danh sách file "ảo" của một kết nối đã chết.
                DisconnectClient();

                MessageBox.Show(
                    "Mất kết nối tới Server (Server có thể đã dừng).",
                    "Mất kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                _isMonitorTicking = false;
            }
        }

        // ── Refresh server file list ───────────────────────────────────
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!IsClientConnected())
            {
                DisconnectClient();
                MessageBox.Show(
                    "Chưa kết nối hoặc kết nối đến Server đã bị ngắt.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnRefresh.Enabled = false;

                List<FileItem> files = await _clientService!.GetFileListAsync();

                lstServerFiles.Items.Clear();

                foreach (FileItem file in files)
                {
                    lstServerFiles.Items.Add(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể cập nhật danh sách file.\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DisconnectClient();
            }
            finally
            {
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
            // 1. Kiểm tra kết nối trước khi tải
            if (!IsClientConnected())
            {
                DisconnectClient();
                MessageBox.Show(
                    "Vui lòng kết nối tới Server.",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Yêu cầu phải chọn ít nhất 1 file trong hàng đợi trước khi tải.
            if (lvDownloads.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Chưa chọn file nào để tải. Vui lòng chọn ít nhất 1 file trong hàng đợi.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            List<DownloadItem> queue = lvDownloads.SelectedItems
                .Cast<ListViewItem>()
                .Select(lvi => lvi.Tag as DownloadItem)
                .Where(d => d != null)
                .Cast<DownloadItem>()
                .ToList();

            if (queue.Count == 0)
            {
                MessageBox.Show(
                    "Hàng đợi đang trống. Vui lòng chọn file để thêm vào hàng đợi trước.",
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
                _isDownloadInProgress = true;

                RefreshDownloadView();

                // TẢI TUẦN TỰ TỪNG FILE (đã chọn, hoặc toàn bộ nếu không chọn)
                foreach (DownloadItem item in queue)
                {
                    // Kiểm tra kết nối lại trước mỗi file
                    if (!IsClientConnected())
                    {
                        MessageBox.Show(
                            "Kết nối tới Server đã bị ngắt! Quá trình tải xuống tạm dừng.",
                            "Mất kết nối",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        DisconnectClient();
                        break;
                    }

                    try
                    {
                        await _downloadService!.ExecuteDownloadAsync(item);
                    }
                    catch (Exception ex)
                    {
                        item.Status = "Lỗi";
                        MessageBox.Show(
                            $"Lỗi khi tải file '{item.FileName}':\n{ex.Message}",
                            "Lỗi tải file",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        if (!IsClientConnected())
                        {
                            DisconnectClient();
                            break;
                        }
                    }

                    // Cập nhật UI sau mỗi file
                    RefreshDownloadView();
                    UpdateStatusBar();
                }

                if (_isConnected)
                {
                    MessageBox.Show(
                        "Đã tải xong tất cả các file.",
                        "Download",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi xảy ra trong quá trình tải xuống.\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isDownloadInProgress = false;
                UpdateButtonStates();
                UpdateStatusBar();
            }
        }

        // Helper kiểm tra trạng thái kết nối thực tế (đã sửa lỗi cú pháp 2 dấu ';')
        private bool IsClientConnected()
        {
            return _isConnected
                && _clientService != null
                && _downloadService != null
                && _clientService.IsConnected;
        }

        // ── Refresh ListView ───────────────────────────────────────────
        private void RefreshDownloadView()
        {
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

        // ── UI state helpers ───────────────────────────────────────────
        private void UpdateButtonStates()
        {
            int queueCount = _queueService.GetQueue().Count;

            btnAdd.Enabled = _isConnected && lstServerFiles.SelectedItems.Count > 0;
            btnRefresh.Enabled = _isConnected;
            btnRemove.Enabled = lvDownloads.SelectedItems.Count > 0;
            btnDownload.Enabled = queueCount > 0;
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

        private void lstServerFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstServerFiles.SelectedItems.Count > 0)
                btnAdd_Click(sender, e);
        }

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

        private void pnlServerBtns_Resize(object sender, EventArgs e)
            => btnAdd.Location = new Point(pnlServerBtns.Width - btnAdd.Width, 8);

        private void pnlQueueBtns_Resize(object sender, EventArgs e)
            => btnDownload.Location = new Point(pnlQueueBtns.Width - btnDownload.Width, 8);

        // ── Quét khối (rubber-band select) cho lvDownloads ────────────
        // ListView mặc định chỉ quét khối được khi bắt đầu kéo từ vùng trống.
        // 3 handler dưới đây cho phép bắt đầu kéo ngay trên 1 dòng và tự
        // chọn/bỏ chọn các dòng giao với vùng đang kéo qua — không cần giữ Ctrl.
        private void lvDownloads_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _dragStartPoint = e.Location;
            _isDragSelecting = true;
        }

        private void lvDownloads_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragSelecting || e.Button != MouseButtons.Left) return;

            Rectangle selectionRect = NormalizeRectangle(_dragStartPoint, e.Location);

            // Bỏ qua những cú click đơn thuần (chưa thực sự kéo)
            if (selectionRect.Width < 4 && selectionRect.Height < 4) return;

            bool additive = ModifierKeys.HasFlag(Keys.Control) || ModifierKeys.HasFlag(Keys.Shift);

            foreach (ListViewItem item in lvDownloads.Items)
            {
                bool intersects = selectionRect.IntersectsWith(item.Bounds);

                if (intersects)
                {
                    item.Selected = true;
                }
                else if (!additive)
                {
                    item.Selected = false;
                }
            }
        }

        private void lvDownloads_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragSelecting = false;
        }

        private static Rectangle NormalizeRectangle(Point p1, Point p2)
        {
            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);
            int width = Math.Abs(p1.X - p2.X);
            int height = Math.Abs(p1.Y - p2.Y);
            return new Rectangle(x, y, width, height);
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) => btnAdd_Click(sender, e);
    }
}