namespace ShootBlues.Script {
    partial class AutoTargeterConfig {
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
            this.TargetNeutralPlayers = new System.Windows.Forms.CheckBox();
            this.TargetHostileNPCs = new System.Windows.Forms.CheckBox();
            this.TargetHostilePlayers = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ReservedTargetSlots = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReservedTargetSlots)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.TargetNeutralPlayers);
            this.groupBox1.Controls.Add(this.TargetHostileNPCs);
            this.groupBox1.Controls.Add(this.TargetHostilePlayers);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(294, 88);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "What";
            // 
            // TargetNeutralPlayers
            // 
            this.TargetNeutralPlayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetNeutralPlayers.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TargetNeutralPlayers.Location = new System.Drawing.Point(5, 20);
            this.TargetNeutralPlayers.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.TargetNeutralPlayers.Name = "TargetNeutralPlayers";
            this.TargetNeutralPlayers.Size = new System.Drawing.Size(284, 20);
            this.TargetNeutralPlayers.TabIndex = 9;
            this.TargetNeutralPlayers.Text = "Target Neutral Players";
            this.TargetNeutralPlayers.UseVisualStyleBackColor = true;
            this.TargetNeutralPlayers.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // TargetHostileNPCs
            // 
            this.TargetHostileNPCs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetHostileNPCs.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TargetHostileNPCs.Checked = true;
            this.TargetHostileNPCs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TargetHostileNPCs.Location = new System.Drawing.Point(5, 64);
            this.TargetHostileNPCs.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.TargetHostileNPCs.Name = "TargetHostileNPCs";
            this.TargetHostileNPCs.Size = new System.Drawing.Size(284, 20);
            this.TargetHostileNPCs.TabIndex = 11;
            this.TargetHostileNPCs.Text = "Target Hostile NPCs";
            this.TargetHostileNPCs.UseVisualStyleBackColor = true;
            this.TargetHostileNPCs.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // TargetHostilePlayers
            // 
            this.TargetHostilePlayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetHostilePlayers.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TargetHostilePlayers.Location = new System.Drawing.Point(5, 42);
            this.TargetHostilePlayers.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.TargetHostilePlayers.Name = "TargetHostilePlayers";
            this.TargetHostilePlayers.Size = new System.Drawing.Size(284, 20);
            this.TargetHostilePlayers.TabIndex = 10;
            this.TargetHostilePlayers.Text = "Target Hostile Players";
            this.TargetHostilePlayers.UseVisualStyleBackColor = true;
            this.TargetHostilePlayers.CheckedChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.ReservedTargetSlots);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(3, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(294, 47);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rules";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 16);
            this.label2.TabIndex = 14;
            this.label2.Text = "Target Slot(s) Free";
            // 
            // ReservedTargetSlots
            // 
            this.ReservedTargetSlots.Location = new System.Drawing.Point(109, 17);
            this.ReservedTargetSlots.Margin = new System.Windows.Forms.Padding(4);
            this.ReservedTargetSlots.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.ReservedTargetSlots.Name = "ReservedTargetSlots";
            this.ReservedTargetSlots.Size = new System.Drawing.Size(37, 23);
            this.ReservedTargetSlots.TabIndex = 13;
            this.ReservedTargetSlots.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ReservedTargetSlots.ValueChanged += new System.EventHandler(this.ValuesChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 16);
            this.label1.TabIndex = 12;
            this.label1.Text = "Keep At Least";
            // 
            // AutoTargeterConfig
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.MinimumSize = new System.Drawing.Size(300, 150);
            this.Name = "AutoTargeterConfig";
            this.Size = new System.Drawing.Size(300, 150);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReservedTargetSlots)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox TargetNeutralPlayers;
        private System.Windows.Forms.CheckBox TargetHostileNPCs;
        private System.Windows.Forms.CheckBox TargetHostilePlayers;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ReservedTargetSlots;
    }
}
