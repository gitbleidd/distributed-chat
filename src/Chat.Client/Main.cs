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
            tbInput.KeyDown += new KeyEventHandler(tb_KeyDown);
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                await connection.StopAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.OnFormClosing(e);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btSend_Click(sender, e);
            }
        }

        private async void setEvents(HubConnection connection)
        {
            // User Messages
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.BeginInvoke(() =>
                {
                    lboxChat.Items.Add($"{user}: {message.Trim()}");
                });
            });

            //History Messages
            connection.On<List<HistMessage>>("GetHistory", (messages) =>
            {
                this.BeginInvoke(() =>
                {
                    setChatHistory(messages);
                });
            });

            connection.Closed += async (error) =>
            {
                MessageBox.Show("Отвал жопы");
                try
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("Auth", userName);
                }
                catch(Exception e)
                {
                    
                }
            };

            // Получить историю
            await connection.InvokeAsync("GetHistory");

            /*connection.Closed += async (error) =>
            {
                MessageBox.Show("Пытаемся подключиться снова");
                try
                {
                    
                    await connection.StartAsync();
                    await connection.InvokeAsync("Auth", userName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Сервер не ожил, тикаем");
                    ReturnStatus = AuthForm.Error["UnknownError"];
                    this.Close();
                }
            };*/
        }

        // TEMP
        private async void btSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbInput.Text)) return;
            try
            {
                await connection.InvokeAsync("SendMessage", userName, tbInput.Text.Trim());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                try
                {
                    await connection.InvokeAsync("SendMessage", userName, tbInput.Text.Trim());
                }
                catch (Exception ex1)
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("Auth", userName);
                    MessageBox.Show("Сервер не ожил, тикаем");
                    ReturnStatus = AuthForm.Error["UnknownError"];
                    this.Close();
                }
                //Отвал сервера - 1 реконнект и тикаем обратно на авторизацию
            }
            tbInput.Text = "";
        }

        private void setChatHistory(List<HistMessage> messages)
        {
            lboxChat.Items.Clear();
            foreach(var item in messages)
            {
                lboxChat.Items.Add($"{item.User}: {item.Content}");
            }
        }
    }
}