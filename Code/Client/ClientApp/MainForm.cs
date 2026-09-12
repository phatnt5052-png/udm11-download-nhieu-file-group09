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
        // ── Colors ─────────────────────────────────────────────────────
        private static readonly Color ClrBrand = Color.FromArgb(41, 128, 185);
        private static readonly Color ClrGreen = Color.FromArgb(39, 174, 96);
        private static readonly Color ClrRed = Color.FromArgb(192, 57, 43);
        private static readonly Color ClrGray = Color.FromArgb(127, 140, 141);
        private static readonly Color ClrBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color ClrRowError = Color.FromArgb(250, 219, 216);

        // ── Services ───────────────────────────────────────────────────
        private TcpClientService? _clientService;
        private DownloadService? _downloadService;
        private bool _isConnected = false;
        private bool _isDownloadInProgress = false;
        private System.Threading.CancellationTokenSource? _downloadCts;

        private readonly Dictionary<string, DownloadItem> _items = new();
        private readonly HashSet<string> _hiddenFiles = new();

        // ── Controls Top ───────────────────────────────────────────────
        private Panel pnlTop = null!;
        private Label lblBrand = null!;
        private Label lblIp = null!;
        private Label lblPort = null!;
        private TextBox txtServerIp = null!;
        private TextBox txtPort = null!;
        private Button btnConnect = null!;
        private Button btnDisconnect = null!;
        private Button btnTestConnection = null!;
        private Button btnOpenFolder = null!;
        private Button btnViewServerFiles = null!;
        private Label lblStatusDot = null!;
        private Label lblStatusText = null!;

        // ── Controls Grid & Bottom ─────────────────────────────────────
        private DataGridView dgv = null!;
        private DataGridViewTextBoxColumn colType = null!;
        private DataGridViewTextBoxColumn colName = null!;
        private DataGridViewTextBoxColumn colSize = null!;
        private DataGridViewTextBoxColumn colStatus = null!;
        private DataGridViewTextBoxColumn colProgress = null!;
        private DataGridViewTextBoxColumn colTransferred = null!;
        private DataGridViewTextBoxColumn colSpeed = null!;
        private DataGridViewButtonColumn colRetry = null!;
        private DataGridViewButtonColumn colDelete = null!;

        private Label lblSummary = null!;
        private Button btnSelectAll = null!;
        private Button btnDeleteSelected = null!;
        private Button btnDeleteAll = null!;
        private Button btnRetryFailed = null!;
        private Button btnDownloadSelected = null!;

        private readonly System.Windows.Forms.Timer _connectionMonitorTimer = new() { Interval = 5000 };
        private readonly System.Windows.Forms.Timer _progressRefreshTimer = new() { Interval = 300 };
        private bool _isMonitorTicking = false;

        // ── Constructor ────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();
            BuildUi();

            _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
            _progressRefreshTimer.Tick += (s, e) => RefreshAllRowVisuals();

            UpdateConnectionUi();
            UpdateSummary();
        }

        // ══════════════════════════════════════════════════════════════
        //  UI CONSTRUCTION
        // ══════════════════════════════════════════════════════════════
        private void BuildUi()
        {
            // ── Top bar ───────────────────────────────────────────────
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };

            lblBrand = new Label
            {
                Text = "⬇  UDM_11 MultiFileDownload",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ClrBrand,
                AutoSize = true
            };

            lblIp = new Label { Text = "IP:", AutoSize = true };
            txtServerIp = new TextBox { Text = "127.0.0.1", Width = 100 };

            lblPort = new Label { Text = "Port:", AutoSize = true };
            txtPort = new TextBox { Text = "5000", Width = 55 };

            btnConnect = new Button
            {
                Text = "Kết nối",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = ClrBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnConnect.FlatAppearance.BorderColor = ClrBlue;
            btnConnect.Click += btnConnect_Click;

            btnDisconnect = new Button
            {
                Text = "Ngắt kết nối",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = ClrRed,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnDisconnect.FlatAppearance.BorderColor = ClrRed;
            btnDisconnect.Click += (s, e) => DisconnectClient();

            btnTestConnection = new Button
            {
                Text = "Test kết nối",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnTestConnection.FlatAppearance.BorderColor = Color.FromArgb(149, 165, 166);
            btnTestConnection.Click += async (s, e) => await TestConnectionAsync();

            btnOpenFolder = new Button
            {
                Text = "Mở thư mục",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnOpenFolder.FlatAppearance.BorderColor = Color.FromArgb(155, 89, 182);
            btnOpenFolder.Click += (s, e) =>
            {
                try { FolderHelper.OpenDownloadsFolder(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            btnViewServerFiles = new Button
            {
                Text = "Xem file trên Server",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnViewServerFiles.FlatAppearance.BorderColor = Color.FromArgb(41, 128, 185);
            btnViewServerFiles.Click += async (s, e) => await ShowServerFilesDialogAsync();

            lblStatusDot = new Label { Text = "●", Font = new Font("Segoe UI", 12f), AutoSize = true, ForeColor = ClrGray };
            lblStatusText = new Label { Text = "Chưa kết nối", AutoSize = true, ForeColor = ClrGray };

            pnlTop.Controls.AddRange(new Control[]
            {
                lblBrand, lblIp, txtServerIp, lblPort, txtPort,
                btnConnect, btnDisconnect, btnTestConnection, btnOpenFolder, btnViewServerFiles,
                lblStatusDot, lblStatusText
            });

            pnlTop.Resize += (s, e) => LayoutTopControls();
            LayoutTopControls();

            var topBorder = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(220, 220, 220) };

            // ── Bottom bar ────────────────────────────────────────────
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.White };
            var bottomBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(220, 220, 220) };

            lblSummary = new Label
            {
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(16, 0),
                Size = new Size(200, 50),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            btnSelectAll = new Button
            {
                Text = "Chọn tất cả",
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnSelectAll.Click += (s, e) => ToggleSelectAll();

            btnRetryFailed = new Button
            {
                Text = "↻ Thử lại",
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnRetryFailed.Click += async (s, e) => await DownloadItemsAsync(_items.Values.Where(i => i.Status == DownloadStatus.Failed).ToList());

            btnDeleteSelected = new Button
            {
                Text = "Xóa",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnDeleteSelected.FlatAppearance.BorderColor = Color.FromArgb(230, 126, 34);
            btnDeleteSelected.Click += (s, e) => DeleteSelectedRows();

            btnDeleteAll = new Button
            {
                Text = "Xóa tất cả",
                FlatStyle = FlatStyle.Flat,
                BackColor = ClrRed,
                ForeColor = Color.White,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 30,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnDeleteAll.FlatAppearance.BorderColor = ClrRed;
            btnDeleteAll.Click += (s, e) => DeleteAllRows();

            btnDownloadSelected = new Button
            {
                Text = "Tải các file đã chọn",
                FlatStyle = FlatStyle.Flat,
                BackColor = ClrGreen,
                ForeColor = Color.White,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(14, 0, 14, 0),
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnDownloadSelected.FlatAppearance.BorderColor = ClrGreen;
            btnDownloadSelected.Click += async (s, e) =>
            {
                if (_isDownloadInProgress)
                {
                    _downloadCts?.Cancel();
                }
                else
                {
                    await btnDownloadSelected_Click();
                }
            };

            pnlBottom.Controls.AddRange(new Control[] { lblSummary, btnSelectAll, btnDeleteSelected, btnDeleteAll, btnRetryFailed, btnDownloadSelected });
            pnlBottom.Resize += (s, e) => LayoutBottomButtons(pnlBottom);
            LayoutBottomButtons(pnlBottom);

            // ── Grid ──────────────────────────────────────────────────
            BuildGrid();

            Controls.Add(dgv);
            Controls.Add(bottomBorder);
            Controls.Add(pnlBottom);
            Controls.Add(topBorder);
            Controls.Add(pnlTop);
        }

        private void LayoutTopControls()
        {
            if (pnlTop == null || lblBrand == null) return;

            int curX = 16;
            int midY = pnlTop.Height / 2;

            lblBrand.Location = new Point(curX, midY - lblBrand.PreferredSize.Height / 2);
            curX = lblBrand.Right + 18;

            lblIp.Location = new Point(curX, midY - lblIp.PreferredSize.Height / 2);
            curX = lblIp.Right + 4;

            txtServerIp.Location = new Point(curX, midY - txtServerIp.Height / 2);
            curX = txtServerIp.Right + 14;

            lblPort.Location = new Point(curX, midY - lblPort.PreferredSize.Height / 2);
            curX = lblPort.Right + 4;

            txtPort.Location = new Point(curX, midY - txtPort.Height / 2);
            curX = txtPort.Right + 14;

            Button[] topButtons = { btnConnect, btnDisconnect, btnTestConnection, btnOpenFolder, btnViewServerFiles };
            foreach (var btn in topButtons)
            {
                btn.Location = new Point(curX, midY - btn.Height / 2);
                curX = btn.Right + 6;
            }

            lblStatusDot.Location = new Point(curX + 8, midY - lblStatusDot.PreferredSize.Height / 2);
            lblStatusText.Location = new Point(lblStatusDot.Right + 4, midY - lblStatusText.PreferredSize.Height / 2);
        }

        private void LayoutBottomButtons(Panel pnlBottom)
        {
            btnDownloadSelected.Location = new Point(pnlBottom.Width - btnDownloadSelected.Width - 16, 8);
            btnRetryFailed.Location = new Point(btnDownloadSelected.Left - btnRetryFailed.Width - 8, 10);
            btnDeleteAll.Location = new Point(btnRetryFailed.Left - btnDeleteAll.Width - 8, 10);
            btnDeleteSelected.Location = new Point(btnDeleteAll.Left - btnDeleteSelected.Width - 8, 10);
            btnSelectAll.Location = new Point(btnDeleteSelected.Left - btnSelectAll.Width - 8, 10);

            int availableWidth = Math.Max(0, btnSelectAll.Left - lblSummary.Left - 12);
            lblSummary.Size = new Size(availableWidth, pnlBottom.Height);
        }

        private void BuildGrid()
        {
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30 },
                EditMode = DataGridViewEditMode.EditProgrammatically
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 248);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 246, 248);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = Color.FromArgb(235, 235, 235);

            Color softSelection = Color.FromArgb(225, 238, 250);
            dgv.DefaultCellStyle.SelectionBackColor = softSelection;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.SelectionBackColor = softSelection;
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = softSelection;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);

            colType = new DataGridViewTextBoxColumn { HeaderText = "Loại", Width = 55, ReadOnly = true };
            colName = new DataGridViewTextBoxColumn { HeaderText = "Tên file", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            colSize = new DataGridViewTextBoxColumn { HeaderText = "Kích thước", Width = 90, ReadOnly = true };
            colStatus = new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Width = 110, ReadOnly = true };
            colProgress = new DataGridViewTextBoxColumn { HeaderText = "Tiến độ (%)", Width = 100, ReadOnly = true };
            colTransferred = new DataGridViewTextBoxColumn { HeaderText = "Đã tải", Width = 150, ReadOnly = true };
            colSpeed = new DataGridViewTextBoxColumn { HeaderText = "Tốc độ", Width = 90, ReadOnly = true };
            colRetry = new DataGridViewButtonColumn { HeaderText = "", Text = "Thử lại", UseColumnTextForButtonValue = true, Width = 64, FlatStyle = FlatStyle.Flat };
            colDelete = new DataGridViewButtonColumn { HeaderText = "", Text = "Xóa", UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat };

            dgv.Columns.AddRange(colType, colName, colSize, colStatus, colProgress, colTransferred, colSpeed, colRetry, colDelete);

            dgv.CellClick += Dgv_CellClick;
            dgv.CellPainting += Dgv_CellPainting;
        }

        // ══════════════════════════════════════════════════════════════
        //  CONNECT / DISCONNECT
        // ══════════════════════════════════════════════════════════════
        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            string ip = txtServerIp.Text.Trim();
            string portText = txtPort.Text.Trim();

            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP của Server.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Port không hợp lệ. Vui lòng nhập trong khoảng 1 - 65535.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                btnConnect.Enabled = false;

                _clientService = new TcpClientService(ip, port);
                List<FileItem> files = await _clientService.GetFileListAsync();
                _downloadService = new DownloadService(_clientService, 3);

                _isConnected = true;
                UpdateConnectionUi();
                _connectionMonitorTimer.Start();

                MessageBox.Show("Đã kết nối tới server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await ShowServerFilesDialogAsync();
            }
            catch (Exception ex)
            {
                DisconnectClient();
                MessageBox.Show(
                    $"Không thể kết nối đến Server.\n\nĐịa chỉ: {ip}:{port}\nChi tiết: {ex.Message}",
                    "Kết nối thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void DisconnectClient()
        {
            _connectionMonitorTimer.Stop();
            _progressRefreshTimer.Stop();

            try { _clientService?.Disconnect(); } catch { }

            _clientService = null;
            _downloadService = null;
            _isConnected = false;

            UpdateConnectionUi();
        }

        private async System.Threading.Tasks.Task TestConnectionAsync()
        {
            if (_clientService == null)
            {
                MessageBox.Show("Chưa kết nối tới Server.", "Test kết nối", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnTestConnection.Enabled = false;

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await _clientService.GetFileListAsync();
                sw.Stop();

                MessageBox.Show(
                    $"Server phản hồi bình thường ({sw.ElapsedMilliseconds} ms).",
                    "Test kết nối", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Server không phản hồi.\n\n" + ex.Message,
                    "Test kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                DisconnectClient();
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  XEM TOÀN BỘ FILE TRÊN SERVER (CHỌN FILE ĐƯA VÀO HÀNG ĐỢI)
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task ShowServerFilesDialogAsync()
        {
            if (!IsClientConnected())
            {
                MessageBox.Show("Vui lòng kết nối tới Server.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<FileItem> allFiles;

            btnViewServerFiles.Enabled = false;
            try
            {
                allFiles = await _clientService!.GetFileListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lấy danh sách file từ Server.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectClient();
                return;
            }
            finally
            {
                btnViewServerFiles.Enabled = true;
            }

            var fileList = allFiles.OrderBy(f => f.FileName).ToList();

            using var dialog = new Form
            {
                Text = "Toàn bộ file trên Server",
                Size = new Size(650, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Font = new Font("Segoe UI", 9f)
            };

            var lblHint = new Label
            {
                Text = "Tích chọn ô, click dòng hoặc quét khối các file muốn thêm vào hàng đợi tải:",
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(12, 10, 12, 0),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            var dgvDialog = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 30 },
                GridColor = Color.FromArgb(225, 230, 235),
                CellBorderStyle = DataGridViewCellBorderStyle.Single
            };

            dgvDialog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 243, 246);
            dgvDialog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvDialog.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 243, 246);
            dgvDialog.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgvDialog.ColumnHeadersDefaultCellStyle.ForeColor;
            dgvDialog.EnableHeadersVisualStyles = false;
            dgvDialog.RowsDefaultCellStyle.BackColor = Color.White;
            dgvDialog.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            var colChk = new DataGridViewCheckBoxColumn
            {
                Name = "colChk",
                HeaderText = "",
                Width = 36,
                Resizable = DataGridViewTriState.False
            };
            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Tên file",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            var colSize = new DataGridViewTextBoxColumn
            {
                Name = "colSize",
                HeaderText = "Kích thước",
                Width = 100,
                ReadOnly = true
            };
            var colStatus = new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Trạng thái",
                Width = 160,
                ReadOnly = true
            };

            dgvDialog.Columns.AddRange(colChk, colName, colSize, colStatus);

            foreach (FileItem file in fileList)
            {
                bool alreadyInQueue = _items.ContainsKey(file.FileName);
                int idx = dgvDialog.Rows.Add(
                    false,
                    file.FileName,
                    FormatBytes(file.FileSize),
                    alreadyInQueue ? "Đã có trong hàng đợi" : "Chưa có"
                );
                dgvDialog.Rows[idx].Tag = file;
                if (alreadyInQueue)
                {
                    dgvDialog.Rows[idx].Cells[colStatus.Index].Style.ForeColor = ClrGray;
                }
            }

            // SỬA ĐỔI QUAN TRỌNG 1: Commit thay đổi CheckBox ngay lập tức khi người dùng tick vào ô CheckBox
            dgvDialog.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvDialog.IsCurrentCellDirty && dgvDialog.CurrentCell is DataGridViewCheckBoxCell)
                {
                    dgvDialog.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            // SỬA ĐỔI QUAN TRỌNG 2: Tự động đảo trạng thái CheckBox khi click vào bất kỳ ô nào trên dòng (ngoại trừ ô checkbox)
            dgvDialog.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex != colChk.Index)
                {
                    bool curVal = Convert.ToBoolean(dgvDialog.Rows[e.RowIndex].Cells[colChk.Index].Value);
                    dgvDialog.Rows[e.RowIndex].Cells[colChk.Index].Value = !curVal;
                    dgvDialog.EndEdit();
                }
            };

            var pnlDialogButtons = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            var topBorderDialog = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(220, 220, 220) };

            var btnSelectAllDialog = new Button
            {
                Text = "Chọn tất cả",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 0, 10, 0),
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Cursor = Cursors.Hand,
                Location = new Point(12, 10)
            };

            btnSelectAllDialog.Click += (s, e) =>
            {
                bool anyUnchecked = dgvDialog.Rows.Cast<DataGridViewRow>().Any(r => !Convert.ToBoolean(r.Cells[colChk.Index].Value));
                foreach (DataGridViewRow row in dgvDialog.Rows)
                {
                    row.Cells[colChk.Index].Value = anyUnchecked;
                    row.Selected = anyUnchecked;
                }
                dgvDialog.EndEdit();
            };

            var btnClose = new Button
            {
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(12, 0, 12, 0),
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Cursor = Cursors.Hand
            };

            var btnAdd = new Button
            {
                Text = "Thêm vào hàng đợi",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = ClrGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(12, 0, 12, 0),
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderColor = ClrGreen;

            pnlDialogButtons.Controls.Add(btnSelectAllDialog);
            pnlDialogButtons.Controls.Add(btnClose);
            pnlDialogButtons.Controls.Add(btnAdd);

            dialog.Controls.Add(dgvDialog);
            dialog.Controls.Add(topBorderDialog);
            dialog.Controls.Add(pnlDialogButtons);
            dialog.Controls.Add(lblHint);

            dialog.AcceptButton = btnAdd;
            dialog.CancelButton = btnClose;

            dialog.Shown += (s, e) =>
            {
                btnAdd.Location = new Point(pnlDialogButtons.ClientSize.Width - btnAdd.Width - 12, 10);
                btnClose.Location = new Point(btnAdd.Left - btnClose.Width - 8, 10);
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // SỬA ĐỔI QUAN TRỌNG 3: Chốt toàn bộ dữ liệu đang sửa trước khi đọc danh sách
                dgvDialog.EndEdit();

                int addedCount = 0;

                foreach (DataGridViewRow row in dgvDialog.Rows)
                {
                    bool isChecked = Convert.ToBoolean(row.Cells[colChk.Index].Value);
                    bool isSelected = row.Selected;

                    // Nhận diện file nếu ô được tích chọn HOẶC dòng đang được quét khối chọn
                    if ((isChecked || isSelected) && row.Tag is FileItem file)
                    {
                        _hiddenFiles.Remove(file.FileName);

                        if (!_items.ContainsKey(file.FileName))
                        {
                            AddFileRow(file);
                            addedCount++;
                        }
                    }
                }

                UpdateSummary();

                if (addedCount > 0)
                {
                    MessageBox.Show($"Đã thêm {addedCount} file vào hàng đợi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void UpdateConnectionUi()
        {
            btnConnect.Enabled = !_isConnected;
            btnDisconnect.Enabled = _isConnected;
            txtServerIp.Enabled = !_isConnected;
            txtPort.Enabled = !_isConnected;

            if (_isConnected)
            {
                lblStatusDot.ForeColor = ClrGreen;
                lblStatusText.Text = $"Đã kết nối {txtServerIp.Text}:{txtPort.Text}";
                lblStatusText.ForeColor = ClrGreen;
            }
            else
            {
                lblStatusDot.ForeColor = ClrGray;
                lblStatusText.Text = "Chưa kết nối";
                lblStatusText.ForeColor = ClrGray;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  CONNECTION MONITOR
        // ══════════════════════════════════════════════════════════════
        private async void ConnectionMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isConnected || _isDownloadInProgress || _isMonitorTicking || _clientService == null) return;

            _isMonitorTicking = true;

            try
            {
                List<FileItem> files = await _clientService.GetFileListAsync();
                SyncGridWithServerFiles(files);
            }
            catch
            {
                DisconnectClient();
                MessageBox.Show("Mất kết nối tới Server (Server có thể đã dừng).", "Mất kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _isMonitorTicking = false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  GRID DATA SYNC
        // ══════════════════════════════════════════════════════════════
        private void SyncGridWithServerFiles(List<FileItem> serverFiles)
        {
            var serverNames = serverFiles.Select(f => f.FileName).ToHashSet();

            foreach (string staleName in _items.Keys.Except(serverNames).ToList())
            {
                _items.Remove(staleName);

                var row = dgv.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (r.Tag as DownloadItem)?.FileName == staleName);
                if (row != null) dgv.Rows.Remove(row);
            }

            UpdateSummary();
        }

        private void AddFileRow(FileItem file)
        {
            if (_items.ContainsKey(file.FileName)) return;

            var item = new DownloadItem(file.FileName, file.FileSize);
            _items[file.FileName] = item;

            int rowIndex = dgv.Rows.Add();
            var row = dgv.Rows[rowIndex];
            row.Tag = item;
            row.Cells[colType.Index].Value = GetFileTypeLabel(file.FileName);
            row.Cells[colName.Index].Value = file.FileName;
            row.Cells[colSize.Index].Value = FormatBytes(file.FileSize);

            RefreshRowVisual(row, item);
        }

        private static string GetFileTypeLabel(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();
            return string.IsNullOrEmpty(ext) ? "FILE" : ext;
        }

        private static string FormatBytes(long bytes)
        {
            double mb = bytes / 1024.0 / 1024.0;
            return mb >= 1 ? $"{mb:F1} MB" : $"{bytes / 1024.0:F1} KB";
        }

        private void RefreshAllRowVisuals()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Tag is DownloadItem item)
                {
                    RefreshRowVisual(row, item);
                }
            }

            UpdateSummary();
        }

        private void RefreshRowVisual(DataGridViewRow row, DownloadItem item)
        {
            row.Cells[colStatus.Index].Value = StatusLabel(item.Status);
            row.Cells[colStatus.Index].Style.ForeColor = StatusColor(item.Status);
            row.Cells[colProgress.Index].Value = $"{item.Progress:F0}%";

            long displayedDownloaded = item.Status == DownloadStatus.Completed
                ? item.FileSize
                : (long)(item.FileSize * Math.Max(0, Math.Min(100, item.Progress)) / 100.0);

            row.Cells[colTransferred.Index].Value = $"{FormatBytes(displayedDownloaded)} / {FormatBytes(item.FileSize)}";
            row.Cells[colSpeed.Index].Value = item.Status == DownloadStatus.Downloading ? $"{item.SpeedMbps:F2} MB/s" : "";
            row.DefaultCellStyle.BackColor = item.Status == DownloadStatus.Failed ? ClrRowError : Color.White;
        }

        private static string StatusLabel(string status) => status switch
        {
            DownloadStatus.Waiting => "• Chờ",
            DownloadStatus.Downloading => "• Đang tải",
            DownloadStatus.Completed => "• Hoàn thành",
            DownloadStatus.Failed => "• Lỗi",
            _ => status
        };

        private static Color StatusColor(string status) => status switch
        {
            DownloadStatus.Completed => Color.FromArgb(39, 174, 96),
            DownloadStatus.Downloading => Color.FromArgb(52, 152, 219),
            DownloadStatus.Failed => Color.FromArgb(192, 57, 43),
            _ => Color.FromArgb(127, 140, 141)
        };

        private void UpdateSummary()
        {
            int total = _items.Count;
            int downloading = _items.Values.Count(i => i.Status == DownloadStatus.Downloading);
            int completed = _items.Values.Count(i => i.Status == DownloadStatus.Completed);
            int failed = _items.Values.Count(i => i.Status == DownloadStatus.Failed);
            long totalSize = _items.Values.Sum(i => i.FileSize);

            lblSummary.Text = $"{total} file ({FormatBytes(totalSize)})   |   ⬇ Đang tải: {downloading}   |   ✔ Xong: {completed}   |   ⚠ Lỗi: {failed}";
        }

        // ══════════════════════════════════════════════════════════════
        //  GRID INTERACTIONS
        // ══════════════════════════════════════════════════════════════
        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgv.Rows[e.RowIndex];
            if (row.Tag is not DownloadItem item) return;

            if (e.ColumnIndex == colRetry.Index)
            {
                _ = DownloadItemsAsync(new List<DownloadItem> { item });
            }
            else if (e.ColumnIndex == colDelete.Index)
            {
                if (item.Status == DownloadStatus.Downloading)
                {
                    MessageBox.Show($"Không thể xoá '{item.FileName}' khi đang tải.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Xoá '{item.FileName}' khỏi danh sách?",
                    "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    DeleteRow(item);
                    UpdateSummary();
                }
            }
        }

        private void Dgv_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colProgress.Index) return;
            if (dgv.Rows[e.RowIndex].Tag is not DownloadItem item) return;

            e.PaintBackground(e.CellBounds, true);

            double pct = Math.Max(0, Math.Min(100, item.Progress));
            int barWidth = (int)((e.CellBounds.Width - 8) * pct / 100.0);

            Color barColor = item.Status switch
            {
                DownloadStatus.Completed => Color.FromArgb(46, 204, 113),
                DownloadStatus.Downloading => Color.FromArgb(93, 173, 226),
                DownloadStatus.Failed => Color.FromArgb(231, 76, 60),
                _ => Color.FromArgb(225, 225, 225)
            };

            using (var brush = new SolidBrush(barColor))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds.X + 4, e.CellBounds.Y + 6, Math.Max(0, barWidth), e.CellBounds.Height - 12);
            }

            TextRenderer.DrawText(
                e.Graphics, $"{pct:F0}%", e.CellStyle.Font, e.CellBounds,
                pct > 50 ? Color.White : Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Handled = true;
        }

        private void ToggleSelectAll()
        {
            bool anyUnselected = dgv.Rows.Cast<DataGridViewRow>().Any(r => !r.Selected);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.Selected = anyUnselected;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  DOWNLOAD
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task btnDownloadSelected_Click()
        {
            var selected = _items.Values
                .Where(IsRowSelected)
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Chưa chọn file nào để tải. Vui lòng chọn (click hoặc quét khối) ít nhất 1 file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await DownloadItemsAsync(selected);
        }

        private bool IsRowSelected(DownloadItem item)
        {
            var row = dgv.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == item);
            return row != null && row.Selected;
        }

        private async System.Threading.Tasks.Task DownloadItemsAsync(List<DownloadItem> toDownload)
        {
            if (!IsClientConnected())
            {
                DisconnectClient();
                MessageBox.Show("Vui lòng kết nối tới Server.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (toDownload.Count == 0) return;

            btnRetryFailed.Enabled = false;
            btnDeleteSelected.Enabled = false;
            btnDeleteAll.Enabled = false;
            _isDownloadInProgress = true;
            _progressRefreshTimer.Start();

            _downloadCts = new System.Threading.CancellationTokenSource();
            SetDownloadButtonToStopMode(true);

            int successCount = 0;
            int failCount = 0;
            bool stoppedEarly = false;
            bool connectionLost = false;

            try
            {
                foreach (DownloadItem item in toDownload)
                {
                    if (_downloadCts.IsCancellationRequested)
                    {
                        stoppedEarly = true;
                        break;
                    }

                    if (!IsClientConnected())
                    {
                        connectionLost = true;
                        break;
                    }

                    try
                    {
                        await _downloadService!.ExecuteDownloadAsync(item);
                        successCount++;
                    }
                    catch
                    {
                        item.Status = DownloadStatus.Failed;
                        failCount++;

                        if (!IsClientConnected())
                        {
                            connectionLost = true;
                            RefreshAllRowVisuals();
                            break;
                        }
                    }

                    DeselectRow(item);
                    MoveRowToTop(item);

                    RefreshAllRowVisuals();
                }
            }
            finally
            {
                _isDownloadInProgress = false;
                _progressRefreshTimer.Stop();
                RefreshAllRowVisuals();

                _downloadCts?.Dispose();
                _downloadCts = null;
                SetDownloadButtonToStopMode(false);

                btnRetryFailed.Enabled = true;
                btnDeleteSelected.Enabled = true;
                btnDeleteAll.Enabled = true;
            }

            if (connectionLost)
            {
                DisconnectClient();
                MessageBox.Show(
                    $"Mất kết nối tới Server giữa chừng.\nĐã tải xong: {successCount}   |   Lỗi: {failCount}",
                    "Mất kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (stoppedEarly)
            {
                MessageBox.Show(
                    $"Đã dừng tải theo yêu cầu.\nĐã tải xong: {successCount}   |   Lỗi: {failCount}   |   Chưa tải: {toDownload.Count - successCount - failCount}",
                    "Đã dừng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Hoàn tất.\nThành công: {successCount}   |   Lỗi: {failCount}",
                    "Tải xuống", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SetDownloadButtonToStopMode(bool isDownloading)
        {
            if (isDownloading)
            {
                btnDownloadSelected.Text = "Dừng tải";
                btnDownloadSelected.BackColor = ClrRed;
                btnDownloadSelected.FlatAppearance.BorderColor = ClrRed;
            }
            else
            {
                btnDownloadSelected.Text = "Tải các file đã chọn";
                btnDownloadSelected.BackColor = ClrGreen;
                btnDownloadSelected.FlatAppearance.BorderColor = ClrGreen;
            }
        }

        private void DeselectRow(DownloadItem item)
        {
            var row = dgv.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == item);
            if (row != null) row.Selected = false;
        }

        // ══════════════════════════════════════════════════════════════
        //  XÓA FILE KHỎI DANH SÁCH
        // ══════════════════════════════════════════════════════════════
        private void DeleteRow(DownloadItem item, bool warnIfDownloading = true)
        {
            if (item.Status == DownloadStatus.Downloading)
            {
                if (warnIfDownloading)
                {
                    MessageBox.Show($"Không thể xoá '{item.FileName}' khi đang tải.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            _hiddenFiles.Add(item.FileName);
            _items.Remove(item.FileName);

            var row = dgv.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == item);
            if (row != null) dgv.Rows.Remove(row);
        }

        private void DeleteSelectedRows()
        {
            var selectedItems = _items.Values.Where(IsRowSelected).ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Chưa chọn file nào để xoá. Vui lòng click hoặc quét khối chọn ít nhất 1 file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Xoá {selectedItems.Count} file đã chọn khỏi danh sách?",
                "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            foreach (var item in selectedItems)
            {
                DeleteRow(item, warnIfDownloading: false);
            }

            UpdateSummary();
        }

        private void DeleteAllRows()
        {
            if (_items.Count == 0) return;

            var confirm = MessageBox.Show(
                $"Xoá toàn bộ {_items.Count} file khỏi danh sách?",
                "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            foreach (var item in _items.Values.ToList())
            {
                DeleteRow(item, warnIfDownloading: false);
            }

            UpdateSummary();
        }

        private void MoveRowToTop(DownloadItem item)
        {
            var row = dgv.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == item);
            if (row == null || row.Index == 0) return;

            dgv.Rows.Remove(row);
            dgv.Rows.Insert(0, row);
        }

        private bool IsClientConnected()
        {
            return _isConnected && _clientService != null && _downloadService != null && _clientService.IsConnected;
        }
    }
}