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
    public partial class EnemyPrioritizerConfig : TaskUserControl {
        EnemyPrioritizer Script;

        public EnemyPrioritizerConfig (EnemyPrioritizer script) 
            : base (Program.Scheduler) {
            InitializeComponent();
            Script = script;
        }

        public IEnumerator<object> RefreshList () {
            return RefreshList(null);
        }

        private IEnumerator<object> RefreshList (PriorityEntry newSelection) {
            PriorityEntry[] oldSelectedEntries;
            if (newSelection != null)
                oldSelectedEntries = new PriorityEntry[] { newSelection };
            else
                oldSelectedEntries = SelectedEntries.ToArray();

            using (new ControlWaitCursor(this)) {
                List.BeginUpdate();

                List.Items.Clear();
                List.Groups.Clear();

                int[] priorities = null;
                yield return Program.Database.ExecutePrimitiveArray<int>(
                    "SELECT DISTINCT priority FROM enemyPriorities ORDER BY priority DESC"
                ).Bind(() => priorities);

                var priorityGroups = new Dictionary<int, ListViewGroup>();

                foreach (int priority in priorities) {
                    if (priorityGroups.ContainsKey(priority))
                        continue;
                    else if (priority <= 0)
                        continue;

                    List.Groups.Add(priorityGroups[priority] = new ListViewGroup(
                        String.Format("priority:{0}", priority),
                        String.Format("Priority +{0}", priority)
                    ));
                }

                List.Groups.Add(priorityGroups[0] = new ListViewGroup(
                    "priority:0", "Default Priority"
                ));
                List.Groups.Add(priorityGroups[-1] = new ListViewGroup(
                    "priority:-1", "Never Attack"
                ));

                PriorityEntry[] entries = null;
                yield return Program.Database.ExecuteArray<PriorityEntry>(
                    "SELECT ep.groupID, ep.typeID, ep.priority, g.name AS groupName, t.name AS typeName " +
                    "FROM enemyPriorities ep " +
                    "LEFT OUTER JOIN evedata.groups g ON ep.groupID = g.groupID " +
                    "LEFT OUTER JOIN evedata.types t ON ep.typeID = t.typeID"
                ).Bind(() => entries);

                foreach (var entry in entries) {
                    var item = new ListViewItem(
                        entry.TypeName ?? entry.GroupName,
                        (entry.TypeName != null) ? "type" : "group",
                        priorityGroups[Math.Max(entry.Priority, -1)]
                    );
                    item.Tag = entry;

                    List.Items.Add(item);

                    foreach (var oldEntry in oldSelectedEntries) {
                        if ((oldEntry.TypeID == entry.TypeID) && (oldEntry.GroupID == entry.GroupID)) {
                            item.Selected = true;
                            break;
                        }
                    }
                }

                foreach (var group in priorityGroups.Values) {
                    if (group.Items.Count != 0)
                        continue;

                    var nullItem = new ListViewItem(
                        "", group
                    );
                    nullItem.Tag = null;
                    List.Items.Add(nullItem);
                }

                List.EndUpdate();
            }
        }

        private void List_SelectedIndexChanged (object sender, EventArgs e) {
            PriorityDown.Enabled = PriorityUp.Enabled = 
                SelectedEntries.Count() > 0;
        }

        private void AddGroup_Click (object sender, EventArgs e) {
            Start(AddGroupTask());
        }

        private IEnumerator<object> AddGroupTask () {
            using (new ControlWaitCursor(this))
            using (var dialog = new AddGroupDialog()) {
                long categoryID = 0;
                yield return Program.Database.ExecuteScalar<long>(
                    "SELECT categoryID FROM evedata.categories WHERE name = ?",
                    "Entity"
                ).Bind(() => categoryID);

                int groupID = (from se in SelectedEntries
                               where se.GroupID.HasValue
                               select se.GroupID.Value).FirstOrDefault();

                yield return dialog.PopulateList(categoryID, groupID);

                if (dialog.ShowDialog() != DialogResult.OK)
                    yield break;

                var newGroup = dialog.Group.SelectedItem as GroupEntry;

                int priority = 0;
                if (SelectedEntries.Count() > 0)
                    priority = SelectedEntries.Max((pe) => pe.Priority);

                using (var xact = Program.Database.CreateTransaction()) {
                    yield return xact;

                    yield return Program.Database.ExecuteSQL(
                        "DELETE FROM enemyPriorities WHERE groupID = ? and typeID IS NULL",
                        newGroup.ID
                    );

                    yield return Program.Database.ExecuteSQL(
                        "REPLACE INTO enemyPriorities (groupID, priority) VALUES (?, ?)",
                        newGroup.ID, priority
                    );

                    yield return xact.Commit();
                }

                yield return RefreshList(new PriorityEntry { GroupID = newGroup.ID });
            }
        }

        private void AddType_Click (object sender, EventArgs e) {
            Start(AddTypeTask());
        }

        private IEnumerator<object> AddTypeTask () {
            using (new ControlWaitCursor(this))
            using (var dialog = new AddTypeDialog(Program.Scheduler)) {
                long categoryID = 0;
                yield return Program.Database.ExecuteScalar<long>(
                    "SELECT categoryID FROM evedata.categories WHERE name = ?",
                    "Entity"
                ).Bind(() => categoryID);

                int groupID = (from se in SelectedEntries
                               where se.GroupID.HasValue
                               select se.GroupID.Value).FirstOrDefault();

                yield return dialog.PopulateGroups(categoryID, groupID); 

                yield return dialog.RefreshList();

                if (dialog.ShowDialog() != DialogResult.OK)
                    yield break;

                var newType = dialog.Types.SelectedItem as TypeEntry;

                int priority = 0;
                if (SelectedEntries.Count() > 0)
                    priority = SelectedEntries.Max((pe) => pe.Priority);

                yield return Program.Database.ExecuteSQL(
                    "REPLACE INTO enemyPriorities (groupID, typeID, priority) VALUES (?, ?, ?)",
                    newType.GroupID, newType.TypeID, priority
                );

                yield return RefreshList(new PriorityEntry { GroupID = newType.GroupID, TypeID = newType.TypeID });
            }
        }

        private void PriorityUp_Click (object sender, EventArgs e) {
            Start(AdjustPriorityTask(SelectedEntries.ToArray(), 1));
        }

        private void PriorityDown_Click (object sender, EventArgs e) {
            Start(AdjustPriorityTask(SelectedEntries.ToArray(), -1));
        }

        private IEnumerable<PriorityEntry> SelectedEntries {
            get {
                return (
                    from item in List.SelectedItems.Cast<ListViewItem>()
                    where item.Tag is PriorityEntry
                    select item.Tag as PriorityEntry
                );
            }
        }

        private IEnumerator<object> AdjustPriorityTask (PriorityEntry[] items, int delta) {
            using (new ControlWaitCursor(this))
            using (var xact = Program.Database.CreateTransaction()) {
                yield return xact;

                object[] args;

                foreach (var item in items) {
                    string sql = "UPDATE enemyPriorities SET priority = MAX(-1, priority + ?) WHERE ";
                    if (item.TypeID.HasValue) {
                        sql += "typeID = ? AND groupID = ?";
                        args = new object[] { delta, item.TypeID.Value, item.GroupID.Value };
                    } else {
                        sql += "groupID = ? AND typeID IS NULL";
                        args = new object[] { delta, item.GroupID.Value };
                    }

                    yield return Program.Database.ExecuteSQL(sql, args);
                }

                yield return xact.Commit();
            }

            yield return RefreshList();
        }

        private void List_SizeChanged (object sender, EventArgs e) {
            var clientWidth = List.ClientSize.Width - 4;
            List.Columns[0].Width = clientWidth;
        }
    }
}
