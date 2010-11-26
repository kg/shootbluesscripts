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
    public partial class TargetColorsConfig : TaskUserControl, IConfigurationPanel {
        TargetColors Script;

        public TargetColorsConfig (TargetColors script)
            : base(Program.Scheduler) {
            InitializeComponent();
            Script = script;
        }

        public IEnumerator<object> LoadConfiguration () {
            using (new ControlWaitCursor(this)) {
                List.BeginUpdate();

                string oldSelection = null;
                {
                    var oldItem = List.SelectedItem as ColorEntry;
                    if (oldItem != null)
                        oldSelection = oldItem.Key;
                }

                List.Items.Clear();

                ColorEntry[] entries = null;
                yield return Program.Database.ExecuteArray<ColorEntry>(
                    "SELECT key, red, green, blue FROM targetColors ORDER BY key ASC"
                ).Bind(() => entries);

                var entryDict = entries.ToDictionary((e) => e.Key);
                var keys = Script.DefinedColors.Keys.ToArray();
                Array.Sort(keys);

                ColorEntry item;
                foreach (var key in keys) {
                    if (!entryDict.TryGetValue(key, out item)) {
                        var def = Script.DefinedColors[key];
                        item = new ColorEntry { 
                            Key = key, 
                            Color = def
                        };
                    }

                    List.Items.Add(item);

                    if (key.Equals(oldSelection, StringComparison.InvariantCultureIgnoreCase))
                        List.SelectedItem = item;
                }

                using (var g = List.CreateGraphics())
                    List.ItemHeight = (int)Math.Ceiling(g.MeasureString("AaBbYyZz", List.Font).Height) + 2;

                List.EndUpdate();
            }
        }

        public IEnumerator<object> SaveConfiguration () {
            yield break;
        }

        private void List_SelectedIndexChanged (object sender, EventArgs e) {
            SetColor.Enabled = ResetToDefault.Enabled =
                (List.SelectedIndices.Count > 0);
        }

        private void List_DrawItem (object sender, DrawItemEventArgs e) {
            var g = e.Graphics;
            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool focused = (e.State & DrawItemState.Focus) == DrawItemState.Focus;

            var item = List.Items[e.Index] as ColorEntry;
            if (item != null) {
                int boxSize = e.Bounds.Height - 6;
                var boxRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, boxSize + 1, boxSize + 1);

                var itemColor = item.Color;

                using (var outlinePen = new Pen(selected ? SystemColors.HighlightText : SystemColors.WindowText, 1.0f))
                using (var colorBrush = new SolidBrush(itemColor)) {                    
                    g.FillRectangle(colorBrush, boxRect);
                    g.DrawRectangle(outlinePen, boxRect);
                }

                var textRect = new RectangleF(
                    e.Bounds.X + boxSize + 7.0f, e.Bounds.Y, e.Bounds.Width - boxSize - 4.0f, e.Bounds.Height
                );

                using (var fmt = StringFormat.GenericTypographic.Clone() as StringFormat)
                using (var textBrush = new SolidBrush(e.ForeColor)) {
                    fmt.Alignment = StringAlignment.Near;
                    fmt.LineAlignment = StringAlignment.Center;
                    fmt.Trimming = StringTrimming.EllipsisCharacter;
                    fmt.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
                    g.DrawString(item.Key, e.Font, textBrush, textRect, fmt);
                }
            }

            if (focused)
                e.DrawFocusRectangle();
        }

        private void SetColor_Click (object sender, EventArgs e) {
            var item = List.SelectedItem as ColorEntry;
            if (item == null)
                return;

            using (var dlg = new ColorDialog()) {
                dlg.Color = item.Color;
                dlg.AllowFullOpen = true;
                dlg.AnyColor = true;
                dlg.FullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    Start(SetItemColor(item, dlg.Color));
            }
        }

        private void ResetToDefault_Click (object sender, EventArgs e) {
            var item = List.SelectedItem as ColorEntry;
            if (item == null)
                return;

            Start(SetItemColor(item, null));
        }

        public IEnumerator<object> SetItemColor (ColorEntry item, Color? newColor) {
            if (newColor.HasValue) {
                item.Color = newColor.Value;

                using (var q = Program.Database.BuildQuery(
                    "REPLACE INTO targetColors (key, red, green, blue) VALUES (?, ?, ?, ?)"
                ))
                    yield return q.ExecuteNonQuery(item.Key, item.Red, item.Green, item.Blue);
            } else {
                using (var q = Program.Database.BuildQuery(
                    "DELETE FROM targetColors WHERE key = ?"
                ))
                    yield return q.ExecuteNonQuery(item.Key);
            }

            Script.PreferencesChanged.Set();

            yield return LoadConfiguration();
        }
    }
}
