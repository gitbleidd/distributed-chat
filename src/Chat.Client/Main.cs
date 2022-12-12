//String Trim()
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client
{
    public partial class Main : Form
    {
        private string userName;
        public string ReturnStatus;
        HubConnection connection;
        public Main(string userName, HubConnection connection)
        {
            this.userName = userName;
            this.connection = connection;
            InitializeComponent();
            setEvents(connection);
            //Ask Server for history

        }

        private async void setEvents(HubConnection connection)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.BeginInvoke(() =>
                {
                    lboxChat.Items.Add($"{user}: {message.Trim()}");
                });
            });

            connection.Closed += async (error) =>
            {
                // Бля это реконнект что ли страшный такой
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        // TEMP
        private async void btSend_Click(object sender, EventArgs e)
        {
            if (tbInput.Text == "" || tbInput.Text == null) return;
            try
            {
                await connection.InvokeAsync("SendMessage", userName, tbInput.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // Ждем секунду для повторного подключения
                await Task.Run
                (
                    async () =>
                    await Task.Delay(1000)
                );

                try
                {
                    await connection.InvokeAsync("SendMessage", userName, tbInput.Text.Trim());
                }
                catch (Exception ex1)
                {
                    MessageBox.Show("Сервер не ожил, тикаем");
                    ReturnStatus = AuthForm.Error["UnknownError"];
                    this.Close();
                }
                //Отвал сервера - 1 реконнект и тикаем обратно на авторизацию
            }
        }

        private void setChatHistory(Dictionary<string, string> messages)
        {
            lboxChat.Items.Clear();
            foreach(var item in messages)
            {
                lboxChat.Items.Add($"{item.Key}: {item.Value}");
            }
        }
    }
}