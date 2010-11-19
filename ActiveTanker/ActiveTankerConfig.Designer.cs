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
            this.ShieldRepairThreshold = new System.Windows.Forms.NumericUpDown();
            this.RepairIfShieldsBelow = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ArmorRepairThreshold = new System.Windows.Forms.NumericUpDown();
            this.RepairIfArmorBelow = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.HullRepairThreshold = new System.Windows.Forms.NumericUpDown();
            this.RepairIfHullBelow = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ShieldRepairThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArmorRepairThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HullRepairThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // ShieldRepairThreshold
            // 
            this.ShieldRepairThreshold.Location = new System.Drawing.Point(202, 3);
            this.ShieldRepairThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.ShieldRepairThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ShieldRepairThreshold.Name = "ShieldRepairThreshold";
            this.ShieldRepairThreshold.Size = new System.Drawing.Size(45, 23);
            this.ShieldRepairThreshold.TabIndex = 4;
            this.ShieldRepairThreshold.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.ShieldRepairThreshold.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // RepairIfShieldsBelow
            // 
            this.RepairIfShieldsBelow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RepairIfShieldsBelow.Checked = true;
            this.RepairIfShieldsBelow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RepairIfShieldsBelow.Location = new System.Drawing.Point(4, 4);
            this.RepairIfShieldsBelow.Margin = new System.Windows.Forms.Padding(4);
            this.RepairIfShieldsBelow.Name = "RepairIfShieldsBelow";
            this.RepairIfShieldsBelow.Size = new System.Drawing.Size(197, 20);
            this.RepairIfShieldsBelow.TabIndex = 3;
            this.RepairIfShieldsBelow.Text = "Repair If Shields Below";
            this.RepairIfShieldsBelow.UseVisualStyleBackColor = true;
            this.RepairIfShieldsBelow.CheckedChanged += new System.EventHandler(this.RepairIfShieldsBelow_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(245, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "%";
            // 
            // ArmorRepairThreshold
            // 
            this.ArmorRepairThreshold.Location = new System.Drawing.Point(202, 34);
            this.ArmorRepairThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.ArmorRepairThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ArmorRepairThreshold.Name = "ArmorRepairThreshold";
            this.ArmorRepairThreshold.Size = new System.Drawing.Size(45, 23);
            this.ArmorRepairThreshold.TabIndex = 7;
            this.ArmorRepairThreshold.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            this.ArmorRepairThreshold.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // RepairIfArmorBelow
            // 
            this.RepairIfArmorBelow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RepairIfArmorBelow.Checked = true;
            this.RepairIfArmorBelow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RepairIfArmorBelow.Location = new System.Drawing.Point(4, 35);
            this.RepairIfArmorBelow.Margin = new System.Windows.Forms.Padding(4);
            this.RepairIfArmorBelow.Name = "RepairIfArmorBelow";
            this.RepairIfArmorBelow.Size = new System.Drawing.Size(197, 20);
            this.RepairIfArmorBelow.TabIndex = 6;
            this.RepairIfArmorBelow.Text = "Repair If Armor Below";
            this.RepairIfArmorBelow.UseVisualStyleBackColor = true;
            this.RepairIfArmorBelow.CheckedChanged += new System.EventHandler(this.RepairIfArmorBelow_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(245, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "%";
            // 
            // HullRepairThreshold
            // 
            this.HullRepairThreshold.Location = new System.Drawing.Point(202, 65);
            this.HullRepairThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.HullRepairThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.HullRepairThreshold.Name = "HullRepairThreshold";
            this.HullRepairThreshold.Size = new System.Drawing.Size(45, 23);
            this.HullRepairThreshold.TabIndex = 10;
            this.HullRepairThreshold.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            this.HullRepairThreshold.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // RepairIfHullBelow
            // 
            this.RepairIfHullBelow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RepairIfHullBelow.Checked = true;
            this.RepairIfHullBelow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RepairIfHullBelow.Location = new System.Drawing.Point(4, 66);
            this.RepairIfHullBelow.Margin = new System.Windows.Forms.Padding(4);
            this.RepairIfHullBelow.Name = "RepairIfHullBelow";
            this.RepairIfHullBelow.Size = new System.Drawing.Size(197, 20);
            this.RepairIfHullBelow.TabIndex = 9;
            this.RepairIfHullBelow.Text = "Repair If Structure Below";
            this.RepairIfHullBelow.UseVisualStyleBackColor = true;
            this.RepairIfHullBelow.CheckedChanged += new System.EventHandler(this.RepairIfHullBelow_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(245, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "%";
            // 
            // ActiveTankerConfig
            // 
            this.Controls.Add(this.HullRepairThreshold);
            this.Controls.Add(this.RepairIfHullBelow);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ArmorRepairThreshold);
            this.Controls.Add(this.RepairIfArmorBelow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ShieldRepairThreshold);
            this.Controls.Add(this.RepairIfShieldsBelow);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(270, 95);
            this.Name = "ActiveTankerConfig";
            this.Size = new System.Drawing.Size(270, 95);
            ((System.ComponentModel.ISupportInitialize)(this.ShieldRepairThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArmorRepairThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HullRepairThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown ShieldRepairThreshold;
        private System.Windows.Forms.CheckBox RepairIfShieldsBelow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown ArmorRepairThreshold;
        private System.Windows.Forms.CheckBox RepairIfArmorBelow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown HullRepairThreshold;
        private System.Windows.Forms.CheckBox RepairIfHullBelow;
        private System.Windows.Forms.Label label3;

    }
}
