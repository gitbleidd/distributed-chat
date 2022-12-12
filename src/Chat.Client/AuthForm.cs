using System.Text;
using System.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

// Пасхалка
// gaiboi
namespace Chat.Client
{
    public partial class AuthForm : Form
    {
        // Получение IP:Port Диспетчера серверов
        private static string DispatcherAddress = ConfigurationSettings.AppSettings.Get("DispatcherAddress");
        public static HubConnection connection;

        // Словарь ошибок
        public static Dictionary<string, string> Error = new Dictionary<string, string>()
        {
            {"EmptyField", "Please enter username"},
            {"TakenUsername", "Username is taken, try another"},
            {"NoServers", "No server is running"},
            {"ServerDisconnection", "You've been disconnected from server"},
            {"UnknownError", "Server is down"},
            {"DispatcherError", "Couldn't connect to Dispatcher"},
            {"LoginLength", "Login length must be less than 20 symbols"}
        };


        public AuthForm()
        {
            InitializeComponent();
        }

        private async void btLogin_Click(object sender, EventArgs e)
        {
            if (tbUserInput.Text.Length > 20)
            {
                lbError.Text = Error["LoginLength"];
                lbError.Visible = true;
                return;
            }
            string trimmedUserName = tbUserInput.Text.Trim();
            if (string.IsNullOrEmpty(trimmedUserName))
            {
                lbError.Text = Error["EmptyField"];
                lbError.Visible = true;
            }
            else
            {
                // Попытка авторизации
                btLogin.Enabled = false;
                await TryAuth(trimmedUserName);
                connection = null;
                btLogin.Enabled = true;
            }
        }

        private async Task TryAuth(string InputUserName)
        {
            int code;
            string body;
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(2000);
            string json = $"{{\"login\": \"{InputUserName}\"}}";
            StringContent postContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                
                using HttpResponseMessage response = await httpClient.PostAsync(DispatcherAddress + "/auth", postContent);
                

                // Получаем ответ
                body = JsonConvert.DeserializeObject<Message>(await response.Content.ReadAsStringAsync()).address;
                code = (int)response.StatusCode;
            }
            catch
            {
                lbError.Text = Error["DispatcherError"];
                lbError.Visible = true;
                return;
            }

            // 401 - Логин занят
            if (code == 401)
            {
                lbError.Text = Error["TakenUsername"];
                lbError.Visible = true;
            }
            else if (code == 200 && body == "")
            {
                // Пустое тело - серверы не запущены
                lbError.Text = Error["NoServers"];
                lbError.Visible = true;
                
            }
            // Полученный адрес точно верный
            else if (code == 200 && body != "")
            {
                // Добавить возвращаемый объект подключения и закинуть как аргумент в открытие формы
                try
                {
                    TryConnect(body, InputUserName);
                }
                catch(Exception ex)
                {
                    lbError.Text = Error["UnknownError"];
                    lbError.Visible = true;
                    return;
                }
                
                // Скрыть текущую форму
                this.Hide();

                // Открыть главную
                using (Main MainForm = new Main(tbUserInput.Text, connection))
                {
                    if (MainForm.ShowDialog() == DialogResult.OK)
                    {
                        if (Error.ContainsValue(MainForm.ReturnStatus))
                        {
                            lbError.Text = MainForm.ReturnStatus;
                            lbError.Visible = true;
                        }
                    }
                }
                this.Show();
            }
            else
            {
                lbError.Text = Error["UnknownError"];
                lbError.Visible = true;
            }
        }

        private async void TryConnect(string address, string InputUserName)
        {
            // Строка подключения к серверу чата
            connection = new HubConnectionBuilder()
                .WithUrl($"http://{address}/ChatHub")
                .Build();

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromMilliseconds(1000));
            CancellationToken ct = tokenSource.Token;

            await connection.StartAsync();

            

            await connection.InvokeAsync("Auth", InputUserName);
        }

        private void tbUserInput_TextChanged(object sender, EventArgs e)
        {
            lbError.Visible = false;
        }
    }
}
