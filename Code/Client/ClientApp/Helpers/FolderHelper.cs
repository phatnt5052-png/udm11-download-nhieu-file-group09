using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ClientApp.Helpers
{
    public static class FolderHelper
    {
        public static void OpenDownloadsFolder()
        {
            string downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");

            if (!Directory.Exists(downloadFolder))
                Directory.CreateDirectory(downloadFolder);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = downloadFolder,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể mở thư mục: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static string GetDownloadsPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
    }
}
