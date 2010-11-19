namespace ShootBlues.Script {
    partial class AddGroupDialog {
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
            this.Group = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // CancelButton_
            // 
            this.CancelButton_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton_.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.CancelButton_.Location = new System.Drawing.Point(282, 47);
            this.CancelButton_.Name = "CancelButton_";
            this.CancelButton_.Size = new System.Drawing.Size(100, 25);
            this.CancelButton_.TabIndex = 2;
            this.CancelButton_.Text = "Cancel";
            this.CancelButton_.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Enabled = false;
            this.OKButton.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.OKButton.Location = new System.Drawing.Point(176, 47);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(100, 25);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Group
            // 
            this.Group.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Group.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Group.FormattingEnabled = true;
            this.Group.Location = new System.Drawing.Point(12, 12);
            this.Group.Name = "Group";
            this.Group.Size = new System.Drawing.Size(370, 24);
            this.Group.TabIndex = 0;
            this.Group.SelectedIndexChanged += new System.EventHandler(this.Group_SelectedIndexChanged);
            // 
            // AddGroupDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.CancelButton_;
            this.ClientSize = new System.Drawing.Size(394, 84);
            this.Controls.Add(this.Group);
            this.Controls.Add(this.CancelButton_);
            this.Controls.Add(this.OKButton);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddGroupDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Group";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CancelButton_;
        private System.Windows.Forms.Button OKButton;
        public System.Windows.Forms.ComboBox Group;
    }
}