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
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPriorityBoost)).BeginInit();
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
            // BroadcastHelperConfig
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(250, 50);
            this.Name = "BroadcastHelperConfig";
            this.Size = new System.Drawing.Size(250, 50);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPriorityBoost)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown TargetPriorityBoost;
    }
}
