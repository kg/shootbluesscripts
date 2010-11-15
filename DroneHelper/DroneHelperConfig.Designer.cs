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
            this.AutoAttackWhat = new System.Windows.Forms.GroupBox();
            this.LargestLockedTarget = new System.Windows.Forms.RadioButton();
            this.ClosestLockedTarget = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AutoAttackWhenIdle = new System.Windows.Forms.CheckBox();
            this.AutoAttackWhenTargetLost = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.RecallShieldThreshold = new System.Windows.Forms.NumericUpDown();
            this.RecallIfShieldsBelow = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.AutoAttackWhat.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecallShieldThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.AutoAttackWhat);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(392, 143);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Auto Attack";
            // 
            // AutoAttackWhat
            // 
            this.AutoAttackWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoAttackWhat.Controls.Add(this.LargestLockedTarget);
            this.AutoAttackWhat.Controls.Add(this.ClosestLockedTarget);
            this.AutoAttackWhat.Location = new System.Drawing.Point(8, 82);
            this.AutoAttackWhat.Margin = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhat.Name = "AutoAttackWhat";
            this.AutoAttackWhat.Padding = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhat.Size = new System.Drawing.Size(376, 52);
            this.AutoAttackWhat.TabIndex = 7;
            this.AutoAttackWhat.TabStop = false;
            this.AutoAttackWhat.Text = "What";
            // 
            // LargestLockedTarget
            // 
            this.LargestLockedTarget.AutoSize = true;
            this.LargestLockedTarget.Location = new System.Drawing.Point(8, 23);
            this.LargestLockedTarget.Margin = new System.Windows.Forms.Padding(4);
            this.LargestLockedTarget.Name = "LargestLockedTarget";
            this.LargestLockedTarget.Size = new System.Drawing.Size(175, 20);
            this.LargestLockedTarget.TabIndex = 8;
            this.LargestLockedTarget.Text = "Largest Locked Target";
            this.LargestLockedTarget.UseVisualStyleBackColor = true;
            this.LargestLockedTarget.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // ClosestLockedTarget
            // 
            this.ClosestLockedTarget.AutoSize = true;
            this.ClosestLockedTarget.Checked = true;
            this.ClosestLockedTarget.Location = new System.Drawing.Point(193, 23);
            this.ClosestLockedTarget.Margin = new System.Windows.Forms.Padding(4);
            this.ClosestLockedTarget.Name = "ClosestLockedTarget";
            this.ClosestLockedTarget.Size = new System.Drawing.Size(174, 20);
            this.ClosestLockedTarget.TabIndex = 7;
            this.ClosestLockedTarget.TabStop = true;
            this.ClosestLockedTarget.Text = "Closest Locked Target";
            this.ClosestLockedTarget.UseVisualStyleBackColor = true;
            this.ClosestLockedTarget.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.AutoAttackWhenIdle);
            this.groupBox2.Controls.Add(this.AutoAttackWhenTargetLost);
            this.groupBox2.Location = new System.Drawing.Point(8, 23);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(376, 52);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "When";
            // 
            // AutoAttackWhenIdle
            // 
            this.AutoAttackWhenIdle.AutoSize = true;
            this.AutoAttackWhenIdle.Checked = true;
            this.AutoAttackWhenIdle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoAttackWhenIdle.Location = new System.Drawing.Point(8, 23);
            this.AutoAttackWhenIdle.Margin = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhenIdle.Name = "AutoAttackWhenIdle";
            this.AutoAttackWhenIdle.Size = new System.Drawing.Size(93, 20);
            this.AutoAttackWhenIdle.TabIndex = 3;
            this.AutoAttackWhenIdle.Text = "When Idle";
            this.AutoAttackWhenIdle.UseVisualStyleBackColor = true;
            this.AutoAttackWhenIdle.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // AutoAttackWhenTargetLost
            // 
            this.AutoAttackWhenTargetLost.AutoSize = true;
            this.AutoAttackWhenTargetLost.Checked = true;
            this.AutoAttackWhenTargetLost.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoAttackWhenTargetLost.Location = new System.Drawing.Point(116, 23);
            this.AutoAttackWhenTargetLost.Margin = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhenTargetLost.Name = "AutoAttackWhenTargetLost";
            this.AutoAttackWhenTargetLost.Size = new System.Drawing.Size(146, 20);
            this.AutoAttackWhenTargetLost.TabIndex = 2;
            this.AutoAttackWhenTargetLost.Text = "When Target Lost";
            this.AutoAttackWhenTargetLost.UseVisualStyleBackColor = true;
            this.AutoAttackWhenTargetLost.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.RecallShieldThreshold);
            this.groupBox4.Controls.Add(this.RecallIfShieldsBelow);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(4, 154);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(392, 51);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Auto Recall";
            // 
            // RecallShieldThreshold
            // 
            this.RecallShieldThreshold.Enabled = false;
            this.RecallShieldThreshold.Location = new System.Drawing.Point(136, 22);
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
            this.RecallShieldThreshold.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // RecallIfShieldsBelow
            // 
            this.RecallIfShieldsBelow.AutoSize = true;
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
            this.label1.Location = new System.Drawing.Point(180, 24);
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
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(400, 210);
            this.Name = "DroneHelperConfig";
            this.Size = new System.Drawing.Size(400, 210);
            this.groupBox1.ResumeLayout(false);
            this.AutoAttackWhat.ResumeLayout(false);
            this.AutoAttackWhat.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecallShieldThreshold)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox AutoAttackWhat;
        private System.Windows.Forms.RadioButton LargestLockedTarget;
        private System.Windows.Forms.RadioButton ClosestLockedTarget;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox AutoAttackWhenIdle;
        private System.Windows.Forms.CheckBox AutoAttackWhenTargetLost;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown RecallShieldThreshold;
        private System.Windows.Forms.CheckBox RecallIfShieldsBelow;
        private System.Windows.Forms.Label label1;
    }
}
