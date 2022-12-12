using System.Collections.Concurrent;

namespace Chat.Server
{
    public class Users
    {
        public ConcurrentDictionary<string, string> Logins = new();
    }
}
