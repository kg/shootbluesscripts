namespace ShootBlues.Script {
    partial class ActiveTankerConfig {
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
            this.KeepShieldsFull = new System.Windows.Forms.CheckBox();
            this.KeepArmorFull = new System.Windows.Forms.CheckBox();
            this.KeepStructureFull = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // KeepShieldsFull
            // 
            this.KeepShieldsFull.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.KeepShieldsFull.Checked = true;
            this.KeepShieldsFull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.KeepShieldsFull.Location = new System.Drawing.Point(4, 4);
            this.KeepShieldsFull.Margin = new System.Windows.Forms.Padding(4);
            this.KeepShieldsFull.Name = "KeepShieldsFull";
            this.KeepShieldsFull.Size = new System.Drawing.Size(160, 20);
            this.KeepShieldsFull.TabIndex = 3;
            this.KeepShieldsFull.Text = "Keep Shields Full";
            this.KeepShieldsFull.UseVisualStyleBackColor = true;
            this.KeepShieldsFull.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // KeepArmorFull
            // 
            this.KeepArmorFull.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.KeepArmorFull.Checked = true;
            this.KeepArmorFull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.KeepArmorFull.Location = new System.Drawing.Point(4, 32);
            this.KeepArmorFull.Margin = new System.Windows.Forms.Padding(4);
            this.KeepArmorFull.Name = "KeepArmorFull";
            this.KeepArmorFull.Size = new System.Drawing.Size(160, 20);
            this.KeepArmorFull.TabIndex = 6;
            this.KeepArmorFull.Text = "Keep Armor Full";
            this.KeepArmorFull.UseVisualStyleBackColor = true;
            this.KeepArmorFull.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // KeepStructureFull
            // 
            this.KeepStructureFull.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.KeepStructureFull.Checked = true;
            this.KeepStructureFull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.KeepStructureFull.Location = new System.Drawing.Point(4, 60);
            this.KeepStructureFull.Margin = new System.Windows.Forms.Padding(4);
            this.KeepStructureFull.Name = "KeepStructureFull";
            this.KeepStructureFull.Size = new System.Drawing.Size(160, 20);
            this.KeepStructureFull.TabIndex = 9;
            this.KeepStructureFull.Text = "Keep Structure Full";
            this.KeepStructureFull.UseVisualStyleBackColor = true;
            this.KeepStructureFull.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // ActiveTankerConfig
            // 
            this.Controls.Add(this.KeepStructureFull);
            this.Controls.Add(this.KeepArmorFull);
            this.Controls.Add(this.KeepShieldsFull);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(200, 85);
            this.Name = "ActiveTankerConfig";
            this.Size = new System.Drawing.Size(200, 85);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox KeepShieldsFull;
        private System.Windows.Forms.CheckBox KeepArmorFull;
        private System.Windows.Forms.CheckBox KeepStructureFull;

    }
}
