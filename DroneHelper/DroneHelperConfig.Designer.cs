namespace ShootBlues.Script {
    partial class DroneHelperConfig {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ConfigurePriorities = new System.Windows.Forms.Button();
            this.WhenIdle = new System.Windows.Forms.CheckBox();
            this.WhenTargetLost = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.RedeployAfterSeconds = new System.Windows.Forms.NumericUpDown();
            this.RedeployAfter = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.RecallShieldThreshold = new System.Windows.Forms.NumericUpDown();
            this.RecallIfShieldsBelow = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RedeployAfterSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecallShieldThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ConfigurePriorities);
            this.groupBox1.Controls.Add(this.WhenIdle);
            this.groupBox1.Controls.Add(this.WhenTargetLost);
            this.groupBox1.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(280, 76);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Auto Attack";
            // 
            // ConfigurePriorities
            // 
            this.ConfigurePriorities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfigurePriorities.Location = new System.Drawing.Point(7, 44);
            this.ConfigurePriorities.Name = "ConfigurePriorities";
            this.ConfigurePriorities.Size = new System.Drawing.Size(265, 25);
            this.ConfigurePriorities.TabIndex = 2;
            this.ConfigurePriorities.Text = "Edit Target Priorities";
            this.ConfigurePriorities.UseVisualStyleBackColor = true;
            this.ConfigurePriorities.Click += new System.EventHandler(this.ConfigurePriorities_Click);
            // 
            // WhenIdle
            // 
            this.WhenIdle.AutoSize = true;
            this.WhenIdle.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.WhenIdle.Checked = true;
            this.WhenIdle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WhenIdle.Location = new System.Drawing.Point(8, 22);
            this.WhenIdle.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.WhenIdle.Name = "WhenIdle";
            this.WhenIdle.Size = new System.Drawing.Size(93, 20);
            this.WhenIdle.TabIndex = 0;
            this.WhenIdle.Text = "When Idle";
            this.WhenIdle.UseVisualStyleBackColor = true;
            // 
            // WhenTargetLost
            // 
            this.WhenTargetLost.AutoSize = true;
            this.WhenTargetLost.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.WhenTargetLost.Checked = true;
            this.WhenTargetLost.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WhenTargetLost.Location = new System.Drawing.Point(109, 22);
            this.WhenTargetLost.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.WhenTargetLost.Name = "WhenTargetLost";
            this.WhenTargetLost.Size = new System.Drawing.Size(146, 20);
            this.WhenTargetLost.TabIndex = 1;
            this.WhenTargetLost.Text = "When Target Lost";
            this.WhenTargetLost.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.RedeployAfterSeconds);
            this.groupBox4.Controls.Add(this.RedeployAfter);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.RecallShieldThreshold);
            this.groupBox4.Controls.Add(this.RecallIfShieldsBelow);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(0, 84);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(280, 81);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Auto Recall";
            // 
            // RedeployAfterSeconds
            // 
            this.RedeployAfterSeconds.Enabled = false;
            this.RedeployAfterSeconds.Location = new System.Drawing.Point(141, 53);
            this.RedeployAfterSeconds.Margin = new System.Windows.Forms.Padding(4);
            this.RedeployAfterSeconds.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.RedeployAfterSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RedeployAfterSeconds.Name = "RedeployAfterSeconds";
            this.RedeployAfterSeconds.Size = new System.Drawing.Size(45, 23);
            this.RedeployAfterSeconds.TabIndex = 4;
            this.RedeployAfterSeconds.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // RedeployAfter
            // 
            this.RedeployAfter.AutoSize = true;
            this.RedeployAfter.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RedeployAfter.Location = new System.Drawing.Point(8, 54);
            this.RedeployAfter.Margin = new System.Windows.Forms.Padding(4);
            this.RedeployAfter.Name = "RedeployAfter";
            this.RedeployAfter.Size = new System.Drawing.Size(132, 20);
            this.RedeployAfter.TabIndex = 3;
            this.RedeployAfter.Text = "Re-Deploy After";
            this.RedeployAfter.UseVisualStyleBackColor = true;
            this.RedeployAfter.CheckedChanged += new System.EventHandler(this.RedeployWhenShieldsAbove_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "second(s)";
            // 
            // RecallShieldThreshold
            // 
            this.RecallShieldThreshold.Enabled = false;
            this.RecallShieldThreshold.Location = new System.Drawing.Point(140, 22);
            this.RecallShieldThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.RecallShieldThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RecallShieldThreshold.Name = "RecallShieldThreshold";
            this.RecallShieldThreshold.Size = new System.Drawing.Size(45, 23);
            this.RecallShieldThreshold.TabIndex = 1;
            this.RecallShieldThreshold.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // RecallIfShieldsBelow
            // 
            this.RecallIfShieldsBelow.AutoSize = true;
            this.RecallIfShieldsBelow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RecallIfShieldsBelow.Location = new System.Drawing.Point(8, 23);
            this.RecallIfShieldsBelow.Margin = new System.Windows.Forms.Padding(4);
            this.RecallIfShieldsBelow.Name = "RecallIfShieldsBelow";
            this.RecallIfShieldsBelow.Size = new System.Drawing.Size(131, 20);
            this.RecallIfShieldsBelow.TabIndex = 0;
            this.RecallIfShieldsBelow.Text = "If Shields Below";
            this.RecallIfShieldsBelow.UseVisualStyleBackColor = true;
            this.RecallIfShieldsBelow.CheckedChanged += new System.EventHandler(this.RecallIfShieldsBelow_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(183, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "%";
            // 
            // DroneHelperConfig
            // 
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(280, 165);
            this.Name = "DroneHelperConfig";
            this.Size = new System.Drawing.Size(280, 165);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RedeployAfterSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecallShieldThreshold)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown RecallShieldThreshold;
        private System.Windows.Forms.CheckBox RecallIfShieldsBelow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox WhenIdle;
        private System.Windows.Forms.CheckBox WhenTargetLost;
        private System.Windows.Forms.Button ConfigurePriorities;
        private System.Windows.Forms.NumericUpDown RedeployAfterSeconds;
        private System.Windows.Forms.CheckBox RedeployAfter;
        private System.Windows.Forms.Label label2;
    }
}
