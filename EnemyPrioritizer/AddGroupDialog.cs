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
    public partial class AddGroupDialog : Form {
        public AddGroupDialog () 
            : base() {
            InitializeComponent();
        }

        public IEnumerator<object> PopulateList (long categoryFilter, int selectedGroupID) {
            Group.BeginUpdate();
            Group.Items.Clear();
            Group.Sorted = false;

            object selectedItem = null;

            using (var q = Program.Database.BuildQuery(
                "SELECT groupID, name FROM evedata.groups WHERE categoryID = ?"
            ))
            using (var e = q.Execute<GroupEntry>(categoryFilter))
            while (!e.Disposed) {
                yield return e.Fetch();

                foreach (var item in e) {
                    Group.Items.Add(item);

                    if (selectedGroupID == item.ID)
                        selectedItem = item;
                }
            }

            Group.Sorted = true;
            Group.SelectedItem = selectedItem;

            Group.EndUpdate();
        }

        private void Group_SelectedIndexChanged (object sender, EventArgs e) {
            OKButton.Enabled = (Group.SelectedItem != null);
        }
    }

    [Mapper]
    class GroupEntry {
        [Column("groupID")]
        public int ID {
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
