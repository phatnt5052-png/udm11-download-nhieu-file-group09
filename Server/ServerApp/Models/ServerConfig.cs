namespace ServerApp.Models
{
    public class ServerConfig
    {
        public int Port { get; set; } = 9000;
        public string SharedFolder { get; set; } = "SharedFiles";
    }
}
