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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Normal Priority", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Never Attack", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnemyPrioritizerConfig));
            this.List = new System.Windows.Forms.ListView();
            this.NameColumn = new System.Windows.Forms.ColumnHeader();
            this.Icons = new System.Windows.Forms.ImageList(this.components);
            this.Toolbar = new System.Windows.Forms.ToolStrip();
            this.AddType = new System.Windows.Forms.ToolStripButton();
            this.AddGroup = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.PriorityUp = new System.Windows.Forms.ToolStripButton();
            this.PriorityDown = new System.Windows.Forms.ToolStripButton();
            this.Remove = new System.Windows.Forms.ToolStripButton();
            this.Toolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // List
            // 
            this.List.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.List.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn});
            this.List.FullRowSelect = true;
            listViewGroup1.Header = "Normal Priority";
            listViewGroup1.Name = "Normal Priority";
            listViewGroup2.Header = "Never Attack";
            listViewGroup2.Name = "Never Attack";
            this.List.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.List.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.List.HideSelection = false;
            this.List.LabelWrap = false;
            this.List.LargeImageList = this.Icons;
            this.List.Location = new System.Drawing.Point(3, 3);
            this.List.Name = "List";
            this.List.Size = new System.Drawing.Size(454, 244);
            this.List.SmallImageList = this.Icons;
            this.List.StateImageList = this.Icons;
            this.List.TabIndex = 0;
            this.List.UseCompatibleStateImageBehavior = false;
            this.List.View = System.Windows.Forms.View.Details;
            this.List.SelectedIndexChanged += new System.EventHandler(this.List_SelectedIndexChanged);
            this.List.SizeChanged += new System.EventHandler(this.List_SizeChanged);
            // 
            // NameColumn
            // 
            this.NameColumn.Text = "Name";
            // 
            // Icons
            // 
            this.Icons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Icons.ImageStream")));
            this.Icons.TransparentColor = System.Drawing.Color.Transparent;
            this.Icons.Images.SetKeyName(0, "group");
            this.Icons.Images.SetKeyName(1, "type");
            // 
            // Toolbar
            // 
            this.Toolbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Toolbar.AutoSize = false;
            this.Toolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.Toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.Toolbar.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddType,
            this.AddGroup,
            this.toolStripSeparator1,
            this.PriorityUp,
            this.PriorityDown,
            this.Remove});
            this.Toolbar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.Toolbar.Location = new System.Drawing.Point(460, 3);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.Size = new System.Drawing.Size(37, 244);
            this.Toolbar.TabIndex = 1;
            // 
            // AddType
            // 
            this.AddType.AutoSize = false;
            this.AddType.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddType.Image = ((System.Drawing.Image)(resources.GetObject("AddType.Image")));
            this.AddType.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.AddType.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddType.Margin = new System.Windows.Forms.Padding(0);
            this.AddType.Name = "AddType";
            this.AddType.Size = new System.Drawing.Size(36, 36);
            this.AddType.Text = "Add Type";
            this.AddType.Click += new System.EventHandler(this.AddType_Click);
            // 
            // AddGroup
            // 
            this.AddGroup.AutoSize = false;
            this.AddGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddGroup.Image = ((System.Drawing.Image)(resources.GetObject("AddGroup.Image")));
            this.AddGroup.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.AddGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddGroup.Margin = new System.Windows.Forms.Padding(0);
            this.AddGroup.Name = "AddGroup";
            this.AddGroup.Size = new System.Drawing.Size(36, 36);
            this.AddGroup.Text = "Add Group";
            this.AddGroup.Click += new System.EventHandler(this.AddGroup_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(35, 6);
            // 
            // PriorityUp
            // 
            this.PriorityUp.AutoSize = false;
            this.PriorityUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PriorityUp.Enabled = false;
            this.PriorityUp.Image = ((System.Drawing.Image)(resources.GetObject("PriorityUp.Image")));
            this.PriorityUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.PriorityUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PriorityUp.Margin = new System.Windows.Forms.Padding(0);
            this.PriorityUp.Name = "PriorityUp";
            this.PriorityUp.Size = new System.Drawing.Size(36, 36);
            this.PriorityUp.Text = "Priority Up";
            this.PriorityUp.Click += new System.EventHandler(this.PriorityUp_Click);
            // 
            // PriorityDown
            // 
            this.PriorityDown.AutoSize = false;
            this.PriorityDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PriorityDown.Enabled = false;
            this.PriorityDown.Image = ((System.Drawing.Image)(resources.GetObject("PriorityDown.Image")));
            this.PriorityDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.PriorityDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PriorityDown.Margin = new System.Windows.Forms.Padding(0);
            this.PriorityDown.Name = "PriorityDown";
            this.PriorityDown.Size = new System.Drawing.Size(36, 36);
            this.PriorityDown.Text = "Priority Down";
            this.PriorityDown.Click += new System.EventHandler(this.PriorityDown_Click);
            // 
            // Remove
            // 
            this.Remove.AutoSize = false;
            this.Remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Remove.Enabled = false;
            this.Remove.Image = ((System.Drawing.Image)(resources.GetObject("Remove.Image")));
            this.Remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Remove.Margin = new System.Windows.Forms.Padding(0);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(36, 36);
            this.Remove.Text = "Remove";
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // EnemyPrioritizerConfig
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.Toolbar);
            this.Controls.Add(this.List);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Name = "EnemyPrioritizerConfig";
            this.Size = new System.Drawing.Size(500, 250);
            this.Toolbar.ResumeLayout(false);
            this.Toolbar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView List;
        private System.Windows.Forms.ToolStrip Toolbar;
        private System.Windows.Forms.ToolStripButton AddType;
        private System.Windows.Forms.ToolStripButton AddGroup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton PriorityUp;
        private System.Windows.Forms.ToolStripButton PriorityDown;
        private System.Windows.Forms.ImageList Icons;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ToolStripButton Remove;
    }
}
