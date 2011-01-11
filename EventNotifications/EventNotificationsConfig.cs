using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class EventNotificationsConfig : TaskUserControl, IConfigurationPanel {
        EventNotifications Script;
        EventEntry[] EventData;

        string[] EndpointNames;

        public EventNotificationsConfig (EventNotifications script)
            : base(Program.Scheduler) {
            InitializeComponent();
            Script = script;
        }

        public IEnumerator<object> LoadConfiguration () {
            using (new ControlWaitCursor(this)) {
                DataGrid.RowCount = 0;
                Endpoints.Items.Clear();

                EventEntry[] dbEvents = null;
                yield return Program.Database.ExecuteArray<EventEntry>(
                    "SELECT * FROM eventNotifications ORDER BY key ASC"
                ).Bind(() => dbEvents);

                var dict = new Dictionary<string, EventEntry>();
                foreach (var dbe in dbEvents)
                    dict[dbe.Key] = dbe;

                foreach (var key in Script.DefinedEvents)
                    if (!dict.ContainsKey(key))
                        dict[key] = new EventEntry(key);

                var newEvents = dict.Values.ToArray();
                Array.Sort(newEvents, (lhs, rhs) => lhs.Key.CompareTo(rhs.Key));

                EndpointNames = Script.GetEndpointNames();
                if (EndpointNames.Length > 0) {
                    Endpoints.Items.Add(DBNull.Value);
                    Endpoints.Items.AddRange(EndpointNames);
                    Endpoints.Visible = true;
                } else {
                    Endpoints.Visible = false;
                }

                EventData = newEvents;
                DataGrid.RowCount = newEvents.Length;
            }
        }

        public IEnumerator<object> SaveConfiguration () {
            yield break;
        }

        private void DataGrid_CellValueNeeded (object sender, DataGridViewCellValueEventArgs e) {
            if ((e.RowIndex < 0) || (e.RowIndex >= EventData.Length))
                return;

            var row = EventData[e.RowIndex];
            switch (e.ColumnIndex) {
                case 0:
                    e.Value = row.Key;
                    break;
                case 1:
                    e.Value = row.Sound;
                    break;
                case 2:
                    e.Value = row.BalloonTip;
                    break;
                case 3:
                    e.Value = row.MessageBox;
                    break;
                case 4:
                    if (EndpointNames.Contains(row.Endpoint))
                        e.Value = row.Endpoint;
                    else
                        e.Value = DBNull.Value;
                    break;
            }
        }

        private void DataGrid_CellValuePushed (object sender, DataGridViewCellValueEventArgs e) {
            if ((e.RowIndex < 0) || (e.RowIndex >= EventData.Length))
                return;

            var row = EventData[e.RowIndex];
            switch (e.ColumnIndex) {
                case 1:
                    row.Sound = (string)e.Value;
                    break;
                case 2:
                    row.BalloonTip = (bool)e.Value;
                    break;
                case 3:
                    row.MessageBox = (bool)e.Value;
                    break;
                case 4:
                    row.Endpoint = (string)e.Value;
                    break;
            }

            Start(FlushRow(row));
        }

        protected IEnumerator<object> FlushRow (EventEntry row) {
            using (var q = Program.Database.BuildQuery("REPLACE INTO eventNotifications (key, sound, balloonTip, messageBox, endpoint) VALUES (?, ?, ?, ?, ?)"))
                yield return q.ExecuteNonQuery(row.Key, row.Sound, row.BalloonTip, row.MessageBox, row.Endpoint);

            Script.Preferences.Flush();
        }

        private void DataGrid_CellContentClick (object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex != 0)
                return;
            
            if ((e.RowIndex < 0) || (e.RowIndex >= EventData.Length))
                return;

            if (Program.RunningProcesses.Count == 0) {
                System.Windows.Forms.MessageBox.Show(this, "Cannot test; no processes running.", "Error");
                return;
            }

            var process = Program.RunningProcesses.First();

            var dict = new Dictionary<string, object>();
            dict["title"] = "Testing";
            dict["text"] = "Test Notification";

            Program.CallFunction(process, "eventnotifications", "handleEvent", process.Process.Id, EventData[e.RowIndex].Key, dict);
        }
    }
}
