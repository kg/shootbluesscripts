using System;
using System.Collections.Generic;
using System.Text;
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
    public class ColorEntry {
        [Column("key")]
        public string Key;
        [Column("red")]
        public float Red;
        [Column("green")]
        public float Green;
        [Column("blue")]
        public float Blue;

        internal Color Color {
            get {
                return Color.FromArgb(
                    (int)Math.Ceiling(Red * 255),
                    (int)Math.Ceiling(Green * 255),
                    (int)Math.Ceiling(Blue * 255)
                );
            }
            set {
                Red = value.R / 255.0f;
                Green = value.G / 255.0f;
                Blue = value.B / 255.0f;
            }
        }
    }

    public class TargetColors : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public Dictionary<string, Color> DefinedColors = new Dictionary<string,Color>();

        public TargetColors (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("targetcolors.py");

            CustomMenu = new ToolStripMenuItem("Target Colors");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureTargetColors);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureTargetColors (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Target Colors"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            yield return Program.CreateDBTable(
                "targetColors",
                "( key TEXT PRIMARY KEY NOT NULL, red FLOAT NOT NULL, green FLOAT NOT NULL, blue FLOAT NOT NULL )"
            );

            yield return BaseInitialize();
        }

        public IEnumerator<object> DefineColor (string key, Color defaultValue) {
            DefinedColors[key] = defaultValue;

            yield break;
        }

        protected override IEnumerator<object> OnPreferenceChanged (EventInfo evt, string prefName) {
            var colorDict = new Dictionary<string, object>();

            foreach (var kvp in DefinedColors)
                colorDict[kvp.Key] = new float[] { kvp.Value.R / 255.0f, kvp.Value.G / 255.0f, kvp.Value.B / 255.0f };

            using (var q = Program.Database.BuildQuery("SELECT key, red, green, blue FROM targetColors"))
            using (var e = q.Execute<ColorEntry>())
            while (!e.Disposed) {
                yield return e.Fetch();

                foreach (var item in e)
                    colorDict[item.Key] = new float[] { item.Red, item.Green, item.Blue };
            }

            var serializer = new JavaScriptSerializer();
            var colorsJson = serializer.Serialize(colorDict); 
            
            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "targetcolors", "notifyColorsChanged", colorsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "targetcolors", "initialize");

            EventBus.Broadcast(this, "PreferenceChanged", "*");
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new TargetColorsConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Target Colors", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Target Colors");
            yield break;
        }
    }
}
