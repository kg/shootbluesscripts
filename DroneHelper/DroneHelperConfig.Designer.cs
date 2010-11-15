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
            this.label3 = new System.Windows.Forms.Label();
            this.MaxSigRadius = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.MinSigRadius = new System.Windows.Forms.NumericUpDown();
            this.Smallest = new System.Windows.Forms.RadioButton();
            this.Largest = new System.Windows.Forms.RadioButton();
            this.ClosestToDrones = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AutoAttackWhenIdle = new System.Windows.Forms.CheckBox();
            this.AutoAttackWhenTargetLost = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.RecallShieldThreshold = new System.Windows.Forms.NumericUpDown();
            this.RecallIfShieldsBelow = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AttackAsGroup = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.AutoAttackWhat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSigRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSigRadius)).BeginInit();
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
            this.groupBox1.Size = new System.Drawing.Size(382, 191);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Auto Attack";
            // 
            // AutoAttackWhat
            // 
            this.AutoAttackWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoAttackWhat.Controls.Add(this.AttackAsGroup);
            this.AutoAttackWhat.Controls.Add(this.label3);
            this.AutoAttackWhat.Controls.Add(this.MaxSigRadius);
            this.AutoAttackWhat.Controls.Add(this.label2);
            this.AutoAttackWhat.Controls.Add(this.MinSigRadius);
            this.AutoAttackWhat.Controls.Add(this.Smallest);
            this.AutoAttackWhat.Controls.Add(this.Largest);
            this.AutoAttackWhat.Controls.Add(this.ClosestToDrones);
            this.AutoAttackWhat.Location = new System.Drawing.Point(8, 82);
            this.AutoAttackWhat.Margin = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhat.Name = "AutoAttackWhat";
            this.AutoAttackWhat.Padding = new System.Windows.Forms.Padding(4);
            this.AutoAttackWhat.Size = new System.Drawing.Size(366, 100);
            this.AutoAttackWhat.TabIndex = 1;
            this.AutoAttackWhat.TabStop = false;
            this.AutoAttackWhat.Text = "What";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(256, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "and";
            // 
            // MaxSigRadius
            // 
            this.MaxSigRadius.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.MaxSigRadius.Location = new System.Drawing.Point(288, 68);
            this.MaxSigRadius.Margin = new System.Windows.Forms.Padding(4);
            this.MaxSigRadius.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.MaxSigRadius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MaxSigRadius.Name = "MaxSigRadius";
            this.MaxSigRadius.Size = new System.Drawing.Size(61, 23);
            this.MaxSigRadius.TabIndex = 6;
            this.MaxSigRadius.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.MaxSigRadius.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(186, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Signature Radius Between:";
            // 
            // MinSigRadius
            // 
            this.MinSigRadius.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.MinSigRadius.Location = new System.Drawing.Point(194, 68);
            this.MinSigRadius.Margin = new System.Windows.Forms.Padding(4);
            this.MinSigRadius.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.MinSigRadius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MinSigRadius.Name = "MinSigRadius";
            this.MinSigRadius.Size = new System.Drawing.Size(61, 23);
            this.MinSigRadius.TabIndex = 4;
            this.MinSigRadius.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.MinSigRadius.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // Smallest
            // 
            this.Smallest.AutoSize = true;
            this.Smallest.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Smallest.Location = new System.Drawing.Point(91, 22);
            this.Smallest.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.Smallest.Name = "Smallest";
            this.Smallest.Size = new System.Drawing.Size(81, 20);
            this.Smallest.TabIndex = 1;
            this.Smallest.Text = "Smallest";
            this.Smallest.UseVisualStyleBackColor = true;
            this.Smallest.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // Largest
            // 
            this.Largest.AutoSize = true;
            this.Largest.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Largest.Location = new System.Drawing.Point(8, 22);
            this.Largest.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.Largest.Name = "Largest";
            this.Largest.Size = new System.Drawing.Size(75, 20);
            this.Largest.TabIndex = 0;
            this.Largest.Text = "Largest";
            this.Largest.UseVisualStyleBackColor = true;
            this.Largest.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // ClosestToDrones
            // 
            this.ClosestToDrones.AutoSize = true;
            this.ClosestToDrones.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ClosestToDrones.Checked = true;
            this.ClosestToDrones.Location = new System.Drawing.Point(180, 22);
            this.ClosestToDrones.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.ClosestToDrones.Name = "ClosestToDrones";
            this.ClosestToDrones.Size = new System.Drawing.Size(146, 20);
            this.ClosestToDrones.TabIndex = 2;
            this.ClosestToDrones.TabStop = true;
            this.ClosestToDrones.Text = "Closest To Drones";
            this.ClosestToDrones.UseVisualStyleBackColor = true;
            this.ClosestToDrones.CheckedChanged += new System.EventHandler(this.ValuesChanged);
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
            this.groupBox2.Size = new System.Drawing.Size(366, 52);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "When";
            // 
            // AutoAttackWhenIdle
            // 
            this.AutoAttackWhenIdle.AutoSize = true;
            this.AutoAttackWhenIdle.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AutoAttackWhenIdle.Checked = true;
            this.AutoAttackWhenIdle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoAttackWhenIdle.Location = new System.Drawing.Point(8, 22);
            this.AutoAttackWhenIdle.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.AutoAttackWhenIdle.Name = "AutoAttackWhenIdle";
            this.AutoAttackWhenIdle.Size = new System.Drawing.Size(51, 20);
            this.AutoAttackWhenIdle.TabIndex = 0;
            this.AutoAttackWhenIdle.Text = "Idle";
            this.AutoAttackWhenIdle.UseVisualStyleBackColor = true;
            this.AutoAttackWhenIdle.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // AutoAttackWhenTargetLost
            // 
            this.AutoAttackWhenTargetLost.AutoSize = true;
            this.AutoAttackWhenTargetLost.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AutoAttackWhenTargetLost.Checked = true;
            this.AutoAttackWhenTargetLost.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoAttackWhenTargetLost.Location = new System.Drawing.Point(67, 22);
            this.AutoAttackWhenTargetLost.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.AutoAttackWhenTargetLost.Name = "AutoAttackWhenTargetLost";
            this.AutoAttackWhenTargetLost.Size = new System.Drawing.Size(104, 20);
            this.AutoAttackWhenTargetLost.TabIndex = 1;
            this.AutoAttackWhenTargetLost.Text = "Target Lost";
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
            this.groupBox4.Location = new System.Drawing.Point(4, 203);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox4.Size = new System.Drawing.Size(382, 51);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Auto Recall";
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
            this.RecallShieldThreshold.ValueChanged += new System.EventHandler(this.ValuesChanged);
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
            // AttackAsGroup
            // 
            this.AttackAsGroup.AutoSize = true;
            this.AttackAsGroup.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AttackAsGroup.Checked = true;
            this.AttackAsGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AttackAsGroup.Location = new System.Drawing.Point(8, 46);
            this.AttackAsGroup.Margin = new System.Windows.Forms.Padding(2, 2, 6, 2);
            this.AttackAsGroup.Name = "AttackAsGroup";
            this.AttackAsGroup.Size = new System.Drawing.Size(149, 20);
            this.AttackAsGroup.TabIndex = 7;
            this.AttackAsGroup.Text = "Attack As A Group";
            this.AttackAsGroup.UseVisualStyleBackColor = true;
            this.AttackAsGroup.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // DroneHelperConfig
            // 
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(390, 260);
            this.Name = "DroneHelperConfig";
            this.Size = new System.Drawing.Size(390, 260);
            this.groupBox1.ResumeLayout(false);
            this.AutoAttackWhat.ResumeLayout(false);
            this.AutoAttackWhat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxSigRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinSigRadius)).EndInit();
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
        private System.Windows.Forms.RadioButton Largest;
        private System.Windows.Forms.RadioButton ClosestToDrones;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox AutoAttackWhenIdle;
        private System.Windows.Forms.CheckBox AutoAttackWhenTargetLost;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown RecallShieldThreshold;
        private System.Windows.Forms.CheckBox RecallIfShieldsBelow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton Smallest;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown MaxSigRadius;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown MinSigRadius;
        private System.Windows.Forms.CheckBox AttackAsGroup;
    }
}
