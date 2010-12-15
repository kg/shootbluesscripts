using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Util.Bind;
using Squared.Util;

namespace ShootBlues.Script {
    public struct Mapping {
        public readonly IBoundMember Setting;
        public readonly IBoundMember Control;

        public Mapping (IBoundMember setting, IBoundMember control) {
            Setting = setting;
            Control = control;
        }
    }

    public partial class AddEndpointDialog : Form {
        public readonly EndpointSettings Settings;
        public readonly Mapping[] Prefs;

        public AddEndpointDialog (EndpointSettings settings) {
            InitializeComponent();

            if (settings != null) {
                Settings = settings;
                EndpointName.ReadOnly = true;
            } else
                Settings = new EndpointSettings();
            
            Prefs = new[] {
                new Mapping(
                    BoundMember.New(() => Settings.Name),
                    BoundMember.New(() => EndpointName.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.Server),
                    BoundMember.New(() => Server.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.Username),
                    BoundMember.New(() => Username.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.Password),
                    BoundMember.New(() => Password.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.Resource),
                    BoundMember.New(() => Resource.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.ChatChannel),
                    BoundMember.New(() => ChatChannel.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.ChatAlias),
                    BoundMember.New(() => ChatAlias.Text)
                ),
                new Mapping(
                    BoundMember.New(() => Settings.ToUsername),
                    BoundMember.New(() => ToUsername.Text)
                )
            };

            foreach (var pref in Prefs)
                pref.Control.Value = pref.Setting.Value;

            SendToGroupChat.Checked = (ChatChannel.Text ?? "").Length > 0;
            SendToUser.Checked = (ToUsername.Text ?? "").Length > 0;

            RefreshEnabledState(null, EventArgs.Empty);
        }

        private void OKButton_Click (object sender, EventArgs e) {
            if (!SendToGroupChat.Checked) {
                ChatChannel.Text = null;
                ChatAlias.Text = null;
            }

            if (!SendToUser.Checked)
                ToUsername.Text = null;

            foreach (var pref in Prefs)
                pref.Setting.Value = pref.Control.Value;
        }

        private void RefreshEnabledState (object sender, EventArgs e) {
            OKButton.Enabled = ((EndpointName.Text ?? "").Length > 0) &&
                ((Server.Text ?? "").Length > 0) &&
                ((Username.Text ?? "").Length > 0) &&
                ((Password.Text ?? "").Length > 0) &&
                (
                    (SendToGroupChat.Checked && (ChatChannel.Text ?? "").Length > 0) ||
                    (SendToUser.Checked && (ToUsername.Text ?? "").Length > 0)
                );
        }

        private void SendToGroupChat_CheckedChanged (object sender, EventArgs e) {
            SendToGroupChatGroup.Enabled = SendToGroupChat.Checked;
            RefreshEnabledState(sender, e);
        }

        private void SendToUser_CheckedChanged (object sender, EventArgs e) {
            SendToUserGroup.Enabled = SendToUser.Checked;
            RefreshEnabledState(sender, e);
        }
    }
}
