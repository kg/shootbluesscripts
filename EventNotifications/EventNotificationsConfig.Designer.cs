namespace ShootBlues.Script {
    partial class EventNotificationsConfig {
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
            this.DataGrid = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sound = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BalloonTip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.MessageBox = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.JabberEndpoints = new System.Windows.Forms.DataGridViewComboBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // DataGrid
            // 
            this.DataGrid.AllowUserToAddRows = false;
            this.DataGrid.AllowUserToDeleteRows = false;
            this.DataGrid.AllowUserToResizeColumns = false;
            this.DataGrid.AllowUserToResizeRows = false;
            this.DataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.DataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.DataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.DataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.Sound,
            this.BalloonTip,
            this.MessageBox,
            this.JabberEndpoints});
            this.DataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGrid.Location = new System.Drawing.Point(0, 0);
            this.DataGrid.MultiSelect = false;
            this.DataGrid.Name = "DataGrid";
            this.DataGrid.RowHeadersVisible = false;
            this.DataGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.DataGrid.Size = new System.Drawing.Size(400, 200);
            this.DataGrid.TabIndex = 0;
            this.DataGrid.VirtualMode = true;
            this.DataGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGrid_CellValueNeeded);
            this.DataGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGrid_CellValuePushed);
            // 
            // Key
            // 
            this.Key.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Key.HeaderText = "Name";
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            this.Key.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Sound
            // 
            this.Sound.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Sound.HeaderText = "Sound";
            this.Sound.Name = "Sound";
            this.Sound.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Sound.Width = 74;
            // 
            // BalloonTip
            // 
            this.BalloonTip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.BalloonTip.HeaderText = "Balloon";
            this.BalloonTip.Name = "BalloonTip";
            this.BalloonTip.Width = 60;
            // 
            // MessageBox
            // 
            this.MessageBox.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.MessageBox.HeaderText = "Dialog";
            this.MessageBox.Name = "MessageBox";
            this.MessageBox.Width = 53;
            // 
            // JabberEndpoints
            // 
            this.JabberEndpoints.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.JabberEndpoints.HeaderText = "Jabber";
            this.JabberEndpoints.Name = "JabberEndpoints";
            this.JabberEndpoints.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.JabberEndpoints.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.JabberEndpoints.Width = 76;
            // 
            // EventNotificationsConfig
            // 
            this.Controls.Add(this.DataGrid);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "EventNotificationsConfig";
            this.Size = new System.Drawing.Size(400, 200);
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sound;
        private System.Windows.Forms.DataGridViewCheckBoxColumn BalloonTip;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MessageBox;
        private System.Windows.Forms.DataGridViewComboBoxColumn JabberEndpoints;

    }
}
