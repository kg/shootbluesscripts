namespace ShootBlues.Script {
    partial class AddTypeDialog {
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
            this.Groups = new System.Windows.Forms.ComboBox();
            this.CancelButton_ = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.Types = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // Groups
            // 
            this.Groups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Groups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Groups.FormattingEnabled = true;
            this.Groups.Location = new System.Drawing.Point(12, 12);
            this.Groups.Name = "Groups";
            this.Groups.Size = new System.Drawing.Size(370, 21);
            this.Groups.TabIndex = 3;
            this.Groups.SelectedIndexChanged += new System.EventHandler(this.Groups_SelectedIndexChanged);
            // 
            // CancelButton_
            // 
            this.CancelButton_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton_.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.CancelButton_.Location = new System.Drawing.Point(282, 185);
            this.CancelButton_.Name = "CancelButton_";
            this.CancelButton_.Size = new System.Drawing.Size(100, 25);
            this.CancelButton_.TabIndex = 5;
            this.CancelButton_.Text = "Cancel";
            this.CancelButton_.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Enabled = false;
            this.OKButton.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.OKButton.Location = new System.Drawing.Point(176, 185);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(100, 25);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Types
            // 
            this.Types.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Types.FormattingEnabled = true;
            this.Types.IntegralHeight = false;
            this.Types.Location = new System.Drawing.Point(12, 39);
            this.Types.Name = "Types";
            this.Types.Size = new System.Drawing.Size(370, 140);
            this.Types.TabIndex = 6;
            this.Types.SelectedIndexChanged += new System.EventHandler(this.Types_SelectedIndexChanged);
            this.Types.DoubleClick += new System.EventHandler(this.Types_DoubleClick);
            // 
            // AddTypeDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.CancelButton_;
            this.ClientSize = new System.Drawing.Size(394, 222);
            this.Controls.Add(this.Types);
            this.Controls.Add(this.Groups);
            this.Controls.Add(this.CancelButton_);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddTypeDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Type";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ComboBox Groups;
        private System.Windows.Forms.Button CancelButton_;
        private System.Windows.Forms.Button OKButton;
        public System.Windows.Forms.ListBox Types;
    }
}