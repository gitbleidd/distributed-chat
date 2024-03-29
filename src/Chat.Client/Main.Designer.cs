﻿namespace Chat.Client
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
            this.btSend.Location = new System.Drawing.Point(611, 370);
            this.btSend.Margin = new System.Windows.Forms.Padding(6);
            this.btSend.Name = "btSend";
            this.btSend.Size = new System.Drawing.Size(106, 49);
            this.btSend.TabIndex = 0;
            this.btSend.Text = "Send";
            this.btSend.UseVisualStyleBackColor = true;
            this.btSend.Click += new System.EventHandler(this.btSend_Click);
            // 
            // tbInput
            // 
            this.tbInput.Location = new System.Drawing.Point(10, 380);
            this.tbInput.Margin = new System.Windows.Forms.Padding(6);
            this.tbInput.Name = "tbInput";
            this.tbInput.Size = new System.Drawing.Size(589, 39);
            this.tbInput.TabIndex = 1;
            // 
            // lboxChat
            // 
            this.lboxChat.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lboxChat.FormattingEnabled = true;
            this.lboxChat.ItemHeight = 32;
            this.lboxChat.Location = new System.Drawing.Point(10, 30);
            this.lboxChat.Margin = new System.Windows.Forms.Padding(6);
            this.lboxChat.Name = "lboxChat";
            this.lboxChat.Size = new System.Drawing.Size(708, 324);
            this.lboxChat.TabIndex = 2;
            this.lboxChat.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lboxChat_DrawItem);
            this.lboxChat.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lboxChat_MeasureItem);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 434);
            this.Controls.Add(this.lboxChat);
            this.Controls.Add(this.tbInput);
            this.Controls.Add(this.btSend);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Distributed Chat";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btSend;
        private TextBox tbInput;
        private ListBox lboxChat;
    }
}