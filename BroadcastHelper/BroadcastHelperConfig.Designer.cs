namespace ShootBlues.Script {
    partial class BroadcastHelperConfig {
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TargetPriorityBoost = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RepPriorityBoost = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPriorityBoost)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RepPriorityBoost)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.TargetPriorityBoost);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(250, 47);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Target Broadcasts";
            // 
            // TargetPriorityBoost
            // 
            this.TargetPriorityBoost.Location = new System.Drawing.Point(126, 17);
            this.TargetPriorityBoost.Margin = new System.Windows.Forms.Padding(4);
            this.TargetPriorityBoost.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.TargetPriorityBoost.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            -2147483648});
            this.TargetPriorityBoost.Name = "TargetPriorityBoost";
            this.TargetPriorityBoost.Size = new System.Drawing.Size(37, 23);
            this.TargetPriorityBoost.TabIndex = 13;
            this.TargetPriorityBoost.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TargetPriorityBoost.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 16);
            this.label1.TabIndex = 12;
            this.label1.Text = "Boost Priority By";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.RepPriorityBoost);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(0, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 47);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repair Broadcasts";
            // 
            // RepPriorityBoost
            // 
            this.RepPriorityBoost.Location = new System.Drawing.Point(126, 17);
            this.RepPriorityBoost.Margin = new System.Windows.Forms.Padding(4);
            this.RepPriorityBoost.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.RepPriorityBoost.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            -2147483648});
            this.RepPriorityBoost.Name = "RepPriorityBoost";
            this.RepPriorityBoost.Size = new System.Drawing.Size(37, 23);
            this.RepPriorityBoost.TabIndex = 13;
            this.RepPriorityBoost.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RepPriorityBoost.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Boost Priority By";
            // 
            // BroadcastHelperConfig
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(250, 105);
            this.Name = "BroadcastHelperConfig";
            this.Size = new System.Drawing.Size(250, 105);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPriorityBoost)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RepPriorityBoost)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown TargetPriorityBoost;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown RepPriorityBoost;
        private System.Windows.Forms.Label label2;
    }
}
