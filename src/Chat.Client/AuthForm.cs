// Пасхалка
// gaiboi
namespace Chat.Client
{
    public partial class AuthForm : Form
    {
        // Errors dictionary
        public static Dictionary<string, string> Errors = new()
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
                lbError.Text = Errors["LoginLength"];
                lbError.Visible = true;
                return;
            }

            string trimmedUserName = tbUserInput.Text.Trim();

            if (string.IsNullOrEmpty(trimmedUserName))
            {
                lbError.Text = Errors["EmptyField"];
                lbError.Visible = true;
            }
            else
            {
                // Trying to get server address
                lbError.Visible = false;
                btLogin.Enabled = false;
                await TryAuth(trimmedUserName);
                btLogin.Enabled = true;
            }
        }

        private async Task TryAuth(string inputUserName)
        {
            (int code, string address) response;

            try
            {
                response = await AuthClient.Auth(inputUserName);
            }
            catch
            {
                lbError.Text = Errors["DispatcherError"];
                lbError.Visible = true;
                return;
            }

            switch (response.code)
            {
                // Status code 401 - This login is used
                case 401:
                    lbError.Text = Errors["TakenUsername"];
                    lbError.Visible = true;
                    break;
                // Status code 200 and address is empty string - No servers available
                case 200 when response.address == string.Empty:
                    lbError.Text = Errors["NoServers"];
                    lbError.Visible = true;
                    break;
                // Status code and address is non empty string - Received server address
                case 200:
                    Hide();

                    // Open chat form
                    using (Main MainForm = new(inputUserName, response.address))
                    {
                        if (MainForm.ShowDialog() == DialogResult.OK)
                        {
                            if (Errors.ContainsValue(MainForm.ReturnStatus))
                            {
                                lbError.Text = MainForm.ReturnStatus;
                                lbError.Visible = true;
                            }
                        }
                    }

                    Show();
                    break;
                default:
                    lbError.Text = Errors["UnknownError"];
                    lbError.Visible = true;
                    break;
            }
        }

        private void tbUserInput_TextChanged(object sender, EventArgs e)
        {
            lbError.Visible = false;
        }
    }
}