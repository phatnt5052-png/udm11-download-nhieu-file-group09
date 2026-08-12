namespace ClientApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblServerFiles = new Label();
            lblDownloadQueue = new Label();
            lstServerFiles = new ListBox();
            btnRefresh = new Button();
            btnAdd = new Button();
            btnDownload = new Button();
            btnRemove = new Button();
            lstDownloadQueue = new ListBox();
            lvDownloads = new ListView();
            File = new ColumnHeader();
            Trạngthái = new ColumnHeader();
            tientrinh = new ColumnHeader();
            tocdo = new ColumnHeader();
            SuspendLayout();
            // 
            // lblServerFiles
            // 
            lblServerFiles.AutoSize = true;
            lblServerFiles.Location = new Point(20, 20);
            lblServerFiles.Name = "lblServerFiles";
            lblServerFiles.Size = new Size(177, 20);
            lblServerFiles.TabIndex = 0;
            lblServerFiles.Text = "Danh sách file trên Server";
            lblServerFiles.Click += label1_Click;
            // 
            // lblDownloadQueue
            // 
            lblDownloadQueue.AutoSize = true;
            lblDownloadQueue.Location = new Point(760, 20);
            lblDownloadQueue.Name = "lblDownloadQueue";
            lblDownloadQueue.Size = new Size(126, 20);
            lblDownloadQueue.TabIndex = 1;
            lblDownloadQueue.Text = "Danh sách chờ tải";
            // 
            // lstServerFiles
            // 
            lstServerFiles.FormattingEnabled = true;
            lstServerFiles.Location = new Point(20, 50);
            lstServerFiles.Name = "lstServerFiles";
            lstServerFiles.Size = new Size(400, 244);
            lstServerFiles.TabIndex = 2;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(20, 320);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "Làm mới";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(500, 100);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(150, 40);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "Thêm";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += button2_Click;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(760, 320);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(94, 29);
            btnDownload.TabIndex = 5;
            btnDownload.Text = "Tải xuống";
            btnDownload.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(500, 160);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(150, 40);
            btnRemove.TabIndex = 6;
            btnRemove.Text = "Xóa";
            btnRemove.UseVisualStyleBackColor = true;
            // 
            // lstDownloadQueue
            // 
            lstDownloadQueue.FormattingEnabled = true;
            lstDownloadQueue.Location = new Point(760, 50);
            lstDownloadQueue.Name = "lstDownloadQueue";
            lstDownloadQueue.Size = new Size(400, 244);
            lstDownloadQueue.TabIndex = 7;
            // 
            // lvDownloads
            // 
            lvDownloads.Columns.AddRange(new ColumnHeader[] { File, Trạngthái, tientrinh, tocdo });
            lvDownloads.FullRowSelect = true;
            lvDownloads.GridLines = true;
            lvDownloads.Location = new Point(20, 380);
            lvDownloads.Name = "lvDownloads";
            lvDownloads.Size = new Size(1140, 230);
            lvDownloads.TabIndex = 8;
            lvDownloads.UseCompatibleStateImageBehavior = false;
            // 
            // File
            // 
            File.Text = "File";
            File.Width = 300;
            // 
            // Trạngthái
            // 
            Trạngthái.Tag = "";
            Trạngthái.Text = "Trạng thái";
            Trạngthái.Width = 150;
            // 
            // tientrinh
            // 
            tientrinh.Text = "Tiến trình";
            tientrinh.Width = 150;
            // 
            // tocdo
            // 
            tocdo.Text = "Tốc ";
            tocdo.Width = 150;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1827, 776);
            Controls.Add(lvDownloads);
            Controls.Add(lstDownloadQueue);
            Controls.Add(btnRemove);
            Controls.Add(btnDownload);
            Controls.Add(btnAdd);
            Controls.Add(btnRefresh);
            Controls.Add(lstServerFiles);
            Controls.Add(lblDownloadQueue);
            Controls.Add(lblServerFiles);
            Name = "MainForm";
            Text = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblServerFiles;
        private Label lblDownloadQueue;
        private ListBox lstServerFiles;
        private Button button1;
        private Button button2;
        private Button btnDownload;
        private Button btnRemove;
        private ListBox lstDownloadQueue;
        private System.Windows.Forms.Button btnAdd;
        private ListView lvDownloads;
        private System.Windows.Forms.Button btnRefresh;
        private ColumnHeader File;
        private ColumnHeader Trạngthái;
        private ColumnHeader tientrinh;
        private ColumnHeader tocdo;
    }
}
