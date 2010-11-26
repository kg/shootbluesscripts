namespace ShootBlues.Script {
    partial class TargetColorsConfig {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetColorsConfig));
            this.List = new System.Windows.Forms.ListBox();
            this.SetColor = new System.Windows.Forms.Button();
            this.ResetToDefault = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // List
            // 
            this.List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.List.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.List.IntegralHeight = false;
            this.List.Location = new System.Drawing.Point(0, 0);
            this.List.Name = "List";
            this.List.Size = new System.Drawing.Size(300, 142);
            this.List.TabIndex = 0;
            this.List.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.List_DrawItem);
            this.List.SelectedIndexChanged += new System.EventHandler(this.List_SelectedIndexChanged);
            // 
            // SetColor
            // 
            this.SetColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SetColor.Enabled = false;
            this.SetColor.Image = ((System.Drawing.Image)(resources.GetObject("SetColor.Image")));
            this.SetColor.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SetColor.Location = new System.Drawing.Point(0, 148);
            this.SetColor.Name = "SetColor";
            this.SetColor.Size = new System.Drawing.Size(115, 27);
            this.SetColor.TabIndex = 2;
            this.SetColor.Text = "Set &Color...";
            this.SetColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SetColor.UseVisualStyleBackColor = true;
            this.SetColor.Click += new System.EventHandler(this.SetColor_Click);
            // 
            // ResetToDefault
            // 
            this.ResetToDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResetToDefault.Enabled = false;
            this.ResetToDefault.Image = ((System.Drawing.Image)(resources.GetObject("ResetToDefault.Image")));
            this.ResetToDefault.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ResetToDefault.Location = new System.Drawing.Point(121, 148);
            this.ResetToDefault.Name = "ResetToDefault";
            this.ResetToDefault.Size = new System.Drawing.Size(150, 27);
            this.ResetToDefault.TabIndex = 3;
            this.ResetToDefault.Text = "Reset To &Default";
            this.ResetToDefault.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ResetToDefault.UseVisualStyleBackColor = true;
            this.ResetToDefault.Click += new System.EventHandler(this.ResetToDefault_Click);
            // 
            // TargetColorsConfig
            // 
            this.Controls.Add(this.ResetToDefault);
            this.Controls.Add(this.SetColor);
            this.Controls.Add(this.List);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(300, 175);
            this.Name = "TargetColorsConfig";
            this.Size = new System.Drawing.Size(300, 175);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox List;
        private System.Windows.Forms.Button SetColor;
        private System.Windows.Forms.Button ResetToDefault;
    }
}
