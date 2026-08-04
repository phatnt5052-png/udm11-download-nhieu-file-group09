namespace ClientApp.Models
{
    public class ServerInfo
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public ServerInfo()
        {
            Host = "127.0.0.1";
            Port = 5000;
        }
    }
}
