namespace ShootBlues.Script {
    partial class SelectModuleDialog {
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
            this.Modules = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // CancelButton_
            // 
            this.CancelButton_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton_.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.CancelButton_.Location = new System.Drawing.Point(342, 44);
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
            this.OKButton.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.OKButton.Location = new System.Drawing.Point(236, 44);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(100, 25);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Modules
            // 
            this.Modules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Modules.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.Modules.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Modules.FormattingEnabled = true;
            this.Modules.Location = new System.Drawing.Point(12, 12);
            this.Modules.Name = "Modules";
            this.Modules.Size = new System.Drawing.Size(430, 24);
            this.Modules.TabIndex = 0;
            // 
            // SelectModuleDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelButton_;
            this.ClientSize = new System.Drawing.Size(454, 81);
            this.Controls.Add(this.Modules);
            this.Controls.Add(this.CancelButton_);
            this.Controls.Add(this.OKButton);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectModuleDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Module";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CancelButton_;
        private System.Windows.Forms.Button OKButton;
        public System.Windows.Forms.ComboBox Modules;
    }
}