using Newtonsoft.Json;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Chat.Dispatcher
{
    public static class Utils
    {
        public static bool Serialize<T>(T obj, string filePath)
        {
            string json = JsonConvert.SerializeObject(obj);
            try
            {
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T? Deserialize<T>(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var deserializedObj = JsonConvert.DeserializeObject<T>(json);
                return deserializedObj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static string GetLocalAddress()
        {
            string localIP = string.Empty;
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            try
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            catch (Exception) { }

            return localIP;
        }

        public static string GetLocalAddressFromAdapter(string adapterName, AddressFamily addressFamily = AddressFamily.InterNetwork)
        {
            // Get all network interfaces;
            // Get all IP addresses based on the local host name
            var adapters = NetworkInterface.GetAllNetworkInterfaces(); 
            var ips = Dns.GetHostAddresses(Dns.GetHostName()); 

            foreach (NetworkInterface adapter in adapters) //for each Network interface in addapters
            {
                IPInterfaceProperties properties = adapter.GetIPProperties(); // Get the ip properties from the adapter
                foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses) // for each UnicastIPAddressInformation in the IPInterfaceProperties Unicast address( this assocaites the IP address with the correct adapter)
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
