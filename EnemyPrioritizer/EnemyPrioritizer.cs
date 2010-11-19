using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Squared.Task.Data.Mapper;

namespace ShootBlues.Script {
    [Mapper]
    class PriorityEntry {
        [Column("groupID")]
        public int? GroupID {
            get;
            set;
        }
        [Column("typeID")]
        public int? TypeID {
            get;
            set;
        }
        [Column("priority")]
        public int Priority {
            get;
            set;
        }
        [Column("groupName")]
        public string GroupName {
            get;
            set;
        }
        [Column("typeName")]
        public string TypeName {
            get;
            set;
        }
    }

    public class EnemyPrioritizer : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public EnemyPrioritizer (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("enemyprioritizer.py");

            CustomMenu = new ToolStripMenuItem("Enemy Prioritizer");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureEnemyPrioritizer);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureEnemyPrioritizer (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Enemy Prioritizer"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            yield return Program.AttachDB(
                "evedata"
            );

            yield return Program.CreateDBTable(
                "enemyPriorities",
                "( groupID INTEGER NOT NULL, typeID INTEGER, priority INTEGER NOT NULL, PRIMARY KEY (groupID, typeID) )"
            );

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferencesChanged () {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "enemyprioritizer", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "enemyprioritizer", "initialize");

            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);
            yield return Program.CallFunction(process, "enemyprioritizer", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            // Delete any stray items with a priority of 0 since the default is 0.
            // When the user interacts with the config window, new items start at
            //  priority 0, so if they never set a priority, they stick around.
            // This kind of sucks, but it shouldn't be too confusing.
            yield return Program.Database.ExecuteSQL(
                "DELETE FROM enemyPriorities WHERE priority = 0"
            );

            var panel = new EnemyPrioritizerConfig(this);
            yield return panel.RefreshList();
            statusWindow.ShowConfigurationPanel("Enemy Prioritizer", panel);
            yield break;
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Enemy Prioritizer");
            yield break;
        }
    }
}
