namespace Chat.Dispatcher
{
    public class ServerAddresses
    {
        public Dictionary<string, AddressInfo> Addresses { get; } = new(); // Key: IPv4:port1:port2 (values: IPv4 and HTTP/1.1, HTTP/2.0 ports)
    }

    public class AddressInfo
    {
        public string Ip { get; set; } = null!;
        public string Http1Port { get; set; } = null!;
        public string Http2Port { get; set; } = null!;
    }
}
