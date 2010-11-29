namespace ShootBlues.Script {
    partial class FontSizerConfig {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.FontScale = new System.Windows.Forms.TrackBar();
            this.FontSizeCaption = new System.Windows.Forms.Label();
            this.FontSizeValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.FontScale)).BeginInit();
            this.SuspendLayout();
            // 
            // FontScale
            // 
            this.FontScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FontScale.AutoSize = false;
            this.FontScale.LargeChange = 25;
            this.FontScale.Location = new System.Drawing.Point(3, 25);
            this.FontScale.Maximum = 225;
            this.FontScale.Minimum = 75;
            this.FontScale.Name = "FontScale";
            this.FontScale.Size = new System.Drawing.Size(194, 30);
            this.FontScale.SmallChange = 5;
            this.FontScale.TabIndex = 0;
            this.FontScale.TickFrequency = 25;
            this.FontScale.Value = 100;
            this.FontScale.ValueChanged += new System.EventHandler(this.FontScale_ValueChanged);
            // 
            // FontSizeCaption
            // 
            this.FontSizeCaption.AutoSize = true;
            this.FontSizeCaption.Location = new System.Drawing.Point(3, 4);
            this.FontSizeCaption.Name = "FontSizeCaption";
            this.FontSizeCaption.Size = new System.Drawing.Size(106, 16);
            this.FontSizeCaption.TabIndex = 1;
            this.FontSizeCaption.Text = "Font Size (%):";
            // 
            // FontSizeValue
            // 
            this.FontSizeValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FontSizeValue.Location = new System.Drawing.Point(115, 4);
            this.FontSizeValue.Name = "FontSizeValue";
            this.FontSizeValue.Size = new System.Drawing.Size(82, 18);
            this.FontSizeValue.TabIndex = 2;
            this.FontSizeValue.Text = "100%";
            this.FontSizeValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FontSizerConfig
            // 
            this.Controls.Add(this.FontSizeValue);
            this.Controls.Add(this.FontSizeCaption);
            this.Controls.Add(this.FontScale);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(200, 70);
            this.Name = "FontSizerConfig";
            this.Size = new System.Drawing.Size(200, 70);
            ((System.ComponentModel.ISupportInitialize)(this.FontScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar FontScale;
        private System.Windows.Forms.Label FontSizeCaption;
        private System.Windows.Forms.Label FontSizeValue;

    }
}
