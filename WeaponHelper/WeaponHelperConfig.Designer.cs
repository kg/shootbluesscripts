namespace ShootBlues.Script {
    partial class WeaponHelperConfig {
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.MinimumChanceToHit = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PreferMyTargets = new System.Windows.Forms.CheckBox();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinimumChanceToHit)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.PreferMyTargets);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.MinimumChanceToHit);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(4, 4);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(277, 82);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Target Selection";
            // 
            // MinimumChanceToHit
            // 
            this.MinimumChanceToHit.Enabled = false;
            this.MinimumChanceToHit.Location = new System.Drawing.Point(171, 24);
            this.MinimumChanceToHit.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumChanceToHit.Name = "MinimumChanceToHit";
            this.MinimumChanceToHit.Size = new System.Drawing.Size(45, 23);
            this.MinimumChanceToHit.TabIndex = 1;
            this.MinimumChanceToHit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(214, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "%";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Minimum chance to hit:";
            // 
            // PreferMyTargets
            // 
            this.PreferMyTargets.AutoSize = true;
            this.PreferMyTargets.Checked = true;
            this.PreferMyTargets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PreferMyTargets.Location = new System.Drawing.Point(10, 54);
            this.PreferMyTargets.Name = "PreferMyTargets";
            this.PreferMyTargets.Size = new System.Drawing.Size(263, 20);
            this.PreferMyTargets.TabIndex = 4;
            this.PreferMyTargets.Text = "Prefer targets I\'m already attacking";
            this.PreferMyTargets.UseVisualStyleBackColor = true;
            // 
            // WeaponHelperConfig
            // 
            this.Controls.Add(this.groupBox4);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(285, 100);
            this.Name = "WeaponHelperConfig";
            this.Size = new System.Drawing.Size(285, 100);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinimumChanceToHit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown MinimumChanceToHit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox PreferMyTargets;



    }
}
