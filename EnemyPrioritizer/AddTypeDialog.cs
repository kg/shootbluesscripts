using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using Squared.Task.Data.Mapper;

namespace ShootBlues.Script {
    public partial class AddTypeDialog : TaskForm {
        protected long CategoryFilter;

        public AddTypeDialog (TaskScheduler scheduler) 
            : base (scheduler) {
            InitializeComponent();
        }

        public IEnumerator<object> PopulateGroups (long categoryFilter, int selectedGroupID) {
            CategoryFilter = categoryFilter;

            Groups.BeginUpdate();
            Groups.Items.Clear();
            Groups.Sorted = false;

            object selectedItem = "<All Items>";

            Groups.Items.Add(selectedItem);

            using (var q = Program.Database.BuildQuery(
                "SELECT groupID, name FROM evedata.groups WHERE categoryID = ?"
            ))
            using (var e = q.Execute<GroupEntry>(categoryFilter))
            while (!e.Disposed) {
                yield return e.Fetch();

                foreach (var item in e) {
                    Groups.Items.Add(item);
                    if (selectedGroupID == item.ID)
                        selectedItem = item;
                }
            }

            Groups.Sorted = true;
            Groups.SelectedItem = selectedItem;
            Groups.EndUpdate();
        }

        public IEnumerator<object> RefreshList () {
            using (new ControlWaitCursor(this)) {
                int oldSelectedType = -1;
                if (Types.SelectedItem is TypeEntry)
                    oldSelectedType = (Types.SelectedItem as TypeEntry).TypeID;

                Types.BeginUpdate();
                Types.Items.Clear();
                Types.Sorted = false;
                OKButton.Enabled = false;

                string sql;
                object[] args;

                if (Groups.SelectedItem is GroupEntry) {
                    var group = Groups.SelectedItem as GroupEntry;
                    sql = "SELECT groupID, typeID, name FROM evedata.types WHERE groupID = ?";
                    args = new object[] { group.ID };
                } else {
                    sql = "SELECT groupID, typeID, name FROM evedata.types WHERE categoryID = ?";
                    args = new object[] { CategoryFilter };
                }

                using (var q = Program.Database.BuildQuery(sql))
                using (var e = q.Execute<TypeEntry>(args))
                while (!e.Disposed) {
                    yield return e.Fetch();

                    foreach (var item in e) {
                        Types.Items.Add(item);
                        if (item.TypeID == oldSelectedType)
                            Types.SelectedItem = item;
                    }
                }

                Types.Sorted = true;
                Types.EndUpdate();
            }
        }

        private void Groups_SelectedIndexChanged (object sender, EventArgs e) {
            if (Visible && !this.UseWaitCursor)
                Start(RefreshList());
        }

        private void Types_SelectedIndexChanged (object sender, EventArgs e) {
            OKButton.Enabled = (Types.SelectedItem is TypeEntry);
        }
    }

    [Mapper]
    class TypeEntry {
        [Column("groupID")]
        public int GroupID {
            get;
            set;
        }

        [Column("typeID")]
        public int TypeID {
            get;
            set;
        }

        [Column("name")]
        public string Name {
            get;
            set;
        }

        public override string ToString () {
            return Name;
        }
    }
}
