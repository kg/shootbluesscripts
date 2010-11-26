namespace ShootBlues.Script {
    partial class PythonExplorer {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PythonExplorer));
            this.DataGrid = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ProcessList = new System.Windows.Forms.ComboBox();
            this.Breadcrumbs = new System.Windows.Forms.ToolStrip();
            this.Key = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataGrid
            // 
            this.DataGrid.AllowUserToAddRows = false;
            this.DataGrid.AllowUserToDeleteRows = false;
            this.DataGrid.AllowUserToResizeColumns = false;
            this.DataGrid.AllowUserToResizeRows = false;
            this.DataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.DataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.DataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.Value});
            this.DataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DataGrid.GridColor = System.Drawing.SystemColors.ButtonFace;
            this.DataGrid.Location = new System.Drawing.Point(0, 25);
            this.DataGrid.Name = "DataGrid";
            this.DataGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.DataGrid.RowHeadersVisible = false;
            this.DataGrid.RowTemplate.Height = 25;
            this.DataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.DataGrid.ShowEditingIcon = false;
            this.DataGrid.ShowRowErrors = false;
            this.DataGrid.Size = new System.Drawing.Size(565, 420);
            this.DataGrid.TabIndex = 1;
            this.DataGrid.VirtualMode = true;
            this.DataGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGrid_CellValueNeeded);
            this.DataGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGrid_CellContentClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ProcessList);
            this.panel1.Controls.Add(this.Breadcrumbs);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(565, 24);
            this.panel1.TabIndex = 2;
            // 
            // ProcessList
            // 
            this.ProcessList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.ProcessList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProcessList.FormattingEnabled = true;
            this.ProcessList.Location = new System.Drawing.Point(0, 0);
            this.ProcessList.Name = "ProcessList";
            this.ProcessList.Size = new System.Drawing.Size(175, 24);
            this.ProcessList.Sorted = true;
            this.ProcessList.TabIndex = 2;
            this.ProcessList.SelectedValueChanged += new System.EventHandler(this.ProcessList_SelectedValueChanged);
            // 
            // Breadcrumbs
            // 
            this.Breadcrumbs.AllowMerge = false;
            this.Breadcrumbs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Breadcrumbs.AutoSize = false;
            this.Breadcrumbs.CanOverflow = false;
            this.Breadcrumbs.Dock = System.Windows.Forms.DockStyle.None;
            this.Breadcrumbs.GripMargin = new System.Windows.Forms.Padding(0);
            this.Breadcrumbs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.Breadcrumbs.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.Breadcrumbs.Location = new System.Drawing.Point(175, 0);
            this.Breadcrumbs.Name = "Breadcrumbs";
            this.Breadcrumbs.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.Breadcrumbs.ShowItemToolTips = false;
            this.Breadcrumbs.Size = new System.Drawing.Size(390, 24);
            this.Breadcrumbs.TabIndex = 1;
            // 
            // Key
            // 
            this.Key.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Key.DefaultCellStyle = dataGridViewCellStyle1;
            this.Key.FillWeight = 50F;
            this.Key.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Key.HeaderText = "Key";
            this.Key.MinimumWidth = 100;
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            this.Key.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Key.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Key.ToolTipText = "Click to browse into this attribute";
            this.Key.Width = 200;
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.NullValue = null;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Value.DefaultCellStyle = dataGridViewCellStyle2;
            this.Value.FillWeight = 50F;
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PythonExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 445);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.DataGrid);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "PythonExplorer";
            this.Text = "Python Explorer";
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DataGrid;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox ProcessList;
        private System.Windows.Forms.ToolStrip Breadcrumbs;
        private System.Windows.Forms.DataGridViewButtonColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
    }
}