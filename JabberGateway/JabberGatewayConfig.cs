using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using Squared.Task.Data.Mapper;

namespace ShootBlues.Script {
    public partial class JabberGatewayConfig : TaskUserControl, IConfigurationPanel {
        JabberGateway Script;

        public JabberGatewayConfig (JabberGateway script)
            : base(Program.Scheduler) {
            InitializeComponent();
            Script = script;
        }        

        public IEnumerator<object> LoadConfiguration () {
            using (new ControlWaitCursor(this)) {
                List.BeginUpdate();
                List.Sorted = false;
                List.Items.Clear();

                string[] names = null;
                yield return Program.Database.ExecutePrimitiveArray<string>(
                    "SELECT name FROM jabber.endpoints ORDER BY name ASC"
                ).Bind(() => names);

                List.Items.AddRange(names);
                List.Sorted = true;
                List.EndUpdate();

                Edit.Enabled = TestEndpoint.Enabled = Remove.Enabled = (List.SelectedIndices.Count > 0);
            }
        }

        public IEnumerator<object> SaveConfiguration () {
            yield break;
        }

        private void AddNew_Click (object sender, EventArgs e) {
            using (var dialog = new AddEndpointDialog(null)) {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                Start(SaveEndpoint(dialog.Settings));
            }
        }

        private IEnumerator<object> SaveEndpoint (EndpointSettings settings) {
            using (new ControlDisabler(this)) {
                var columnNames = Mapper<EndpointSettings>.ColumnNames;
                var questionMarks = new string[columnNames.Length];
                for (int i = 0; i < questionMarks.Length; i++)
                    questionMarks[i] = "?";

                using (var q = Program.Database.BuildQuery(String.Format(
                    "REPLACE INTO jabber.endpoints ({0}) VALUES ({1})", String.Join(", ", columnNames), String.Join(", ", questionMarks)
                )))
                    yield return q.ExecuteNonQuery(Mapper<EndpointSettings>.GetColumnValues(settings));

                yield return LoadConfiguration();

                yield return Script.InitGateways();
            }
        }

        private IEnumerator<object> RemoveEndpoint (string endpointName) {
            using (new ControlDisabler(this)) {
                yield return Program.Database.ExecuteSQL("DELETE FROM jabber.endpoints WHERE name = ?", endpointName);

                yield return LoadConfiguration();

                yield return Script.InitGateways();
            }
        }

        private IEnumerator<object> EditEndpoint (string endpointName) {
            EndpointSettings[] endpoints = null;

            using (var q = Program.Database.BuildQuery("SELECT * FROM jabber.endpoints WHERE name = ?"))
                yield return q.ExecuteArray<EndpointSettings>(endpointName).Bind(() => endpoints);

            if ((endpoints == null) || (endpoints.Length == 0))
                yield break;

            using (var dialog = new AddEndpointDialog(endpoints[0])) {
                if (dialog.ShowDialog() != DialogResult.OK)
                    yield break;

                yield return SaveEndpoint(dialog.Settings);
            }
        }

        private void Remove_Click (object sender, EventArgs e) {
            Start(RemoveEndpoint(List.SelectedItem as string));
        }

        private void List_SelectedIndexChanged (object sender, EventArgs e) {
            Edit.Enabled = TestEndpoint.Enabled = Remove.Enabled = (List.SelectedIndices.Count > 0);
        }

        private void List_DoubleClick (object sender, EventArgs e) {
            var name = List.SelectedItem as string;
            Start(EditEndpoint(name));
        }

        private void TestEndpoint_Click (object sender, EventArgs e) {
            var name = List.SelectedItem as string;
            Script.GetQueue(name).Enqueue("Testing");
        }

        private void Edit_Click (object sender, EventArgs e) {
            var name = List.SelectedItem as string;
            Start(EditEndpoint(name));
        }
    }
}
