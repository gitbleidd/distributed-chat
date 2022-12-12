namespace Chat.Client
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btSend = new System.Windows.Forms.Button();
            this.tbInput = new System.Windows.Forms.TextBox();
            this.lboxChat = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btSend
            // 
            this.btSend.Location = new System.Drawing.Point(285, 183);
            this.btSend.Name = "btSend";
            this.btSend.Size = new System.Drawing.Size(57, 23);
            this.btSend.TabIndex = 0;
            this.btSend.Text = "Send";
            this.btSend.UseVisualStyleBackColor = true;
            this.btSend.Click += new System.EventHandler(this.btSend_Click);
            // 
            // tbInput
            // 
            this.tbInput.Location = new System.Drawing.Point(12, 183);
            this.tbInput.Name = "tbInput";
            this.tbInput.Size = new System.Drawing.Size(258, 23);
            this.tbInput.TabIndex = 1;
            // 
            // lboxChat
            // 
            this.lboxChat.FormattingEnabled = true;
            this.lboxChat.ItemHeight = 15;
            this.lboxChat.Location = new System.Drawing.Point(12, 12);
            this.lboxChat.Name = "lboxChat";
            this.lboxChat.Size = new System.Drawing.Size(330, 154);
            this.lboxChat.TabIndex = 2;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 215);
            this.Controls.Add(this.lboxChat);
            this.Controls.Add(this.tbInput);
            this.Controls.Add(this.btSend);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "Distributed Chat";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btSend;
        private TextBox tbInput;
        private ListBox lboxChat;
    }
}