using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Chat.Dispatcher
{
    public static class AddressUtils
    {
        /// <summary>
        /// Returns local network address with UPD Sockets in case of multiple network adapters with own local address.
        /// More: https://stackoverflow.com/a/27376368
        /// </summary>
        /// <returns>Address or empty string if couldn't get it.</returns>
        public static string GetLocalAddress()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint; 
            
            return endPoint != null ? endPoint.Address.ToString() : string.Empty;
        }

        /// <summary>
        /// Returns the first found local network address from specific adapter.
        /// </summary>
        /// <returns>Address or empty string if couldn't get it.</returns>
        public static string GetLocalAddressFromAdapter(string adapterName, AddressFamily addressFamily = AddressFamily.InterNetwork)
        {
            // Get all network interfaces;
            // Then get all IP addresses based on the local host name
            var adapters = NetworkInterface.GetAllNetworkInterfaces(); 
            var ips = Dns.GetHostAddresses(Dns.GetHostName()); 

            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses) // This associates the IP address with the correct adapter
                {
                    if ((adapter.Name == adapterName) && (ip.Address.AddressFamily == addressFamily)) 
                    {
                        return ip.Address.ToString();
                    }
                }
            }

            return string.Empty;
        }
    }
}
