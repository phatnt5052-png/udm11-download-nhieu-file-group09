namespace ServerApp
{
    partial class ServerMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblFolder = new Label();
            txtFolder = new TextBox();
            btnBrowse = new Button();
            btnRefresh = new Button();

            lblPort = new Label();
            txtPort = new TextBox();

            btnStart = new Button();
            btnStop = new Button();

            lblStatusTitle = new Label();
            lblStatus = new Label();

            lblFileList = new Label();
            lvFiles = new ListView();

            colFileName = new ColumnHeader();
            colSize = new ColumnHeader();
            colPath = new ColumnHeader();

            lblLog = new Label();
            txtLog = new TextBox();

            SuspendLayout();

            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.LightSteelBlue;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1100, 45);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "SERVER - UDM_11 - Download nhiều file";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // lblFolder
            // 
            lblFolder.AutoSize = true;
            lblFolder.Font = new Font("Segoe UI", 10F);
            lblFolder.Location = new Point(25, 70);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(142, 23);
            lblFolder.TabIndex = 1;
            lblFolder.Text = "Thư mục Server:";

            // 
            // txtFolder
            // 
            txtFolder.Location = new Point(175, 67);
            txtFolder.Name = "txtFolder";
            txtFolder.Size = new Size(600, 27);
            txtFolder.TabIndex = 2;
            txtFolder.Text = "ServerFiles";

            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(790, 65);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(100, 32);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "Chọn...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;

            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(900, 65);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(120, 32);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Làm mới";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;

            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Segoe UI", 10F);
            lblPort.Location = new Point(25, 115);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(53, 23);
            lblPort.TabIndex = 5;
            lblPort.Text = "Port:";

            // 
            // txtPort
            // 
            txtPort.Location = new Point(175, 112);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(150, 27);
            txtPort.TabIndex = 6;
            txtPort.Text = "5000";

            // 
            // btnStart
            // 
            btnStart.BackColor = Color.LightGreen;
            btnStart.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStart.Location = new Point(350, 108);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(130, 35);
            btnStart.TabIndex = 7;
            btnStart.Text = "Start Server";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;

            // 
            // btnStop
            // 
            btnStop.BackColor = Color.LightCoral;
            btnStop.Enabled = false;
            btnStop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStop.Location = new Point(495, 108);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(130, 35);
            btnStop.TabIndex = 8;
            btnStop.Text = "Stop Server";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;

            // 
            // lblStatusTitle
            // 
            lblStatusTitle.AutoSize = true;
            lblStatusTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatusTitle.Location = new Point(650, 115);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Size = new Size(75, 23);
            lblStatusTitle.TabIndex = 9;
            lblStatusTitle.Text = "Status:";

            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(730, 115);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(130, 23);
            lblStatus.TabIndex = 10;
            lblStatus.Text = "Server Offline";

            // 
            // lblFileList
            // 
            lblFileList.AutoSize = true;
            lblFileList.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblFileList.Location = new Point(25, 170);
            lblFileList.Name = "lblFileList";
            lblFileList.Size = new Size(175, 25);
            lblFileList.TabIndex = 11;
            lblFileList.Text = "Danh sách file Server";

            // 
            // lvFiles
            // 
            lvFiles.Columns.AddRange(new ColumnHeader[]
            {
                colFileName,
                colSize,
                colPath
            });

            lvFiles.FullRowSelect = true;
            lvFiles.GridLines = true;
            lvFiles.HideSelection = false;
            lvFiles.Location = new Point(25, 205);
            lvFiles.Name = "lvFiles";
            lvFiles.Size = new Size(995, 270);
            lvFiles.TabIndex = 12;
            lvFiles.UseCompatibleStateImageBehavior = false;
            lvFiles.View = View.Details;

            // 
            // colFileName
            // 
            colFileName.Text = "Tên file";
            colFileName.Width = 300;

            // 
            // colSize
            // 
            colSize.Text = "Kích thước";
            colSize.Width = 180;

            // 
            // colPath
            // 
            colPath.Text = "Đường dẫn";
            colPath.Width = 480;

            // 
            // lblLog
            // 
            lblLog.AutoSize = true;
            lblLog.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLog.Location = new Point(25, 495);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(150, 25);
            lblLog.TabIndex = 13;
            lblLog.Text = "Server Log";

            // 
            // txtLog
            // 
            txtLog.Location = new Point(25, 530);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(995, 150);
            txtLog.TabIndex = 14;

            // 
            // ServerMainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 720);

            Controls.Add(txtLog);
            Controls.Add(lblLog);

            Controls.Add(lvFiles);
            Controls.Add(lblFileList);

            Controls.Add(lblStatus);
            Controls.Add(lblStatusTitle);

            Controls.Add(btnStop);
            Controls.Add(btnStart);

            Controls.Add(txtPort);
            Controls.Add(lblPort);

            Controls.Add(btnRefresh);
            Controls.Add(btnBrowse);
            Controls.Add(txtFolder);
            Controls.Add(lblFolder);

            Controls.Add(lblTitle);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ServerMainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Server - UDM_11";

            Load += ServerMainForm_Load;
            FormClosing += ServerMainForm_FormClosing;

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;

        private Label lblFolder;
        private TextBox txtFolder;
        private Button btnBrowse;
        private Button btnRefresh;

        private Label lblPort;
        private TextBox txtPort;

        private Button btnStart;
        private Button btnStop;

        private Label lblStatusTitle;
        private Label lblStatus;

        private Label lblFileList;
        private ListView lvFiles;

        private ColumnHeader colFileName;
        private ColumnHeader colSize;
        private ColumnHeader colPath;

        private Label lblLog;
        private TextBox txtLog;
    }
}
