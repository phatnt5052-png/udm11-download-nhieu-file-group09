using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp
{
    public partial class MainForm : Form
    {
        private readonly DownloadQueueService _queueService = new();

        public MainForm()
        {
            InitializeComponent();

            // Thiết lập giao diện ban đầu
            ConfigureUi();
            UpdateButtonStates();
        }

        private void ConfigureUi()
        {
            // Cho phép chọn nhiều file ở danh sách Server
            lstServerFiles.SelectionMode = SelectionMode.MultiExtended;

            // Cho phép chọn nhiều file trong hàng đợi
            lstDownloadQueue.SelectionMode = SelectionMode.MultiExtended;

            // Cấu hình bảng download
            lvDownloads.View = View.Details;
            lvDownloads.FullRowSelect = true;
            lvDownloads.GridLines = true;

            // Tiêu đề cửa sổ
            Text = "Download nhiều file qua TCP";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);

            // Trạng thái ban đầu
            btnRemove.Enabled = false;
            btnDownload.Enabled = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Mốc 1: chưa kết nối Server.
            // Tạm thời tạo dữ liệu mẫu để kiểm tra giao diện.
            lstServerFiles.Items.Clear();

            lstServerFiles.Items.Add(new FileItem("TaiLieu1.pdf", 1024 * 500));
            lstServerFiles.Items.Add(new FileItem("BaiTapLapTrinh.zip", 1024 * 2048));
            lstServerFiles.Items.Add(new FileItem("VideoDemo.mp4", 1024 * 10240));

            UpdateButtonStates();
        }

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
                if (selectedItem is not FileItem file)
                    continue;

                var downloadItem = new DownloadItem(
                    file.FileName,
                    file.FileSize);

                if (_queueService.AddToQueue(downloadItem))
                {
                    addedCount++;
                }
                else
                {
                    duplicateCount++;
                }
            }

            RefreshDownloadQueueList();

            string message = $"Đã thêm {addedCount} file vào hàng đợi.";

            if (duplicateCount > 0)
            {
                message += $"\nCó {duplicateCount} file đã tồn tại trong hàng đợi.";
            }

            MessageBox.Show(
                message,
                "Hàng đợi tải xuống",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            UpdateButtonStates();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstDownloadQueue.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn file cần xóa.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            var selectedFiles = lstDownloadQueue.SelectedItems
                .Cast<object>()
                .OfType<DownloadItem>()
                .ToList();

            foreach (var item in selectedFiles)
            {
                _queueService.RemoveFromQueue(item.FileName);
            }

            RefreshDownloadQueueList();
            UpdateButtonStates();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (lstDownloadQueue.Items.Count == 0)
            {
                MessageBox.Show(
                    "Hàng đợi đang trống.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            MessageBox.Show(
                "Bắt đầu tải xuống.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void lstDownloadQueue_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void lstServerFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void RefreshDownloadQueueList()
        {
            lstDownloadQueue.Items.Clear();

            foreach (var item in _queueService.GetQueue())
            {
                lstDownloadQueue.Items.Add(item);
            }

            UpdateDownloadListView();
        }

        private void UpdateDownloadListView()
        {
            lvDownloads.Items.Clear();

            foreach (var item in _queueService.GetQueue())
            {
                var row = new ListViewItem(item.FileName);

                row.SubItems.Add(item.Status);
                row.SubItems.Add($"{item.Progress:F1}%");
                row.SubItems.Add($"{item.SpeedMbps:F2} Mbps");

                row.Tag = item;

                lvDownloads.Items.Add(row);
            }
        }

        private void UpdateButtonStates()
        {
            btnAdd.Enabled = lstServerFiles.SelectedItems.Count > 0;
            btnRemove.Enabled = lstDownloadQueue.SelectedItems.Count > 0;
            btnDownload.Enabled = lstDownloadQueue.Items.Count > 0;
        }

        // Các event cũ của Designer
        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            btnAdd_Click(sender, e);
        }
    }
}