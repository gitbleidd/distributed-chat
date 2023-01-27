namespace Chat.Dispatcher
{
    public class ChatServerAddresses
    {
        /// <summary>
        /// Key: "IPv4:port1:port2".
        /// Values: "IPv4, HTTP/1.1, HTTP/2.0 ports".
        /// </summary>
        public Dictionary<string, ChatServerAddressInfo> Addresses { get; } = new(); 
    }

    public class ChatServerAddressInfo
    {
        public string Ip { get; init; } = null!;
        public string Http1Port { get; init; } = null!;
        public string Http2Port { get; init; } = null!;
    }
}
