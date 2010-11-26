using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using System.Web.Script.Serialization;

namespace ShootBlues.Script {
    public partial class PythonExplorer : TaskForm {
        public Common Script;

        public string[] Context;
        public int ActiveContext;

        protected string[] Keys;
        protected string[] Values;

        IFuture _CurrentRefresh = null;

        public PythonExplorer (TaskScheduler scheduler, Common script) 
            : base (scheduler) {
            InitializeComponent();
            Script = script;

            SubscribeTo<ProcessInfo>(Program.EventBus, Program.Profile, "RunningProcessAdded", (e, pi) => {
                ProcessList.Items.Add(pi);
            });
            SubscribeTo<ProcessInfo>(Program.EventBus, Program.Profile, "RunningProcessRemoved", (e, pi) => {
                ProcessList.Items.Remove(pi);
            });

            Context = new string[] { "__main__" };
            ActiveContext = 0;
            RefreshBreadcrumbs();

            ProcessList.BeginUpdate();
            foreach (var pi in Program.RunningProcesses)
                ProcessList.Items.Add(pi);
            ProcessList.SelectedIndex = 0;
            ProcessList.EndUpdate();
        }

        public void ReplaceContext (params string[] context) {
            Context = context;
            ActiveContext = context.Length - 1;
            RefreshBreadcrumbs();
            RefreshValues();
        }

        public void PushContext (string attributeName) {
            string[] newContext = new string[ActiveContext + 2];
            Array.Copy(Context, 0, newContext, 0, ActiveContext + 1);
            newContext[ActiveContext + 1] = attributeName;
            ReplaceContext(newContext);
        }

        public void RefreshBreadcrumbs () {
            using (new ControlWaitCursor(this)) {
                Breadcrumbs.Items.Clear();

                for (int i = 0; i < Context.Length; i++) {
                    var button = new ToolStripButton();
                    {
                        int _i = i;
                        button.Click += (s, e) => BreadcrumbClick(_i);
                    }

                    button.Text = Context[i];
                    button.Checked = (i == ActiveContext);
                    button.DisplayStyle = ToolStripItemDisplayStyle.Text;

                    Breadcrumbs.Items.Add(button);

                    if (i == 0) {
                        var ellipsis = new ToolStripButton();
                        ellipsis.Text = "…";
                        ellipsis.ToolTipText = "Select new module";
                        ellipsis.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        ellipsis.Click += (s, e) =>
                            Start(SelectModuleTask(ProcessList.SelectedItem as ProcessInfo, Context[0]));
                        Breadcrumbs.Items.Add(ellipsis);
                    }
                }
            }
        }

        public IEnumerator<object> SelectModuleTask (ProcessInfo pi, string oldText) {
            var fModules = Program.CallFunction<string[]>(pi, "pythonexplorer", "getModules");
            yield return fModules;

            using (var dlg = new SelectModuleDialog()) {
                dlg.Modules.Items.AddRange(fModules.Result);
                dlg.Modules.Text = oldText;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    ReplaceContext(dlg.Modules.Text);
            }
        }

        protected void BreadcrumbClick (int index) {
            ActiveContext = index;
            RefreshBreadcrumbs();
            RefreshValues();
        }

        public void RefreshValues () {
            if (_CurrentRefresh != null)
                _CurrentRefresh.Dispose();

            var pi = ProcessList.SelectedItem as ProcessInfo;
            if (pi != null)
                _CurrentRefresh = Start(RefreshValuesTask(pi, Context, ActiveContext));
            else
                _CurrentRefresh = null;
        }

        protected IEnumerator<object> RefreshValuesTask (ProcessInfo process, string[] context, int activeContext) {
            int windowSize = 128;

            using (new ControlDisabler(this)) {
                DataGrid.RowCount = 0;
                DataGrid.Invalidate();

                var serializer = new JavaScriptSerializer();

                string[] keys = null;
                yield return Program.CallFunction<string[]>(
                    process, "pythonexplorer", "getKeys", context, activeContext
                ).Bind(() => keys);
                Values = new string[keys.Length];
                Keys = keys;

                DataGrid.RowCount = keys.Length;
                DataGrid.InvalidateColumn(0);

                string[] values = null;
                string[] windowKeys = new string[windowSize];

                for (int windowPos = 0; windowPos < keys.Length; windowPos += windowSize) {
                    if (windowPos + windowSize >= keys.Length) {
                        windowSize = keys.Length - windowPos;
                        windowKeys = new string[windowSize];
                    }

                    Array.Copy(
                        keys, windowPos, windowKeys, 0, 
                        windowSize
                    );

                    yield return Program.CallFunction<string[]>(
                        process, "pythonexplorer", "getValues", context, activeContext, windowKeys
                    ).Bind(() => values);

                    Array.Copy(values, 0, Values, windowPos, values.Length);

                    DataGrid.InvalidateColumn(1);
                }
            }
        }

        private void ProcessList_SelectedValueChanged (object sender, EventArgs e) {
            RefreshValues();
        }

        private void DataGrid_CellContentClick (object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 0) {
                string attributeName = DataGrid.Rows[e.RowIndex].Cells[0].Value.ToString();
                PushContext(attributeName);
            }
        }

        private void DataGrid_CellValueNeeded (object sender, DataGridViewCellValueEventArgs e) {
            if ((e.RowIndex < 0) || (e.RowIndex >= Values.Length))
                return;

            if (e.ColumnIndex == 0)
                e.Value = Keys[e.RowIndex];
            else if (e.ColumnIndex == 1)
                e.Value = Values[e.RowIndex];
        }

        private void DataGrid_CellDoubleClick (object sender, DataGridViewCellEventArgs e) {
            if ((e.RowIndex < 0) || (e.RowIndex >= Values.Length))
                return;

            if (e.ColumnIndex == 1) {
            }
        }
    }
}
