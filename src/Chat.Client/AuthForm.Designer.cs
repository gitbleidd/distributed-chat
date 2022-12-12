namespace Chat.Client
{
    partial class AuthForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btLogin = new System.Windows.Forms.Button();
            this.lbEnterUser = new System.Windows.Forms.Label();
            this.tbUserInput = new System.Windows.Forms.TextBox();
            this.lbError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btLogin
            // 
            this.btLogin.Location = new System.Drawing.Point(396, 58);
            this.btLogin.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btLogin.Name = "btLogin";
            this.btLogin.Size = new System.Drawing.Size(139, 49);
            this.btLogin.TabIndex = 0;
            this.btLogin.Text = "Login";
            this.btLogin.UseVisualStyleBackColor = true;
            this.btLogin.Click += new System.EventHandler(this.btLogin_Click);
            // 
            // lbEnterUser
            // 
            this.lbEnterUser.AutoSize = true;
            this.lbEnterUser.Location = new System.Drawing.Point(22, 19);
            this.lbEnterUser.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbEnterUser.Name = "lbEnterUser";
            this.lbEnterUser.Size = new System.Drawing.Size(186, 32);
            this.lbEnterUser.TabIndex = 1;
            this.lbEnterUser.Text = "Enter username:";
            // 
            // tbUserInput
            // 
            this.tbUserInput.Location = new System.Drawing.Point(28, 58);
            this.tbUserInput.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tbUserInput.Name = "tbUserInput";
            this.tbUserInput.Size = new System.Drawing.Size(325, 39);
            this.tbUserInput.TabIndex = 2;
            this.tbUserInput.TextChanged += new System.EventHandler(this.tbUserInput_TextChanged);
            // 
            // lbError
            // 
            this.lbError.AutoSize = true;
            this.lbError.ForeColor = System.Drawing.Color.Red;
            this.lbError.Location = new System.Drawing.Point(28, 113);
            this.lbError.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbError.Name = "lbError";
            this.lbError.Size = new System.Drawing.Size(64, 32);
            this.lbError.TabIndex = 0;
            this.lbError.Text = "Error";
            this.lbError.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbError.Visible = false;
            // 
            // AuthForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 162);
            this.Controls.Add(this.lbError);
            this.Controls.Add(this.tbUserInput);
            this.Controls.Add(this.lbEnterUser);
            this.Controls.Add(this.btLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.Name = "AuthForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Authorization";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btLogin;
        private Label lbEnterUser;
        private TextBox tbUserInput;
        private Label lbError;
    }
}