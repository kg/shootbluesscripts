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
        }

        public void SetText (string text) {
            LogText.Text = text;
            LogText.SelectionStart = text.Length;
            LogText.SelectionLength = 0;
        }

        public void AddLine (string line) {
            LogText.SelectionStart = LogText.Text.Length;
            LogText.SelectionLength = 0;
            if (LogText.Text.Length > 0)
                LogText.SelectedText = Environment.NewLine + line;
            else
                LogText.SelectedText = line;
        }
    }
}
