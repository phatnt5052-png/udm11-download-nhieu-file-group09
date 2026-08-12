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

        public ServerInfo(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host khong hop le.");

            if (port < 1 || port > 65535)
                throw new ArgumentException("Host khong hop le.");

            Host = host;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
