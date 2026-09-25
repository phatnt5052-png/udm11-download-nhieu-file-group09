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

    // Logo trường (UTH) nhúng thẳng vào code dạng Base64 — không cần file ảnh
    // rời kèm theo, không cần bước "Copy to Output Directory" thủ công trong
    // Visual Studio, không sợ quên copy khi đóng gói/deploy .exe.
    private const string UthLogoBase64 =
        "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";
    // ── Services ───────────────────────────────────────────────────
    private TcpClientService? _clientService;
    private DownloadService? _downloadService;
    private bool _isConnected = false;
    private bool _isDownloadInProgress = false;
    private System.Threading.CancellationTokenSource? _downloadCts;

    private readonly Dictionary<string, DownloadItem> _items = new();
    private readonly HashSet<string> _hiddenFiles = new();

    // Token hủy RIÊNG cho từng file đang tải, để có thể Hủy một file cụ thể
    // mà không ảnh hưởng tới các file khác đang tải đồng thời trong cùng lượt.
    // Mỗi token này được "link" (CreateLinkedTokenSource) từ _downloadCts của
    // cả lượt tải, nên bấm "Dừng tải" (hủy toàn bộ) vẫn hoạt động như cũ.
    private readonly Dictionary<DownloadItem, System.Threading.CancellationTokenSource> _itemCts = new();

    // ── Controls Top ───────────────────────────────────────────────
    private Panel pnlTop = null!;
    private PictureBox logoBox = null!;
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
    private DataGridViewCheckBoxColumn colSelect = null!;
    private DataGridViewTextBoxColumn colType = null!;
    private DataGridViewTextBoxColumn colName = null!;
    private DataGridViewTextBoxColumn colSize = null!;
    private DataGridViewTextBoxColumn colStatus = null!;
    private DataGridViewTextBoxColumn colProgress = null!;
    private DataGridViewTextBoxColumn colTransferred = null!;
    private DataGridViewTextBoxColumn colSpeed = null!;
    private DataGridViewButtonColumn colCancel = null!;
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
    private DateTime? _serverDisconnectedAt;

    // ── Constructor ────────────────────────────────────────────────
    public MainForm()
    {
        InitializeComponent();
        BuildUi();

        _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;
        _progressRefreshTimer.Tick += (s, e) => RefreshAllRowVisuals();

        UpdateConnectionUi();
        UpdateSummary();

        // Chặn không cho cửa sổ bị kéo nhỏ hơn mức đủ để hiển thị toàn bộ nút
        // của top bar và bottom bar (xem giải thích chi tiết tại EnsureMinimumWindowSize).
        EnsureMinimumWindowSize();
    }

    // ══════════════════════════════════════════════════════════════
    //  UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════
    private void BuildUi()
    {
        // ── Top bar ───────────────────────────────────────────────
        pnlTop = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
        logoBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        try
        {
            byte[] logoBytes = Convert.FromBase64String(UthLogoBase64);
            using var ms = new System.IO.MemoryStream(logoBytes);
            logoBox.Image = Image.FromStream(ms);

            // Giữ đúng tỉ lệ khung hình gốc của logo, chỉ giới hạn chiều cao
            // theo thanh top bar (64px), không kéo dãn méo ảnh.
            int logoHeight = 40;
            int logoWidth = (int)(logoBox.Image.Width * (logoHeight / (double)logoBox.Image.Height));
            logoBox.Size = new Size(logoWidth, logoHeight);
        }
        catch
        {
            // Không giải mã được logo (khó xảy ra vì đã nhúng sẵn) — bỏ qua,
            // không ảnh hưởng phần còn lại của UI.
        }

        pnlTop.Controls.Add(logoBox);

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

    // ══════════════════════════════════════════════════════════════
    //  LAYOUT DẠNG "WRAP" (tự động xuống dòng khi không đủ chỗ ngang)
    // ══════════════════════════════════════════════════════════════
    //
    // Thay vì định vị control theo tọa độ cố định trên MỘT hàng ngang duy
    // nhất (cách cũ khiến control bị đẩy ra ngoài vùng nhìn thấy và "biến
    // mất" khi cửa sổ hẹp lại), hàm dùng chung này xếp control theo kiểu
    // "chảy" từ trái sang phải: hết chỗ trên hàng hiện tại thì tự động
    // xuống hàng dưới, panel tự tăng chiều cao theo số hàng thực tế cần
    // dùng. Nhờ vậy không control nào bị mất, chỉ là chúng xếp thành nhiều
    // hàng hơn khi cửa sổ hẹp.
    //
    // apply = true  : thực sự gán Location cho từng control (dùng khi vẽ layout thật).
    // apply = false : chỉ tính toán, không đụng vào Location (dùng để "đo thử"
    //                 chiều cao cần thiết ở một chiều rộng giả định, phục vụ
    //                 tính MinimumSize — xem EnsureMinimumWindowSize).
    private static int LayoutFlowRow(
        int panelWidth,
        int marginX,
        int rowHeight,
        int vGap,
        IReadOnlyList<(Control Control, int GapAfter)> items,
        bool apply)
    {
        int curX = marginX;
        int curY = 8;
        bool isFirstOnRow = true;

        foreach (var (control, gapAfter) in items)
        {
            int w = control.Width;

            // Hết chỗ trên hàng hiện tại (và đây không phải control đầu hàng) -> xuống hàng mới
            if (!isFirstOnRow && curX + w > panelWidth - marginX)
            {
                curX = marginX;
                curY += rowHeight + vGap;
                isFirstOnRow = true;
            }

            if (apply)
            {
                control.Location = new Point(curX, curY + (rowHeight - control.Height) / 2);
            }

            curX += w + gapAfter;
            isFirstOnRow = false;
        }

        return curY + rowHeight + 8; // tổng chiều cao cần dùng
    }

    private List<(Control, int)> BuildTopFlowItems()
    {
        var items = new List<(Control, int)>();

        if (logoBox.Image != null)
            items.Add((logoBox, 14));

        items.Add((lblBrand, 18));
        items.Add((lblIp, 4));
        items.Add((txtServerIp, 14));
        items.Add((lblPort, 4));
        items.Add((txtPort, 14));
        items.Add((btnConnect, 6));
        items.Add((btnDisconnect, 6));
        items.Add((btnTestConnection, 6));
        items.Add((btnOpenFolder, 6));
        items.Add((btnViewServerFiles, 14));
        items.Add((lblStatusDot, 4));
        items.Add((lblStatusText, 0));

        return items;
    }

    private List<(Control, int)> BuildBottomFlowItems()
    {
        return new List<(Control, int)>
        {
            (lblSummary, 12),
            (btnSelectAll, 6),
            (btnDeleteSelected, 6),
            (btnDeleteAll, 6),
            (btnRetryFailed, 6),
            (btnDownloadSelected, 0),
        };
    }

    private void LayoutTopControls()
    {
        if (pnlTop == null || lblBrand == null) return;

        if (logoBox.Image == null)
            logoBox.Size = Size.Empty; // logo không tải được -> không chiếm chỗ

        int neededHeight = LayoutFlowRow(pnlTop.Width, 16, 34, 4, BuildTopFlowItems(), apply: true);

        // Panel tự cao thêm khi phải xuống nhiều hàng. Việc set Height ở đây sẽ
        // tự kích hoạt thêm một lần Resize -> gọi lại chính hàm này, nhưng lần
        // thứ 2 sẽ tính ra đúng cùng 1 chiều cao nên không lặp vô hạn (tự ổn định).
        if (pnlTop.Height != neededHeight)
            pnlTop.Height = neededHeight;
    }

    private void LayoutBottomButtons(Panel pnlBottom)
    {
        const int marginX = 16;
        const int summaryMinWidth = 140; // đủ chỗ tối thiểu để đọc dòng tóm tắt

        int buttonsTotalWidth =
            btnSelectAll.Width + btnDeleteSelected.Width + btnDeleteAll.Width +
            btnRetryFailed.Width + btnDownloadSelected.Width + 6 * 4;

        int available = Math.Max(summaryMinWidth, pnlBottom.Width - marginX * 2 - buttonsTotalWidth - 12);
        lblSummary.Size = new Size(available, 34);

        int neededHeight = LayoutFlowRow(pnlBottom.Width, marginX, 34, 6, BuildBottomFlowItems(), apply: true);

        if (pnlBottom.Height != neededHeight)
            pnlBottom.Height = neededHeight;
    }

    // ══════════════════════════════════════════════════════════════
    //  MINIMUM WINDOW SIZE (chống mất nút khi thu nhỏ cửa sổ)
    // ══════════════════════════════════════════════════════════════
    //
    // Vì top bar/bottom bar giờ tự xuống dòng (LayoutFlowRow ở trên), cửa sổ
    // không còn cần rộng bằng TỔNG mọi control nữa — chỉ cần đủ rộng cho
    // CONTROL ĐƠN LẺ RỘNG NHẤT (một nút, hay dòng chữ tiêu đề) là đủ, control
    // đó không thể tự bẻ đôi nên đây mới là giới hạn thực sự không thể nhỏ hơn.
    // Chiều cao tối thiểu được "đo thử" (apply:false, không đụng Location) ở
    // đúng chiều rộng tối thiểu đó, để biết chắc top/bottom bar cần bao nhiêu
    // hàng khi bị dồn hẹp nhất, cộng thêm khoảng tối thiểu cho DataGridView.
    private void EnsureMinimumWindowSize()
    {
        int widestSingleControl = new[]
        {
            lblBrand.PreferredSize.Width,
            btnConnect.Width, btnDisconnect.Width, btnTestConnection.Width, btnOpenFolder.Width, btnViewServerFiles.Width,
            btnSelectAll.Width, btnDeleteSelected.Width, btnDeleteAll.Width, btnRetryFailed.Width, btnDownloadSelected.Width
        }.Max();

        int minClientWidth = Math.Max(320, widestSingleControl + 32);

        int topHeightAtMinWidth = LayoutFlowRow(minClientWidth, 16, 34, 4, BuildTopFlowItems(), apply: false);

        lblSummary.Size = new Size(140, 34); // kích thước tối thiểu giả định để đo
        int bottomHeightAtMinWidth = LayoutFlowRow(minClientWidth, 16, 34, 6, BuildBottomFlowItems(), apply: false);

        const int gridMinHeight = 160; // đủ để thấy header + vài dòng dữ liệu, có thanh cuộn khi cần
        const int bordersHeight = 2;   // 2 đường viền 1px trên/dưới

        int minClientHeight = topHeightAtMinWidth + bottomHeightAtMinWidth + bordersHeight + gridMinHeight;

        this.MinimumSize = this.SizeFromClientSize(new Size(minClientWidth, minClientHeight));

        // Bước đo ở trên chỉ tính toán (không apply Location), nhưng lblSummary.Size
        // vừa bị đổi tạm để đo -> layout lại đúng theo kích thước panel thật hiện tại.
        LayoutTopControls();
        LayoutBottomButtons((Panel)lblSummary.Parent!);
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
            EditMode = DataGridViewEditMode.EditOnEnter
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

        colSelect = new DataGridViewCheckBoxColumn
        {
            HeaderText = "",
            Width = 40,
            ReadOnly = false,
            Name = "colSelect",
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        colType = new DataGridViewTextBoxColumn { HeaderText = "Loại", Width = 55, ReadOnly = true };
        colName = new DataGridViewTextBoxColumn { HeaderText = "Tên file", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
        colSize = new DataGridViewTextBoxColumn { HeaderText = "Kích thước", Width = 90, ReadOnly = true };
        colStatus = new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Width = 110, ReadOnly = true };
        colProgress = new DataGridViewTextBoxColumn { HeaderText = "Tiến độ (%)", Width = 100, ReadOnly = true };
        colTransferred = new DataGridViewTextBoxColumn { HeaderText = "Đã tải", Width = 150, ReadOnly = true };
        colSpeed = new DataGridViewTextBoxColumn { HeaderText = "Tốc độ", Width = 90, ReadOnly = true };
        colCancel = new DataGridViewButtonColumn { HeaderText = "", Text = "Hủy", UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat };
        colRetry = new DataGridViewButtonColumn { HeaderText = "", Text = "Thử lại", UseColumnTextForButtonValue = true, Width = 64, FlatStyle = FlatStyle.Flat };
        colDelete = new DataGridViewButtonColumn { HeaderText = "", Text = "Xóa", UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat };

        dgv.Columns.AddRange(colSelect, colType, colName, colSize, colStatus, colProgress, colTransferred, colSpeed, colCancel, colRetry, colDelete);

        dgv.CellClick += Dgv_CellClick;
        dgv.CellPainting += Dgv_CellPainting;
        dgv.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (dgv.IsCurrentCellDirty &&
                dgv.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        dgv.CellContentClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == colSelect.Index)
            {
                dgv.EndEdit();
            }
        };
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

            _serverDisconnectedAt = null;

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

        _downloadCts?.Cancel();

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

        // Commit thay đổi CheckBox ngay lập tức khi người dùng tick vào ô CheckBox
        dgvDialog.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (dgvDialog.IsCurrentCellDirty && dgvDialog.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvDialog.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        // Tự động đảo trạng thái CheckBox khi click vào bất kỳ ô nào trên dòng (ngoại trừ ô checkbox)
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
            // Chốt toàn bộ dữ liệu đang sửa trước khi đọc danh sách
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
        if (!_isConnected ||
            _isDownloadInProgress ||
            _isMonitorTicking ||
            _clientService == null)
            return;

        _isMonitorTicking = true;

        try
        {
            await _clientService.GetFileListAsync();

            // Server vẫn hoạt động hoặc đã kết nối lại. Không đụng gì tới bảng dữ
            // liệu ở đây nữa (xem lý do trong ghi chú TC_D26) — lệnh gọi này chỉ
            // dùng làm "nhịp tim" để phát hiện mất kết nối.
            _serverDisconnectedAt = null;
        }
        catch
        {
            // Lần đầu phát hiện Server mất kết nối
            if (_serverDisconnectedAt == null)
            {
                _serverDisconnectedAt = DateTime.Now;

                MessageBox.Show(
                    "Mất tín hiệu kết nối với server.",
                    "Mất kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // Tính thời gian kể từ lúc phát hiện mất kết nối
            TimeSpan disconnectedTime =
                DateTime.Now - _serverDisconnectedAt.Value;

            // Chưa đủ 25 giây -> tiếp tục chờ
            if (disconnectedTime.TotalSeconds < 25)
            {
                return;
            }

            // Chờ khoảng 25 giây Server vẫn chưa kết nối lại
            MessageBox.Show(
                "Không thể kết nối lại với Server.\n\n" +
                "Client sẽ đóng ứng dụng.",
                "Mất kết nối Server",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            DisconnectClient();

            // Đóng Client
            Close();
        }
        finally
        {
            _isMonitorTicking = false;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  GRID DATA SYNC
    // ══════════════════════════════════════════════════════════════
    //
    // Lưu ý (fix TC_D26): trước đây có 1 hàm SyncGridWithServerFiles chạy theo
    // timer mỗi 5 giây, tự động xóa khỏi UI các item đang "Chờ" nếu Server không
    // còn thấy file đó nữa. Vấn đề: nếu timer tình cờ chạy đúng lúc file vừa bị
    // xóa bên Server nhưng người dùng CHƯA KỊP bấm "Tải" (item vẫn "Chờ"), nó sẽ
    // âm thầm xóa luôn dòng đó khỏi bảng — tái hiện đúng lỗi TC_D26 ("dòng biến
    // mất thay vì báo Lỗi"), chỉ khác đường đi so với lỗi gốc.
    // Đã bỏ hẳn hàm đó: giờ không có gì tự động xóa item khỏi UI theo trạng thái
    // Server nữa. Trạng thái 1 file chỉ do hành động tải THẬT SỰ quyết định — nếu
    // file không còn tồn tại, Server trả lỗi cho request GET và DownloadService
    // sẽ set item.Status = Failed, đúng như TC_D26 kỳ vọng. Dọn dẹp thủ công vẫn
    // dùng nút "Xóa" / "Xóa tất cả" như bình thường.

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
        row.Cells[colName.Index].Value = item.FileName;
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

        if (e.ColumnIndex == colCancel.Index)
        {
            if (item.Status != DownloadStatus.Downloading)
            {
                MessageBox.Show($"'{item.FileName}' hiện không ở trạng thái đang tải để hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CancelItemDownload(item);
        }
        else if (e.ColumnIndex == colRetry.Index)
        {
            if (item.Status == DownloadStatus.Downloading)
            {
                MessageBox.Show($"'{item.FileName}' đang được tải, vui lòng đợi hoặc bấm Hủy trước khi Thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // FIX: "Thử lại" chỉ có ý nghĩa với file đang ở trạng thái LỖI. Trước đây
            // nút này chấp nhận cả trạng thái "Chờ" (chưa từng bấm Tải) và "Hoàn thành",
            // khiến người dùng vô tình bấm nhầm "Thử lại" trên 1 file còn đang "Chờ" thì
            // Client lại tự động BẮT ĐẦU TẢI file đó — trái với kỳ vọng của một nút
            // "thử lại", và không có cách nào phân biệt được với việc chủ động bấm "Tải".
            if (item.Status != DownloadStatus.Failed)
            {
                string message = item.Status == DownloadStatus.Completed
                    ? $"'{item.FileName}' đã tải xong, không có gì để thử lại.\n\nNếu muốn tải lại, hãy tick chọn file và bấm \"Tải các file đã chọn\"."
                    : $"'{item.FileName}' chưa được tải (đang ở trạng thái Chờ).\n\nNút \"Thử lại\" chỉ dùng cho file bị lỗi. Hãy tick chọn file và bấm \"Tải các file đã chọn\" để bắt đầu tải.";

                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

    // Cho phép "Chọn tất cả" gồm cả file đã Hoàn thành — 1 file đã tải xong vẫn
    // tick chọn và tải lại được bình thường (tạo thêm 1 bản sao đổi tên, giống
    // cách trình duyệt xử lý file trùng tên), không còn bị loại trừ khỏi thao
    // tác chọn hàng loạt nữa.
    private void ToggleSelectAll()
    {
        bool anyUnchecked = dgv.Rows.Cast<DataGridViewRow>()
            .Where(row => row.Tag is DownloadItem)
            .Any(row => !Convert.ToBoolean(row.Cells[colSelect.Index].Value));

        foreach (DataGridViewRow row in dgv.Rows)
        {
            if (row.Tag is DownloadItem)
            {
                row.Cells[colSelect.Index].Value = anyUnchecked;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  DOWNLOAD
    // ══════════════════════════════════════════════════════════════
    private async System.Threading.Tasks.Task btnDownloadSelected_Click()
    {
        var selected = _items.Values
          .Where(item =>
          {
              var row = dgv.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(r => r.Tag == item);

              return row != null &&
                     Convert.ToBoolean(row.Cells[colSelect.Index].Value);
          })
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

       
        var tasks = new List<System.Threading.Tasks.Task>();

        foreach (DownloadItem item in toDownload)
        {
            var itemCts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(_downloadCts.Token);
            _itemCts[item] = itemCts;
            tasks.Add(RunSingleDownloadAsync(item, itemCts.Token));
        }

        try
        {
            await System.Threading.Tasks.Task.WhenAll(tasks);
        }
        finally
        {
            foreach (DownloadItem item in toDownload)
            {
                if (_itemCts.TryGetValue(item, out var cts))
                {
                    _itemCts.Remove(item);
                    cts.Dispose();
                }
            }

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

        int successCount = toDownload.Count(i => i.Status == DownloadStatus.Completed);
        int failCount = toDownload.Count(i => i.Status != DownloadStatus.Completed);
        bool stoppedByUser = _downloadCts?.IsCancellationRequested ?? false;
        bool connectionLost = !IsClientConnected();

        
        string BuildFailureDetails()
        {
            var failedItems = toDownload
                .Where(i => i.Status != DownloadStatus.Completed && !string.IsNullOrWhiteSpace(i.LastError))
                .Take(5)
                .Select(i => $"- {i.FileName}: {i.LastError}")
                .ToList();

            return failedItems.Count == 0 ? string.Empty : "\n\nChi tiết lỗi:\n" + string.Join("\n", failedItems);
        }

        if (connectionLost)
        {
            DisconnectClient();
            MessageBox.Show(
                $"Mất kết nối tới Server giữa chừng.\nĐã tải xong: {successCount}   |   Lỗi: {failCount}{BuildFailureDetails()}",
                "Mất kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (stoppedByUser)
        {
            MessageBox.Show(
                $"Đã dừng tải theo yêu cầu.\nĐã tải xong: {successCount}   |   Lỗi/Đã hủy: {failCount}",
                "Đã dừng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(
                $"Hoàn tất.\nThành công: {successCount}   |   Lỗi: {failCount}{BuildFailureDetails()}",
                "Tải xuống", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Chạy tải cho ĐÚNG MỘT file, dùng token riêng của file đó. Vì DownloadService.
    // ExecuteDownloadAsync tự bắt mọi lỗi bên trong (kể cả bị hủy) và không ném ra
    // ngoài, nhánh catch dưới đây chỉ thật sự chạy tới trong trường hợp hiếm: bị hủy
    // NGAY TRONG LÚC CÒN ĐANG CHỜ SEMAPHORE (chưa kịp bắt đầu tải), vì lúc đó
    // ExecuteDownloadAsync chưa vào tới try/catch nội bộ của nó để tự set trạng thái.
    private async System.Threading.Tasks.Task RunSingleDownloadAsync(DownloadItem item, System.Threading.CancellationToken token)
    {
        try
        {
            await _downloadService!.ExecuteDownloadAsync(item, cancellationToken: token);
        }
        catch (Exception ex)
        {
            item.Status = DownloadStatus.Failed;
            item.SpeedMbps = 0;
            item.LastError = ex.Message;
        }

        DeselectRow(item);
        MoveRowToTop(item);
        RefreshAllRowVisuals();
    }

    // Hủy RIÊNG một file cụ thể đang tải, không ảnh hưởng các file khác trong cùng lượt.
    private void CancelItemDownload(DownloadItem item)
    {
        if (_itemCts.TryGetValue(item, out var cts))
        {
            cts.Cancel();
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
        var row = dgv.Rows.Cast<DataGridViewRow>()
            .FirstOrDefault(r => r.Tag == item);

        if (row != null)
        {
            // Bỏ tick checkbox
            row.Cells[colSelect.Index].Value = false;

            // Bỏ chọn dòng
            row.Selected = false;

            // KHÔNG khóa checkbox nữa — 1 file đã "Hoàn thành" vẫn tick chọn và
            // tải lại được bình thường. Vì DownloadService dùng OverwriteRule.Rename
            // theo mặc định, tải lại sẽ tự tạo thêm 1 bản sao (tên có timestamp)
            // thay vì ghi đè, giống cách trình duyệt xử lý file tải trùng tên.
        }
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

        // QUAN TRỌNG: không dùng thẳng item.FileName làm khóa Dictionary nữa.
        // Khi 1 file trùng tên được tải lại (OverwriteRule.Rename trong
        // DownloadService), item.FileName bị đổi sang tên mới có timestamp để
        // hiển thị — nhưng khóa gốc trong _items vẫn là TÊN FILE THẬT trên Server
        // (không đổi). Nếu xoá bằng item.FileName lúc này sẽ xoá NHẦM/không xoá
        // được gì (Dictionary.Remove trên khoá không tồn tại), khiến _items giữ
        // lại 1 mục "ma" dù dòng trên UI đã biến mất.
        string? originalKey = _items.FirstOrDefault(kv => kv.Value == item).Key;

        if (originalKey != null)
        {
            _hiddenFiles.Add(originalKey);
            _items.Remove(originalKey);
        }

        // Dọn luôn file ".partial" dở dang (nếu có) của item này — tránh để lại
        // rác trong thư mục Downloads khi người dùng đã chủ động xóa khỏi hàng đợi.
        _downloadService?.DeletePartialFile(item);

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