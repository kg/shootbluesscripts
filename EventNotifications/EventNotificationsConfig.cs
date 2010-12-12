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

        public EventNotificationsConfig (EventNotifications script)
            : base(Program.Scheduler) {
            InitializeComponent();
            Script = script;
        }

        public IEnumerator<object> LoadConfiguration () {
            using (new ControlWaitCursor(this)) {
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
            }

            Start(FlushRow(row));
        }

        protected IEnumerator<object> FlushRow (EventEntry row) {
            using (var q = Program.Database.BuildQuery("REPLACE INTO eventNotifications (key, sound, balloonTip, messageBox) VALUES (?, ?, ?, ?)"))
                yield return q.ExecuteNonQuery(row.Key, row.Sound, row.BalloonTip, row.MessageBox);

            Script.Preferences.Flush();
        }
    }
}
