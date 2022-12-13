using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client
{
    public partial class Main : Form
    {
        private string _address;
        private string _userName;
        public string ReturnStatus = null!;
        HubConnection _connection = null!;

        public Main(string userName, string address)
        {
            _address = address;
            _userName = userName;
            InitializeComponent();
            tbInput.KeyDown += new KeyEventHandler(tb_KeyDown);
        }
        private async void Main_Load(object sender, EventArgs e)
        {
            SetComponentsEnableability(false);

            var isConnected = await TryConnectToServer(_address, _userName);


            if (isConnected)
            {
                try
                {
                    await _connection.InvokeAsync("GetHistory");
                    SetComponentsEnableability(true);
                    return;
                }
                catch { }
            }

            await TryToReconnectOrClose();
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                await _connection.StopAsync();
            }
            catch { }

            base.OnFormClosing(e);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btSend_Click(sender, e);
            }
        }

        private void SetEvents(HubConnection connection)
        {
            // User Messages
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                BeginInvoke(() =>
                {
                    lboxChat.Items.Add($"{user}: {message}");
                    lboxChat.SelectedIndex = lboxChat.Items.Count - 1;
                    lboxChat.ClearSelected();
                });
            });

            // History Messages
            connection.On<List<HistMessage>>("ReceiveHistory", (messages) =>
            {
                this.Invoke(new Action(() => SetChatHistory(messages)));
            });

            connection.Closed += async (error) =>
            {
                if (error is null)
                {
                    return;
                }

                try
                {
                    this.Invoke(new Action(() => SetComponentsEnableability(false)));
                }
                catch (Exception)
                {
                    Console.WriteLine();
                }

                MessageBox.Show("Disconnected from the server\nTrying to reconnect...");

                await TryToReconnectOrClose();
            };
        }

        private async void btSend_Click(object sender, EventArgs e)
        {
            var trimmedMessage = tbInput.Text.Trim();

            if (string.IsNullOrEmpty(trimmedMessage))
            {
                return;
            }

            try
            {
                await _connection.InvokeAsync("SendMessage", _userName, tbInput.Text.Trim());
                tbInput.Text = "";
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // TODO remove in release
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            try
            {
                await _connection.InvokeAsync("SendMessage", _userName, tbInput.Text.Trim());
            }
            catch
            {
                await TryToReconnectOrClose();
            }

            tbInput.Text = ""; // Clean input
        }

        private void SetChatHistory(List<HistMessage> messages)
        {
            lboxChat.ClearSelected();
            lboxChat.Items.Clear();

            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var item = messages[i];
                lboxChat.Items.Add($"{item.User}: {item.Content}");
            }
        }

        /// <summary>
        /// Trying to reconnect or send message to user about error and close this form.
        /// </summary>
        private async Task TryToReconnectOrClose()
        {
            using CancellationTokenSource cancelTokenSource = new();
            cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(10)); // Timeout for reconnect
            CancellationToken cancelToken = cancelTokenSource.Token;
            await Task.Delay(300); // Time in case server restarting

            async Task<(int, string, bool)> Connect(CancellationToken cancellationToken)
            {
                (int code, string address) response = (0, string.Empty);
                bool isConnectedToServer = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Trying to get another server address
                    try
                    {
                        response = await AuthClient.Auth(_userName);
                    }
                    catch
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(300));
                        continue;
                    }

                    // User with name already logged in or no servers available
                    if (response.code == 401 || (response.code == 200 && string.IsNullOrEmpty(response.address)))
                    {
                        break;
                    }

                    // Received address, trying connect to server
                    if (response.code == 200 && !string.IsNullOrEmpty(response.address))
                    {
                        isConnectedToServer = await TryConnectToServer(response.address, _userName);
                    }

                    // Trying to load chat history
                    if (isConnectedToServer)
                    {
                        try
                        {
                            //this.Invoke(new Action(async () => await _connection.InvokeAsync("GetHistory")));
                            await _connection.InvokeAsync("GetHistory");
                            break;
                        }
                        catch
                        {
                            isConnectedToServer = false;
                        }
                    }
                }

                return (response.code, response.address, isConnectedToServer);
            }
            (int code, string address, bool isConnectedToServer) res = await Connect(cancelToken);

            // Connected to server and received chat history -> enable form for user
            if (res.isConnectedToServer)
            {
                this.Invoke(new Action(() => SetComponentsEnableability(true)));
                return;
            }

            switch (res.code)
            {
                case 401:
                    MessageBox.Show("This login is no longer available...", "Error!", MessageBoxButtons.OK);
                    break;
                // Status code 200 and address is empty string - No servers available
                case 200 when string.IsNullOrEmpty(res.address):
                    MessageBox.Show("No servers available...", "Error!", MessageBoxButtons.OK);
                    break;
                // Status code and address is non empty string - Received server address
                case 200:
                    MessageBox.Show("Couldn't connect to servers (maybe internet problem)", "Error!", MessageBoxButtons.OK);
                    break;
                default:
                    MessageBox.Show("An unknown error has occured");
                    break;
            }

            this.Invoke(new Action(() => Close()));
        }

        private async Task<bool> TryConnectToServer(string address, string userName)
        {
            // Connectin to the server
            _connection = new HubConnectionBuilder()
                .WithUrl($"http://{address}/ChatHub")
                .Build();

            SetEvents(_connection);

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromMilliseconds(2000));
            CancellationToken ct = tokenSource.Token;

            for (var i = 0; i < 2; i++)
            {
                try
                {
                    await AuthToServer(ct, userName);
                    return true;
                }
                catch (OperationCanceledException) { }
            }

            return false;
        }

        private async Task AuthToServer(CancellationToken token, string userName)
        {
            try
            {
                await _connection.StartAsync(token);
                await _connection.InvokeAsync("Auth", userName);
            }
            catch
            {
                throw;
            }
        }

        private void lboxChat_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)e.Graphics.MeasureString(lboxChat.Items[e.Index].ToString(), lboxChat.Font, lboxChat.Width).Height;
        }

        private void lboxChat_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.Index >= 0)
                e.Graphics.DrawString(lboxChat.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }

        private void SetComponentsEnableability(bool enable)
        {
            lboxChat.Enabled = tbInput.Enabled = btSend.Enabled = enable;
            lboxChat.ClearSelected();
        }
    }
}