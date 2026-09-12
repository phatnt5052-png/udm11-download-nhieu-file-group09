namespace ClientApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            SuspendLayout();

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 680);
            MinimumSize = new System.Drawing.Size(900, 500);
            Text = "UDM_11 - MultiFileDownload";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Font = new System.Drawing.Font("Segoe UI", 9f);

            ResumeLayout(false);
        }
    }
}