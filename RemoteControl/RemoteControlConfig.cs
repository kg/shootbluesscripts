using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShootBlues;
using Squared.Util.Bind;
using System.IO;
using Squared.Task;
using System.Diagnostics;

namespace ShootBlues.Script {
    public partial class RemoteControlConfig : RemoteControlConfigPanel {
        private NumericUpDown ServerPort;
        private Label label1;
        private Label label2;
        private TextBox ViewOnlyPassword;
        private TextBox FullAccessPassword;
        private Label label3;
        private GroupBox groupBox2;
        private NumericUpDown KeyframeThreshold;
        private Label label6;
        private NumericUpDown UpdateQuality;
        private Label label5;
        private NumericUpDown KeyframeQuality;
        private Label label4;
        private NumericUpDown MaxFrameRate;
        private Label label7;
        private LinkLabel OpenInBrowser;
        private GroupBox groupBox1;
    
        public RemoteControlConfig (RemoteControl script)
            : base (script) {

            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => ServerPort.Value),
                BoundMember.New(() => ViewOnlyPassword.Text),
                BoundMember.New(() => FullAccessPassword.Text),
                BoundMember.New(() => KeyframeQuality.Value),
                BoundMember.New(() => UpdateQuality.Value),
                BoundMember.New(() => KeyframeThreshold.Value),
                BoundMember.New(() => MaxFrameRate.Value)
            };
        }

        private void InitializeComponent () {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FullAccessPassword = new System.Windows.Forms.TextBox();
            this.ViewOnlyPassword = new System.Windows.Forms.TextBox();
            this.ServerPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MaxFrameRate = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.KeyframeThreshold = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.UpdateQuality = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.KeyframeQuality = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.OpenInBrowser = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPort)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxFrameRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.KeyframeThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.KeyframeQuality)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.FullAccessPassword);
            this.groupBox1.Controls.Add(this.ViewOnlyPassword);
            this.groupBox1.Controls.Add(this.ServerPort);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox1.Size = new System.Drawing.Size(264, 107);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Settings";
            // 
            // FullAccessPassword
            // 
            this.FullAccessPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FullAccessPassword.Location = new System.Drawing.Point(161, 75);
            this.FullAccessPassword.Margin = new System.Windows.Forms.Padding(1);
            this.FullAccessPassword.Name = "FullAccessPassword";
            this.FullAccessPassword.PasswordChar = '●';
            this.FullAccessPassword.Size = new System.Drawing.Size(95, 23);
            this.FullAccessPassword.TabIndex = 4;
            // 
            // ViewOnlyPassword
            // 
            this.ViewOnlyPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewOnlyPassword.Location = new System.Drawing.Point(161, 46);
            this.ViewOnlyPassword.Margin = new System.Windows.Forms.Padding(1);
            this.ViewOnlyPassword.Name = "ViewOnlyPassword";
            this.ViewOnlyPassword.PasswordChar = '●';
            this.ViewOnlyPassword.Size = new System.Drawing.Size(95, 23);
            this.ViewOnlyPassword.TabIndex = 2;
            // 
            // ServerPort
            // 
            this.ServerPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerPort.Location = new System.Drawing.Point(161, 17);
            this.ServerPort.Margin = new System.Windows.Forms.Padding(1);
            this.ServerPort.Maximum = new decimal(new int[] {
            32766,
            0,
            0,
            0});
            this.ServerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ServerPort.Name = "ServerPort";
            this.ServerPort.Size = new System.Drawing.Size(95, 23);
            this.ServerPort.TabIndex = 0;
            this.ServerPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(1, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(164, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "Full Access Password:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(1, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "View-only Password:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.MaxFrameRate);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.KeyframeThreshold);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.UpdateQuality);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.KeyframeQuality);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(3, 114);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(264, 127);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Quality Settings";
            // 
            // MaxFrameRate
            // 
            this.MaxFrameRate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MaxFrameRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.MaxFrameRate.Location = new System.Drawing.Point(161, 95);
            this.MaxFrameRate.Margin = new System.Windows.Forms.Padding(1);
            this.MaxFrameRate.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.MaxFrameRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MaxFrameRate.Name = "MaxFrameRate";
            this.MaxFrameRate.Size = new System.Drawing.Size(95, 23);
            this.MaxFrameRate.TabIndex = 8;
            this.MaxFrameRate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(1, 95);
            this.label7.Margin = new System.Windows.Forms.Padding(1);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(164, 23);
            this.label7.TabIndex = 9;
            this.label7.Text = "Max Frame Rate:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // KeyframeThreshold
            // 
            this.KeyframeThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyframeThreshold.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.KeyframeThreshold.Location = new System.Drawing.Point(161, 70);
            this.KeyframeThreshold.Margin = new System.Windows.Forms.Padding(1);
            this.KeyframeThreshold.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.KeyframeThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.KeyframeThreshold.Name = "KeyframeThreshold";
            this.KeyframeThreshold.Size = new System.Drawing.Size(95, 23);
            this.KeyframeThreshold.TabIndex = 6;
            this.KeyframeThreshold.Value = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(1, 70);
            this.label6.Margin = new System.Windows.Forms.Padding(1);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(164, 23);
            this.label6.TabIndex = 7;
            this.label6.Text = "Keyframe Threshold:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UpdateQuality
            // 
            this.UpdateQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateQuality.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.UpdateQuality.Location = new System.Drawing.Point(161, 45);
            this.UpdateQuality.Margin = new System.Windows.Forms.Padding(1);
            this.UpdateQuality.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.UpdateQuality.Name = "UpdateQuality";
            this.UpdateQuality.Size = new System.Drawing.Size(95, 23);
            this.UpdateQuality.TabIndex = 4;
            this.UpdateQuality.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(1, 45);
            this.label5.Margin = new System.Windows.Forms.Padding(1);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(164, 23);
            this.label5.TabIndex = 5;
            this.label5.Text = "Update Quality:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // KeyframeQuality
            // 
            this.KeyframeQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyframeQuality.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.KeyframeQuality.Location = new System.Drawing.Point(161, 20);
            this.KeyframeQuality.Margin = new System.Windows.Forms.Padding(1);
            this.KeyframeQuality.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.KeyframeQuality.Name = "KeyframeQuality";
            this.KeyframeQuality.Size = new System.Drawing.Size(95, 23);
            this.KeyframeQuality.TabIndex = 2;
            this.KeyframeQuality.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(1, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(1);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(164, 23);
            this.label4.TabIndex = 3;
            this.label4.Text = "Keyframe Quality:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OpenInBrowser
            // 
            this.OpenInBrowser.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(255)))));
            this.OpenInBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenInBrowser.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.OpenInBrowser.LinkColor = System.Drawing.Color.Blue;
            this.OpenInBrowser.Location = new System.Drawing.Point(4, 244);
            this.OpenInBrowser.Name = "OpenInBrowser";
            this.OpenInBrowser.Size = new System.Drawing.Size(263, 22);
            this.OpenInBrowser.TabIndex = 2;
            this.OpenInBrowser.TabStop = true;
            this.OpenInBrowser.Text = "Open In Browser";
            this.OpenInBrowser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.OpenInBrowser.VisitedLinkColor = System.Drawing.Color.Blue;
            this.OpenInBrowser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenInBrowser_LinkClicked);
            // 
            // RemoteControlConfig
            // 
            this.Controls.Add(this.OpenInBrowser);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.MinimumSize = new System.Drawing.Size(270, 270);
            this.Name = "RemoteControlConfig";
            this.Size = new System.Drawing.Size(270, 270);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPort)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MaxFrameRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.KeyframeThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.KeyframeQuality)).EndInit();
            this.ResumeLayout(false);

        }

        private void OpenInBrowser_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(
                String.Format("http://localhost:{0}/eve/", ServerPort.Value)
            );
        }
    }

    public class RemoteControlConfigPanel : SimpleConfigPanel<RemoteControl> {
        public RemoteControlConfigPanel ()
            : base(null) {
        }

        public RemoteControlConfigPanel (RemoteControl script)
            : base(script) {
        }
    }
}
