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
            this.FontWidthValue = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.FontWidth = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.FontScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontWidth)).BeginInit();
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
            this.FontScale.Size = new System.Drawing.Size(244, 30);
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
            this.FontSizeValue.Size = new System.Drawing.Size(132, 18);
            this.FontSizeValue.TabIndex = 2;
            this.FontSizeValue.Text = "100%";
            this.FontSizeValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FontWidthValue
            // 
            this.FontWidthValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FontWidthValue.Location = new System.Drawing.Point(115, 58);
            this.FontWidthValue.Name = "FontWidthValue";
            this.FontWidthValue.Size = new System.Drawing.Size(132, 18);
            this.FontWidthValue.TabIndex = 5;
            this.FontWidthValue.Text = "100%";
            this.FontWidthValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Font Width (%):";
            // 
            // FontWidth
            // 
            this.FontWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FontWidth.AutoSize = false;
            this.FontWidth.LargeChange = 25;
            this.FontWidth.Location = new System.Drawing.Point(3, 79);
            this.FontWidth.Maximum = 200;
            this.FontWidth.Minimum = 50;
            this.FontWidth.Name = "FontWidth";
            this.FontWidth.Size = new System.Drawing.Size(244, 30);
            this.FontWidth.SmallChange = 5;
            this.FontWidth.TabIndex = 3;
            this.FontWidth.TickFrequency = 25;
            this.FontWidth.Value = 100;
            this.FontWidth.ValueChanged += new System.EventHandler(this.FontWidth_ValueChanged);
            // 
            // FontSizerConfig
            // 
            this.Controls.Add(this.FontWidthValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FontWidth);
            this.Controls.Add(this.FontSizeValue);
            this.Controls.Add(this.FontSizeCaption);
            this.Controls.Add(this.FontScale);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(200, 120);
            this.Name = "FontSizerConfig";
            this.Size = new System.Drawing.Size(250, 120);
            ((System.ComponentModel.ISupportInitialize)(this.FontScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar FontScale;
        private System.Windows.Forms.Label FontSizeCaption;
        private System.Windows.Forms.Label FontSizeValue;
        private System.Windows.Forms.Label FontWidthValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar FontWidth;

    }
}
