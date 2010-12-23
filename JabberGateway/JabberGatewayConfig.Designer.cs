namespace ShootBlues.Script {
    partial class JabberGatewayConfig {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JabberGatewayConfig));
            this.Remove = new System.Windows.Forms.Button();
            this.AddNew = new System.Windows.Forms.Button();
            this.List = new System.Windows.Forms.ListBox();
            this.TestEndpoint = new System.Windows.Forms.Button();
            this.Edit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Remove.Enabled = false;
            this.Remove.Image = ((System.Drawing.Image)(resources.GetObject("Remove.Image")));
            this.Remove.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Remove.Location = new System.Drawing.Point(170, 170);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(90, 27);
            this.Remove.TabIndex = 6;
            this.Remove.Text = "&Remove";
            this.Remove.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // AddNew
            // 
            this.AddNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddNew.Image = ((System.Drawing.Image)(resources.GetObject("AddNew.Image")));
            this.AddNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddNew.Location = new System.Drawing.Point(3, 170);
            this.AddNew.Name = "AddNew";
            this.AddNew.Size = new System.Drawing.Size(90, 27);
            this.AddNew.TabIndex = 5;
            this.AddNew.Text = "&Add...";
            this.AddNew.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AddNew.UseVisualStyleBackColor = true;
            this.AddNew.Click += new System.EventHandler(this.AddNew_Click);
            // 
            // List
            // 
            this.List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.List.IntegralHeight = false;
            this.List.ItemHeight = 16;
            this.List.Location = new System.Drawing.Point(3, 3);
            this.List.Name = "List";
            this.List.Size = new System.Drawing.Size(384, 161);
            this.List.TabIndex = 4;
            this.List.SelectedIndexChanged += new System.EventHandler(this.List_SelectedIndexChanged);
            this.List.DoubleClick += new System.EventHandler(this.List_DoubleClick);
            // 
            // TestEndpoint
            // 
            this.TestEndpoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TestEndpoint.Enabled = false;
            this.TestEndpoint.Image = ((System.Drawing.Image)(resources.GetObject("TestEndpoint.Image")));
            this.TestEndpoint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestEndpoint.Location = new System.Drawing.Point(322, 170);
            this.TestEndpoint.Name = "TestEndpoint";
            this.TestEndpoint.Size = new System.Drawing.Size(65, 27);
            this.TestEndpoint.TabIndex = 7;
            this.TestEndpoint.Text = "Test";
            this.TestEndpoint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TestEndpoint.UseVisualStyleBackColor = true;
            this.TestEndpoint.Click += new System.EventHandler(this.TestEndpoint_Click);
            // 
            // Edit
            // 
            this.Edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Edit.Enabled = false;
            this.Edit.Image = ((System.Drawing.Image)(resources.GetObject("Edit.Image")));
            this.Edit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Edit.Location = new System.Drawing.Point(99, 170);
            this.Edit.Name = "Edit";
            this.Edit.Size = new System.Drawing.Size(65, 27);
            this.Edit.TabIndex = 8;
            this.Edit.Text = "&Edit";
            this.Edit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Edit.UseVisualStyleBackColor = true;
            this.Edit.Click += new System.EventHandler(this.Edit_Click);
            // 
            // JabberGatewayConfig
            // 
            this.Controls.Add(this.Edit);
            this.Controls.Add(this.TestEndpoint);
            this.Controls.Add(this.Remove);
            this.Controls.Add(this.AddNew);
            this.Controls.Add(this.List);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "JabberGatewayConfig";
            this.Size = new System.Drawing.Size(390, 200);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button AddNew;
        private System.Windows.Forms.ListBox List;
        private System.Windows.Forms.Button TestEndpoint;
        private System.Windows.Forms.Button Edit;


    }
}
