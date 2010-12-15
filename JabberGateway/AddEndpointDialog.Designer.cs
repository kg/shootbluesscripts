namespace ShootBlues.Script {
    partial class AddEndpointDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.CancelButton_ = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.Server = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.EndpointName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Username = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Resource = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SendToGroupChatGroup = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ChatChannel = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.ChatAlias = new System.Windows.Forms.TextBox();
            this.SendToGroupChat = new System.Windows.Forms.RadioButton();
            this.SendToUser = new System.Windows.Forms.RadioButton();
            this.SendToUserGroup = new System.Windows.Forms.GroupBox();
            this.ToUsername = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SendToGroupChatGroup.SuspendLayout();
            this.SendToUserGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // CancelButton_
            // 
            this.CancelButton_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton_.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.CancelButton_.Location = new System.Drawing.Point(282, 322);
            this.CancelButton_.Name = "CancelButton_";
            this.CancelButton_.Size = new System.Drawing.Size(100, 25);
            this.CancelButton_.TabIndex = 4;
            this.CancelButton_.Text = "Cancel";
            this.CancelButton_.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Enabled = false;
            this.OKButton.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.OKButton.Location = new System.Drawing.Point(176, 322);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(100, 25);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Server
            // 
            this.Server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Server.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Server.Location = new System.Drawing.Point(90, 7);
            this.Server.Name = "Server";
            this.Server.Size = new System.Drawing.Size(274, 23);
            this.Server.TabIndex = 1;
            this.Server.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // EndpointName
            // 
            this.EndpointName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.EndpointName.Location = new System.Drawing.Point(68, 12);
            this.EndpointName.Name = "EndpointName";
            this.EndpointName.Size = new System.Drawing.Size(314, 23);
            this.EndpointName.TabIndex = 1;
            this.EndpointName.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Username:";
            // 
            // Username
            // 
            this.Username.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Username.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Username.Location = new System.Drawing.Point(90, 36);
            this.Username.Name = "Username";
            this.Username.Size = new System.Drawing.Size(274, 23);
            this.Username.TabIndex = 3;
            this.Username.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Password:";
            // 
            // Password
            // 
            this.Password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Password.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Password.Location = new System.Drawing.Point(90, 65);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '●';
            this.Password.Size = new System.Drawing.Size(274, 23);
            this.Password.TabIndex = 5;
            this.Password.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Resource:";
            // 
            // Resource
            // 
            this.Resource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Resource.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Resource.Location = new System.Drawing.Point(90, 94);
            this.Resource.Name = "Resource";
            this.Resource.Size = new System.Drawing.Size(274, 23);
            this.Resource.TabIndex = 7;
            this.Resource.Text = "ShootBlues";
            this.Resource.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.Server);
            this.groupBox1.Controls.Add(this.Resource);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.Username);
            this.groupBox1.Controls.Add(this.Password);
            this.groupBox1.Font = new System.Drawing.Font("MS Reference Sans Serif", 0.0001F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(370, 124);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // SendToGroupChatGroup
            // 
            this.SendToGroupChatGroup.Controls.Add(this.label6);
            this.SendToGroupChatGroup.Controls.Add(this.ChatChannel);
            this.SendToGroupChatGroup.Controls.Add(this.label7);
            this.SendToGroupChatGroup.Controls.Add(this.ChatAlias);
            this.SendToGroupChatGroup.Enabled = false;
            this.SendToGroupChatGroup.Location = new System.Drawing.Point(12, 171);
            this.SendToGroupChatGroup.Name = "SendToGroupChatGroup";
            this.SendToGroupChatGroup.Size = new System.Drawing.Size(370, 81);
            this.SendToGroupChatGroup.TabIndex = 3;
            this.SendToGroupChatGroup.TabStop = false;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(9, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 16);
            this.label6.TabIndex = 2;
            this.label6.Text = "Alias:";
            // 
            // ChatChannel
            // 
            this.ChatChannel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ChatChannel.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatChannel.Location = new System.Drawing.Point(93, 22);
            this.ChatChannel.Name = "ChatChannel";
            this.ChatChannel.Size = new System.Drawing.Size(271, 23);
            this.ChatChannel.TabIndex = 1;
            this.ChatChannel.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(9, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 16);
            this.label7.TabIndex = 0;
            this.label7.Text = "Channel:";
            // 
            // ChatAlias
            // 
            this.ChatAlias.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ChatAlias.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatAlias.Location = new System.Drawing.Point(93, 51);
            this.ChatAlias.Name = "ChatAlias";
            this.ChatAlias.Size = new System.Drawing.Size(271, 23);
            this.ChatAlias.TabIndex = 3;
            this.ChatAlias.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // SendToGroupChat
            // 
            this.SendToGroupChat.AutoSize = true;
            this.SendToGroupChat.Location = new System.Drawing.Point(21, 170);
            this.SendToGroupChat.Name = "SendToGroupChat";
            this.SendToGroupChat.Size = new System.Drawing.Size(160, 20);
            this.SendToGroupChat.TabIndex = 4;
            this.SendToGroupChat.Text = "Send To Group Chat";
            this.SendToGroupChat.UseVisualStyleBackColor = true;
            this.SendToGroupChat.CheckedChanged += new System.EventHandler(this.SendToGroupChat_CheckedChanged);
            // 
            // SendToUser
            // 
            this.SendToUser.AutoSize = true;
            this.SendToUser.Location = new System.Drawing.Point(21, 257);
            this.SendToUser.Name = "SendToUser";
            this.SendToUser.Size = new System.Drawing.Size(115, 20);
            this.SendToUser.TabIndex = 6;
            this.SendToUser.Text = "Send To User";
            this.SendToUser.UseVisualStyleBackColor = true;
            this.SendToUser.CheckedChanged += new System.EventHandler(this.SendToUser_CheckedChanged);
            // 
            // SendToUserGroup
            // 
            this.SendToUserGroup.Controls.Add(this.ToUsername);
            this.SendToUserGroup.Controls.Add(this.label9);
            this.SendToUserGroup.Enabled = false;
            this.SendToUserGroup.Location = new System.Drawing.Point(12, 258);
            this.SendToUserGroup.Name = "SendToUserGroup";
            this.SendToUserGroup.Size = new System.Drawing.Size(370, 52);
            this.SendToUserGroup.TabIndex = 5;
            this.SendToUserGroup.TabStop = false;
            // 
            // ToUsername
            // 
            this.ToUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ToUsername.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToUsername.Location = new System.Drawing.Point(93, 22);
            this.ToUsername.Name = "ToUsername";
            this.ToUsername.Size = new System.Drawing.Size(271, 23);
            this.ToUsername.TabIndex = 1;
            this.ToUsername.TextChanged += new System.EventHandler(this.RefreshEnabledState);
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(9, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 16);
            this.label9.TabIndex = 0;
            this.label9.Text = "Username:";
            // 
            // AddEndpointDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelButton_;
            this.ClientSize = new System.Drawing.Size(394, 359);
            this.Controls.Add(this.SendToUser);
            this.Controls.Add(this.SendToUserGroup);
            this.Controls.Add(this.SendToGroupChat);
            this.Controls.Add(this.SendToGroupChatGroup);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.EndpointName);
            this.Controls.Add(this.CancelButton_);
            this.Controls.Add(this.OKButton);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddEndpointDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Endpoint";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.SendToGroupChatGroup.ResumeLayout(false);
            this.SendToGroupChatGroup.PerformLayout();
            this.SendToUserGroup.ResumeLayout(false);
            this.SendToUserGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelButton_;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.TextBox Server;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox EndpointName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Username;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Resource;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox SendToGroupChatGroup;
        private System.Windows.Forms.RadioButton SendToGroupChat;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox ChatChannel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox ChatAlias;
        private System.Windows.Forms.RadioButton SendToUser;
        private System.Windows.Forms.GroupBox SendToUserGroup;
        private System.Windows.Forms.TextBox ToUsername;
        private System.Windows.Forms.Label label9;
    }
}