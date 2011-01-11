using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.IO;
using Squared.Task.Data.Mapper;
using System.Web.Script.Serialization;
using System.Drawing;
using Squared.Util.Event;

namespace ShootBlues.Script {
    [Mapper]
    public class EventEntry {
        [Column("key")]
        public string Key = null;
        [Column("sound")]
        public string Sound = null;
        [Column("balloonTip")]
        public bool BalloonTip = false;
        [Column("messageBox")]
        public bool MessageBox = false;
        [Column("endpoint")]
        public string Endpoint = null;

        public EventEntry () {
        }

        public EventEntry (string key) {
            Key = key;
        }
    }

    public class EventNotifications : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public HashSet<string> DefinedEvents = new HashSet<string>();

        public EventNotifications (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("eventnotifications.py");

            CustomMenu = new ToolStripMenuItem("Event Notifications");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureEventNotifications);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureEventNotifications (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Event Notifications"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> Initialize () {
            yield return Program.CreateDBTable(
                "eventNotifications",
                "( key TEXT PRIMARY KEY NOT NULL, sound TEXT, balloonTip BOOLEAN NOT NULL, messageBox BOOLEAN NOT NULL, endpoint TEXT )"
            );
        }

        public void DefineEvent (ProcessInfo process, string key) {
            DefineEvent(key);
        }

        public void DefineEvent (string key) {
            DefinedEvents.Add(key);
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            var endpointNames = new HashSet<string>(GetEndpointNames());

            var cfgDict = new Dictionary<string, Dictionary<string, object>>();

            EventEntry[] ee = null;
            using (var q = Program.Database.BuildQuery("SELECT * FROM eventNotifications")) {
                yield return q.ExecuteArray<EventEntry>().Bind(() => ee);

                foreach (var el in ee) {
                    var dict = new Dictionary<string, object>();

                    if (el.BalloonTip)
                        dict["balloonTip"] = true;
                    if (el.MessageBox)
                        dict["messageBox"] = true;
                    if (el.Sound != null)
                        dict["sound"] = el.Sound;
                    if ((el.Endpoint != null) && (endpointNames.Contains(el.Endpoint)))
                        dict["endpoint"] = el.Endpoint;

                    cfgDict[el.Key] = dict;
                }
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(cfgDict);

            yield return CallFunction("eventnotifications", "notifySettingsChanged", json);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "eventnotifications", "initialize");

            Preferences.Flush();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new EventNotificationsConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Event Notifications", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Event Notifications");
            yield break;
        }

        public string[] GetEndpointNames () {
            var names = new HashSet<string>();

            foreach (var instance in Program.GetScriptInstances<IMessageGateway>()) {
                foreach (var name in instance.GetEndpoints())
                    names.Add(name);
            }

            return names.ToArray();
        }

        public void SendToEndpoint (ProcessInfo process, string endpoint, string message) {
            foreach (var instance in Program.GetScriptInstances<IMessageGateway>())
                if (instance.Send(endpoint, message))
                    return;

            throw new Exception("No script could send your message to an endpoint of that name.");
        }
    }
}
