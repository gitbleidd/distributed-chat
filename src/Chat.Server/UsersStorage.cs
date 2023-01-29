using System.Collections.Concurrent;

namespace Chat.Server
{
    public class UsersStorage
    {
        public ConcurrentDictionary<string, string> Logins = new();
    }
}
