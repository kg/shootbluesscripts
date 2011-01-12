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
using System.Net;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ShootBlues.Script {
    public class GoonfleetGateway : ManagedScript, IMessageGateway {
        public const double SendInterval = 5.0;
        public const int MessageBodySize = 1024;

        ToolStripMenuItem CustomMenu;
        IFuture SendTaskFuture = null;
        BlockingQueue<string> Queue;

        public GoonfleetGateway (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("EventNotifications.script.dll");

            CustomMenu = new ToolStripMenuItem("Goonfleet Gateway");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureGoonfleetGateway);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();

            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureGoonfleetGateway (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Goonfleet Gateway"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> Initialize () {
            StartSendTask();

            yield break;
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            StartSendTask();

            return base.OnPreferencesChanged(evt, prefNames);
        }

        protected void StartSendTask () {
            if (SendTaskFuture != null) {
                SendTaskFuture.Dispose();
                SendTaskFuture = null;
            }

            SendTaskFuture = Program.Scheduler.Start(SendTask(), TaskExecutionPolicy.RunAsBackgroundTask);
        }

        protected IEnumerator<object> SendTask () {
            var sleep = new Sleep(SendInterval);

            Dictionary<string, object> prefs = new Dictionary<string, object>();
            yield return Preferences.GetAll().Bind(() => prefs);

            List<string> allItems = new List<string>();

            var oldQueue = Queue;
            Queue = new BlockingQueue<string>();
            if (oldQueue != null)
                Queue.EnqueueMultiple(oldQueue.DequeueAll());

            while (true) {
                var nextItem = Queue.Dequeue();

                using (nextItem)
                    yield return nextItem;

                yield return sleep;

                allItems.Clear();
                allItems.Add(nextItem.Result);
                allItems.AddRange(Queue.DequeueAll());

                yield return new Start(
                    Send(prefs, allItems.ToArray()), TaskExecutionPolicy.RunAsBackgroundTask
                );
            }
        }

        protected IEnumerator<object> Send (Dictionary<string, object> prefs, string[] messages) {
            var request = (HttpWebRequest)WebRequest.Create(
                (string)prefs["URI"]
            );
            request.Method = "POST";
            request.ContentType = "text/xml";

            // Disable UTF8 BOM
            var encoding = new UTF8Encoding(false);

            var ms = new MemoryStream();
            var settings = new XmlWriterSettings() {
                Encoding = encoding,
                CloseOutput = false,
                CheckCharacters = true,
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true,
                IndentChars = " ",
                OmitXmlDeclaration = false
            };

            using (var xw = XmlWriter.Create(ms, settings)) {
                xw.WriteStartDocument();
                xw.WriteStartElement("messaging");

                xw.WriteStartElement("auth");
                xw.WriteElementString("sourceID", (string)prefs["Username"]);
                xw.WriteElementString("sharedKey", (string)prefs["Key"]);
                xw.WriteEndElement();

                xw.WriteStartElement("messages");
                int i = 0;
                foreach (var message in messages) {
                    xw.WriteStartElement("message");
                    xw.WriteElementString("id", i.ToString());
                    xw.WriteElementString("target", (string)prefs["Target"]);

                    if (message.Length > MessageBodySize)
                        xw.WriteElementString("text", message.Substring(0, MessageBodySize - 13) + " (truncated)");
                    else
                        xw.WriteElementString("text", message);

                    xw.WriteEndElement();

                    i += 1;
                }
                xw.WriteEndElement();

                xw.WriteEndElement();
            }

            int count = (int)ms.Length;
            var buf = ms.GetBuffer();
            request.ContentLength = count;

            using (var rs = request.GetRequestStream())
                rs.Write(buf, 0, count);

            Squared.Task.Web.Response response = null;
            yield return Squared.Task.Web.IssueRequest(request).Bind(
                () => response
            );

            try {
                var body = XElement.Parse(response.Body);
                if (body.Element("error") != null) {
                    Program.ShowErrorMessage(String.Format(
                        "Failed to send Goonfleet notifications: Server returned error code {0} ({1}).",
                        int.Parse(body.Element("error").Element("responseCode").Value),
                        body.Element("error").Element("response").Value
                    ));
                } else {
                    var responseCodes = from e in body.Elements("message")
                                        select new {
                                            id = int.Parse(e.Element("id").Value),
                                            responseCode = int.Parse(e.Element("responseCode").Value),
                                            responseText = e.Element("response").Value
                                        };

                    foreach (var rc in responseCodes) {
                        if (rc.responseCode >= 400)
                            Program.ShowErrorMessage(String.Format(
                                "Failed to send Goonfleet notification '{0}': Server returned error code {1} ({2}).",
                                messages[rc.id], rc.responseCode, rc.responseText
                            ));
                    }
                }
            } catch (Exception ex) {
                Program.ShowErrorMessage(String.Format(
                    "Failed to parse response from Goonfleet notification server: {0}",
                    ex
                ));
            }
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new GoonfleetGatewayConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Goonfleet Gateway", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Goonfleet Gateway");
            yield break;
        }

        string[] IMessageGateway.GetEndpoints () {
            return new[] { "Goonfleet" };
        }

        bool IMessageGateway.Send (string endpoint, string message) {
            if (endpoint == "Goonfleet") {
                Queue.Enqueue(message);
                return true;
            }

            return false;
        }
    }
}
