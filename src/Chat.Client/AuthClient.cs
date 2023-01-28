using Newtonsoft.Json;
using System.Configuration;
using System.Text;

namespace Chat.Client
{
    internal static class AuthClient
    {
        private static HttpClient _httpClient;
        private static string _dispatcherAddress;

        static AuthClient()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
            _dispatcherAddress = ConfigurationSettings.AppSettings.Get("DispatcherAddress");
        }

        public static async Task<(int, string)> Auth(string login)
        {
            // var json = $"{{\"Login\": \"{login}\"}}";
            string json = JsonConvert.SerializeObject(new { Login = login });
            StringContent postContent = new(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(_dispatcherAddress + "/auth", postContent);
                   
                var body = JsonConvert.DeserializeObject<Message>(await response.Content.ReadAsStringAsync()).address;
                var statusCode = (int)response.StatusCode;
                return (statusCode, body);
            }
            catch
            {
                throw;
            }
        }
    }
}