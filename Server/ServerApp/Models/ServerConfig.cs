using System;

namespace ServerApp.Models
{
    /// <summary>
    /// Cấu hình thông số mặc định cho Server.
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// Cổng TCP mặc định mà Server sẽ lắng nghe để kết nối.
        /// </summary>
        public int Port { get; set; } = 9000;

        /// <summary>
        /// Đường dẫn đến thư mục chứa các file được chia sẻ để Client download.
        /// </summary>
        public string SharedFolder { get; set; } = "SharedFiles";
    }
}