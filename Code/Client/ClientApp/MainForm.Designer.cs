using System.Threading.Channels;

namespace ClientApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlTop = new Panel();
            lblTitle = new Label();
            lblServer = new Label();
            txtServerIp = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            btnConnect = new Button();
            lblStatusDot = new Label();
            lblStatusText = new Label();

            splitMain = new SplitContainer();

            grpServer = new GroupBox();
            lstServerFiles = new ListBox();
            pnlServerBtns = new Panel();
            btnRefresh = new Button();
            btnAdd = new Button();

            grpQueue = new GroupBox();
            lstDownloadQueue = new ListBox();
            lvDownloads = new ListView();
            colFile = new ColumnHeader();
            colStatus = new ColumnHeader();
            colProgress = new ColumnHeader();
            colSpeed = new ColumnHeader();
            pnlQueueBtns = new Panel();
            btnRemove = new Button();
            btnDownload = new Button();

            statusStrip1 = new StatusStrip();
            tsslStatus = new ToolStripStatusLabel();
            tsslQueue = new ToolStripStatusLabel();
            tsslSpring = new ToolStripStatusLabel();

            toolTip1 = new ToolTip(components);
            // ── FORM SIZE ──────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);  // >= 646 pixels
            this.Controls.Add(pnlTop);
            this.Controls.Add(splitMain);
            this.Controls.Add(statusStrip1);
            this.Name = "MainForm";
            this.Text = "TCP File Downloader";
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;

            
            // ── Suspend ──────────────────────────────────────────────
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            grpServer.SuspendLayout();
            pnlServerBtns.SuspendLayout();
            grpQueue.SuspendLayout();
            pnlQueueBtns.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            // ── TOP BAR ──────────────────────────────────────────────
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 58;
            pnlTop.BackColor = Color.FromArgb(28, 40, 51);
            pnlTop.Padding = new Padding(14, 12, 14, 12);

            lblTitle.Text = "📥 TCP File Downloader";
            lblTitle.ForeColor = Color.FromArgb(174, 214, 241);
            lblTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(14, 17);

            // ── Resume ──────────────────────────────────────────────────────────
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpQueue.ResumeLayout(false);
            pnlQueueBtns.ResumeLayout(false);
            grpServer.ResumeLayout(false);
            pnlServerBtns.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

            lblServer.Text = "Server:";
            lblServer.ForeColor = Color.FromArgb(174, 182, 191);
            lblServer.Font = new Font("Segoe UI", 9f);
            lblServer.AutoSize = true;
            lblServer.Location = new Point(220, 19);

            txtServerIp.Text = "127.0.0.1";
            txtServerIp.Font = new Font("Segoe UI", 9.5f);
            txtServerIp.BorderStyle = BorderStyle.FixedSingle;
            txtServerIp.Location = new Point(272, 15);
            txtServerIp.Size = new Size(145, 26);
            txtServerIp.BackColor = Color.FromArgb(44, 62, 80);
            txtServerIp.ForeColor = Color.White;

            lblPort.Text = "Cổng:";
            lblPort.ForeColor = Color.FromArgb(174, 182, 191);
            lblPort.Font = new Font("Segoe UI", 9f);
            lblPort.AutoSize = true;
            lblPort.Location = new Point(428, 19);

            txtPort.Text = "5000";
            txtPort.Font = new Font("Segoe UI", 9.5f);
            txtPort.BorderStyle = BorderStyle.FixedSingle;
            txtPort.Location = new Point(472, 15);
            txtPort.Size = new Size(68, 26);
            txtPort.BackColor = Color.FromArgb(44, 62, 80);
            txtPort.ForeColor = Color.White;

            btnConnect.Text = "🔌  Kết nối";
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.FlatAppearance.BorderColor = Color.FromArgb(52, 152, 219);
            btnConnect.BackColor = Color.FromArgb(52, 152, 219);
            btnConnect.ForeColor = Color.White;
            btnConnect.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnConnect.Location = new Point(554, 12);
            btnConnect.Size = new Size(105, 32);
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.Click += btnConnect_Click;

            lblStatusDot.Text = "●";
            lblStatusDot.ForeColor = Color.FromArgb(100, 100, 100);
            lblStatusDot.Font = new Font("Segoe UI", 13f);
            lblStatusDot.AutoSize = true;
            lblStatusDot.Location = new Point(672, 14);

            lblStatusText.Text = "Chưa kết nối";
            lblStatusText.ForeColor = Color.FromArgb(127, 140, 141);
            lblStatusText.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
            lblStatusText.AutoSize = true;
            lblStatusText.Location = new Point(692, 19);

            pnlTop.Controls.AddRange(new Control[] {
                lblTitle, lblServer, txtServerIp,
                lblPort, txtPort, btnConnect,
                lblStatusDot, lblStatusText
            });

            // ── SPLIT CONTAINER ──────────────────────────────────────
            splitMain.Dock = DockStyle.Fill;
            splitMain.SplitterDistance = 400;
            splitMain.Panel1MinSize = 200;
            splitMain.Panel2MinSize = 300;
            splitMain.SplitterWidth = 6;
            splitMain.BackColor = Color.FromArgb(200, 210, 220);
            splitMain.Panel1.BackColor = Color.FromArgb(240, 244, 248);
            splitMain.Panel2.BackColor = Color.FromArgb(240, 244, 248);
            splitMain.Panel1.Padding = new Padding(10);
            splitMain.Panel2.Padding = new Padding(10);

            // ── LEFT: SERVER FILES ───────────────────────────────────
            grpServer.Dock = DockStyle.Fill;
            grpServer.Text = "  📁  File trên Server";
            grpServer.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grpServer.ForeColor = Color.FromArgb(44, 62, 80);
            grpServer.BackColor = Color.White;
            grpServer.Padding = new Padding(8, 12, 8, 8);
            splitMain.Panel1.Controls.Add(grpServer);

            lstServerFiles.Dock = DockStyle.Fill;
            lstServerFiles.Font = new Font("Segoe UI", 9.5f);
            lstServerFiles.SelectionMode = SelectionMode.MultiExtended;
            lstServerFiles.BorderStyle = BorderStyle.None;
            lstServerFiles.ItemHeight = 24;
            lstServerFiles.BackColor = Color.FromArgb(250, 251, 252);
            lstServerFiles.SelectedIndexChanged += lstServerFiles_SelectedIndexChanged;

            pnlServerBtns.Dock = DockStyle.Bottom;
            pnlServerBtns.Height = 46;
            pnlServerBtns.BackColor = Color.Transparent;
            pnlServerBtns.Padding = new Padding(0, 8, 0, 0);

            btnRefresh.Text = "🔄  Làm mới";
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
            btnRefresh.BackColor = Color.FromArgb(236, 240, 241);
            btnRefresh.Font = new Font("Segoe UI", 9f);
            btnRefresh.Size = new Size(105, 32);
            btnRefresh.Location = new Point(0, 8);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += btnRefresh_Click;
            toolTip1.SetToolTip(btnRefresh, "Tải lại danh sách file từ server (F5)");

            btnAdd.Text = "➕  Thêm vào hàng đợi";
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderColor = Color.FromArgb(39, 174, 96);
            btnAdd.BackColor = Color.FromArgb(39, 174, 96);
            btnAdd.ForeColor = Color.White;
            btnAdd.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAdd.Size = new Size(165, 32);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += btnAdd_Click;
            toolTip1.SetToolTip(btnAdd, "Thêm file đã chọn vào hàng đợi (Double-click)");

            pnlServerBtns.Controls.Add(btnRefresh);
            pnlServerBtns.Controls.Add(btnAdd);

            // Add to GroupBox: Fill → index 0, Bottom → index 1 (processed first)
            grpServer.Controls.Add(lstServerFiles);   // Fill
            grpServer.Controls.Add(pnlServerBtns);    // Bottom

            // ── RIGHT: DOWNLOAD QUEUE ────────────────────────────────
            grpQueue.Dock = DockStyle.Fill;
            grpQueue.Text = "  📋  Hàng đợi & Tiến độ";
            grpQueue.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grpQueue.ForeColor = Color.FromArgb(44, 62, 80);
            grpQueue.BackColor = Color.White;
            grpQueue.Padding = new Padding(8, 12, 8, 8);
            splitMain.Panel2.Controls.Add(grpQueue);

            // lstDownloadQueue - ẩn, dùng internal (giữ để tương thích service)
            lstDownloadQueue.Visible = false;

            colFile.Text = "Tên File"; colFile.Width = 230;
            colStatus.Text = "Trạng thái"; colStatus.Width = 105;
            colProgress.Text = "Tiến độ"; colProgress.Width = 80;
            colSpeed.Text = "Tốc độ"; colSpeed.Width = 95;

            lvDownloads.Dock = DockStyle.Fill;
            lvDownloads.View = View.Details;
            lvDownloads.FullRowSelect = true;
            lvDownloads.GridLines = false;
            lvDownloads.BorderStyle = BorderStyle.None;
            lvDownloads.Font = new Font("Segoe UI", 9.5f);
            lvDownloads.MultiSelect = true;
            lvDownloads.HideSelection = false;
            lvDownloads.BackColor = Color.FromArgb(250, 251, 252);
            lvDownloads.Columns.AddRange(new[] { colFile, colStatus, colProgress, colSpeed });
            lvDownloads.SelectedIndexChanged += lvDownloads_SelectedIndexChanged;

            pnlQueueBtns.Dock = DockStyle.Bottom;
            pnlQueueBtns.Height = 46;
            pnlQueueBtns.BackColor = Color.Transparent;
            pnlQueueBtns.Padding = new Padding(0, 8, 0, 0);

            btnRemove.Text = "🗑  Xóa";
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.BackColor = Color.FromArgb(231, 76, 60);
            btnRemove.ForeColor = Color.White;
            btnRemove.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRemove.Size = new Size(85, 32);
            btnRemove.Location = new Point(0, 8);
            btnRemove.Cursor = Cursors.Hand;
            btnRemove.Click += btnRemove_Click;
            toolTip1.SetToolTip(btnRemove, "Xóa file đã chọn khỏi hàng đợi (Delete)");

            btnDownload.Text = "▶  Bắt đầu tải";
            btnDownload.FlatStyle = FlatStyle.Flat;
            btnDownload.FlatAppearance.BorderColor = Color.FromArgb(52, 152, 219);
            btnDownload.BackColor = Color.FromArgb(52, 152, 219);
            btnDownload.ForeColor = Color.White;
            btnDownload.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnDownload.Size = new Size(140, 32);
            btnDownload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDownload.Cursor = Cursors.Hand;
            btnDownload.Click += btnDownload_Click;
            toolTip1.SetToolTip(btnDownload, "Bắt đầu tải tất cả file trong hàng đợi");

            pnlQueueBtns.Controls.Add(btnRemove);
            pnlQueueBtns.Controls.Add(btnDownload);

            grpQueue.Controls.Add(lstDownloadQueue); // hidden
            grpQueue.Controls.Add(lvDownloads);      // Fill
            grpQueue.Controls.Add(pnlQueueBtns);     // Bottom

            // ── STATUS STRIP ─────────────────────────────────────────
            statusStrip1.BackColor = Color.FromArgb(28, 40, 51);
            statusStrip1.ForeColor = Color.White;
            statusStrip1.SizingGrip = false;
            statusStrip1.Font = new Font("Segoe UI", 8.5f);

            tsslStatus.Text = "⚫  Chưa kết nối";
            tsslStatus.ForeColor = Color.FromArgb(127, 140, 141);

            tsslQueue.Text = "  |  Hàng đợi: 0 file";
            tsslQueue.ForeColor = Color.FromArgb(189, 195, 199);

            tsslSpring.Spring = true;

            statusStrip1.Items.AddRange(new ToolStripItem[] {
                tsslStatus, tsslQueue, tsslSpring
            });

            // ── FORM ─────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 244, 248);
            ClientSize = new Size(1050, 680);
            MinimumSize = new Size(1000, 630);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCP File Downloader";
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;

            // Form controls: Fill first (index 0), then Top (index 1), Bottom last (index 2)
            Controls.Add(splitMain);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip1);

            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;

            // ── Resume ───────────────────────────────────────────────
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlServerBtns.ResumeLayout(false);
            grpServer.ResumeLayout(false);
            splitMain.Panel1.ResumeLayout(false);
            pnlQueueBtns.ResumeLayout(false);
            grpQueue.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        // ── Field declarations ────────────────────────────────────────
        private Panel pnlTop;
        private Label lblTitle;
        private Label lblServer;
        private TextBox txtServerIp;
        private Label lblPort;
        private TextBox txtPort;
        private Button btnConnect;
        private Label lblStatusDot;
        private Label lblStatusText;

        private SplitContainer splitMain;

        private GroupBox grpServer;
        private ListBox lstServerFiles;
        private Panel pnlServerBtns;
        private Button btnRefresh;
        private Button btnAdd;

        private GroupBox grpQueue;
        private ListBox lstDownloadQueue;
        private ListView lvDownloads;
        private ColumnHeader colFile;
        private ColumnHeader colStatus;
        private ColumnHeader colProgress;
        private ColumnHeader colSpeed;
        private Panel pnlQueueBtns;
        private Button btnRemove;
        private Button btnDownload;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslStatus;
        private ToolStripStatusLabel tsslQueue;
        private ToolStripStatusLabel tsslSpring;

        private ToolTip toolTip1;
    }
}