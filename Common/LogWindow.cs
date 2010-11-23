using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class LogWindow : TaskForm {
        public LogWindow (TaskScheduler scheduler) 
            : base(scheduler) {
            InitializeComponent();

            GC.Collect();
        }

        public void SetText (string text) {
            LogText.Text = text;
            LogText.SelectionLength = 0;
            LogText.SelectionStart = text.Length;
        }

        public void AddLine (string line) {
            LogText.SelectionLength = 0;
            LogText.SelectionStart = LogText.Text.Length;
            if (LogText.Text.Length > 0)
                LogText.SelectedText = Environment.NewLine + line;
            else
                LogText.SelectedText = line;
        }

        public void Clear () {
            LogText.Text = "";
        }
    }
}
