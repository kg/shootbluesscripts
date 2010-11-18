namespace ShootBlues.Script {
    partial class EnemyPrioritizerConfig {
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.AddType = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(473, 212);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // AddType
            // 
            this.AddType.Location = new System.Drawing.Point(3, 221);
            this.AddType.Name = "AddType";
            this.AddType.Size = new System.Drawing.Size(100, 25);
            this.AddType.TabIndex = 1;
            this.AddType.Text = "Add Type";
            this.AddType.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(109, 221);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 25);
            this.button1.TabIndex = 2;
            this.button1.Text = "Add Name";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(215, 221);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 25);
            this.button2.TabIndex = 3;
            this.button2.Text = "Remove";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // EnemyPrioritizerConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.AddType);
            this.Controls.Add(this.listView1);
            this.Name = "EnemyPrioritizerConfig";
            this.Size = new System.Drawing.Size(479, 249);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button AddType;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
