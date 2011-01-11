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
using Coversant.SoapBox.Core;
using Coversant.SoapBox.Base;
using SoapboxCore = Coversant.SoapBox.Core;
using SoapboxBase = Coversant.SoapBox.Base;
using MUC = Coversant.SoapBox.Core.MultiUserChat;
using System.Xml;

namespace ShootBlues.Script {
    [Mapper(Explicit=true)]
    public class EndpointSettings {
        [Column("name")]
        public string Name;
        [Column("server")]
        public string Server;
        [Column("username")]
        public string Username;
        [Column("password")]
        public string Password;
        [Column("resource")]
        public string Resource;

        [Column("chatChannel")]
        public string ChatChannel;
        [Column("chatAlias")]
        public string ChatAlias;

        [Column("toUsername")]
        public string ToUsername;

        public JabberID GetRoomJid (bool includeUsername) {
            if ((ChatChannel ?? "").Length < 1)
                return null;

            if (includeUsername)
                return new JabberID(String.Format(
                    "{0}@conference.{1}/{2}", ChatChannel, Server, ChatAlias ?? Username
                ));
            else
                return new JabberID(String.Format(
                    "{0}@conference.{1}", ChatChannel, Server
                ));
        }

        public JabberID RecipientJid {
            get {
                if (ToUsername == null)
                    return null;

                return new JabberID(String.Format(
                    "{0}@{1}", ToUsername, Server
                ));
            }
        }
    }

    public class Endpoint : IDisposable {
        public readonly JabberGateway Gateway;
        public readonly EndpointSettings Settings;
        public readonly Session Session;
        public readonly BlockingQueue<string> Queue;
        public readonly IFuture QueueTaskFuture;

        protected Endpoint (JabberGateway gateway, EndpointSettings settings, Session session) {
            Gateway = gateway;
            Settings = settings;
            Session = session;
            Queue = Gateway.GetQueue(settings.Name);

            Endpoint oldEndpoint = null;
            if (gateway.Endpoints.TryGetValue(settings.Name, out oldEndpoint) && (oldEndpoint != null))
                oldEndpoint.Dispose();

            gateway.Endpoints[settings.Name] = this;

            QueueTaskFuture = Program.Scheduler.Start(QueueTask(), TaskExecutionPolicy.RunAsBackgroundTask);
        }

        public static Future<Endpoint> Connect (JabberGateway gateway, EndpointSettings settings, Action<float> setStatus) {
            var f = new Future<Endpoint>();
            Program.Scheduler.Start(
                f, new SchedulableGeneratorThunk(DoConnect(gateway, settings, setStatus)), 
                TaskExecutionPolicy.RunWhileFutureLives
            );
            return f;
        }

        protected static IEnumerator<object> HandlePing (Session session, string xml, long socketID) {
            try {
                var si = Program.GetScriptInstance<Common>("Common.Script.dll");
                if (si != null)
                    si.LogPrint(null, "Attempting to respond to jabber ping.");
                else
                    Console.WriteLine("Attempting to respond to jabber ping.");
            } catch {
            }

            var fDoc = Future.RunInThread(() => {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            });

            yield return fDoc;

            var root = fDoc.Result.FirstChild;
            var toJID = new JabberID(root.Attributes["to"].InnerText);
            var fromJID = new JabberID(root.Attributes["from"].InnerText);
            string packetID = root.Attributes["id"].InnerText;

            Packet packet = new SoapboxCore.IQ.IQResultResponse(
                fromJID, toJID, packetID, socketID
            );
            yield return session.AsyncSend(packet);
        }

        protected static void HandleIncomingMessage (JabberGateway gateway, string endpointName, JabberID from, string body) {
            try {
                var si = Program.GetScriptInstance<Common>("Common.Script.dll");
                if (si != null) {
                    var mdata = new ShootBlues.Script.Common.MessageData(
                        null, new Dictionary<string, object> {
                            {"__name__", "JabberMessage"},
                            {"endpoint", endpointName},
                            {"from", from.FullJabberID},
                            {"text", body}
                        }
                    );
                    si.OnNewMessage(gateway, mdata);
                }
            } catch (Exception ex) {
                Program.Scheduler.OnTaskError(ex);
            }
        }

        protected static IEnumerator<object> DoConnect (JabberGateway gateway, EndpointSettings settings, Action<float> setStatus) {
            Session session = null;
            var options = new ConnectionOptions(settings.Server);
            yield return Jabber.AsyncLogin(
                settings.Username, settings.Password, settings.Resource, false, options
            ).Bind(() => session);

            setStatus(0.5f);

            // Fucking soapbox doesn't handle pings or let you handle them with strongly typed packets, wee
            session.OnXMLReceived += (xml, socketID) => {
                if (xml.Contains("<ping xmlns=\"urn:xmpp:ping\""))
                    Program.Scheduler.Start(HandlePing(session, xml, socketID), TaskExecutionPolicy.RunAsBackgroundTask);
            };

            var messagePrefix = settings.ChatAlias.ToLowerInvariant() + ":";

            session.AddHandler(typeof(SoapboxCore.Message.ChatMessagePacket), (p) => {
                var cm = (SoapboxCore.Message.ChatMessagePacket)p;

                string bodyText = null;
                try {
                    bodyText = cm.Body;
                } catch {
                }

                if (bodyText != null)
                    HandleIncomingMessage(gateway, settings.Name, cm.From, bodyText);
            });

            session.AddHandler(typeof(MUC.Message.GroupChatMessage), (p) => {
                var gcm = (MUC.Message.GroupChatMessage)p;

                string bodyText = null;
                try {
                    bodyText = gcm.Body;
                } catch {
                }

                if ((bodyText != null) && (bodyText.ToLowerInvariant().Trim().StartsWith(messagePrefix))) {
                    bodyText = bodyText.Substring(bodyText.IndexOf(':') + 1);
                    HandleIncomingMessage(gateway, settings.Name, gcm.From, bodyText);
                }
            });

            session.OnAsynchronousException += (e) => {
                Program.Scheduler.OnTaskError(e);
            };

            var roomJid = settings.GetRoomJid(true);
            if (roomJid != null)
                yield return session.AsyncSend(new MUC.Presence.JoinRoomRequest(roomJid));

            setStatus(1.0f);

            yield return new Result(new Endpoint(
                gateway, settings, session
            ));
        }

        public Future<Packet> Send (string text) {
            var jid = Settings.GetRoomJid(false) ?? Settings.RecipientJid;
            Packet msg;

            if (jid == null)
                throw new InvalidOperationException("An endpoint must specify either a chat channel or a recipient");

            if ((Settings.ChatChannel ?? "").Length > 0) {
                msg = new MUC.Message.GroupChatMessage(jid, text);
            } else {
                msg = new SoapboxCore.Message.NormalMessagePacket(jid, text);
            }

            return Session.AsyncSend(msg);
        }

        protected IEnumerator<object> QueueTask () {
            Future<Packet> pendingSend = null;

            while (true) {
                var f = Queue.Dequeue();
                yield return f;

                if (pendingSend != null)
                    yield return pendingSend;
                pendingSend = Send(f.Result);

                yield return new Sleep(JabberGateway.SendInterval);
            }
        }

        public void Dispose () {
            Gateway.Endpoints[Settings.Name] = null;
            QueueTaskFuture.Dispose();
            Session.CloseStream();
            Session.Dispose();
        }
    }

    public class JabberGateway : ManagedScript, IMessageGateway {
        public const double SendInterval = 1.0;

        ToolStripMenuItem CustomMenu;
        public readonly Dictionary<string, Endpoint> Endpoints = new Dictionary<string, Endpoint>();
        public readonly Dictionary<string, BlockingQueue<string>> Queues = new Dictionary<string, BlockingQueue<string>>();

        public JabberGateway (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("EventNotifications.script.dll");
            AddDependency("jabber.py");

            CustomMenu = new ToolStripMenuItem("Jabber Gateway");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureJabberGateway);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();

            DestroyEndpoints();

            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        protected void DestroyEndpoints () {
            foreach (var ep in Endpoints.Values.ToArray()) {
                if (ep != null)
                    ep.Dispose();
            }
        }

        public void ConfigureJabberGateway (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Jabber Gateway"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> Initialize () {
            yield return Program.AttachDB(Path.Combine(
                Program.GetDataFolder(),
                "jabber.db"
            ), "jabber");                

            yield return Program.CreateDBTable(
                "jabber.endpoints",
                @"(name TEXT PRIMARY KEY NOT NULL, server TEXT NOT NULL, username TEXT NOT NULL, password TEXT NOT NULL, resource TEXT NOT NULL, chatChannel TEXT, chatAlias TEXT, toUsername TEXT)"
            );

            yield return InitGateways();
        }

        internal BlockingQueue<string> GetQueue (string endpointName) {
            BlockingQueue<string> result;
            if (!Queues.TryGetValue(endpointName, out result))
                result = Queues[endpointName] = new BlockingQueue<string>();

            return result;
        }

        // For some reason reiniting is flaky :(
        public IEnumerator<object> InitGateways () {
            DestroyEndpoints();
            Endpoints.Clear();

            EndpointSettings[] endpoints = null;
            using (var q = Database.BuildQuery("SELECT * FROM jabber.endpoints"))
                yield return q.ExecuteArray<EndpointSettings>().Bind(() => endpoints);

            foreach (var settings in endpoints)
                Endpoints[settings.Name] = null;

            if (endpoints.Length == 0)
                yield break;

            using (var lw = new LoadingWindow()) {
                lw.SetStatus("Connecting", null);
                lw.Text = "Jabber Gateway";
                lw.Show();                    

                float? current = null;
                float stepSize = (1.0f / endpoints.Length);
                var sleep = new Sleep(0.25f);

                foreach (var settings in endpoints) {
                    lw.SetStatus(String.Format("Connecting endpoint {0}", settings.Name), current);
                    Console.Write("Initializing endpoint '{0}'... ", settings.Name);
                    var f = Endpoint.Connect(
                        this, settings, 
                        (s) => lw.SetProgress(current + (s * stepSize))
                    );
                    yield return f;

                    if (f.Error != null) {
                        Console.WriteLine("failed: {0}", f.Error);
                        Scheduler.OnTaskError(f.Error);
                    } else {
                        Console.WriteLine("initialized.");
                    }

                    current = current.GetValueOrDefault(0.0f) + stepSize;
                    lw.SetProgress(current);

                    yield return sleep;
                }

                lw.SetStatus("Ready", 1.0f);
                lw.Close();
            }
        }

        public void Send (ProcessInfo process, string endpointName, string text) {
            GetQueue(endpointName).Enqueue(text);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "jabber", "initialize");
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new JabberGatewayConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Jabber Gateway", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Jabber Gateway");
            yield break;
        }

        string[] IMessageGateway.GetEndpoints () {
            return Endpoints.Keys.ToArray();
        }

        bool IMessageGateway.Send (string endpoint, string message) {
            if (Endpoints.ContainsKey(endpoint)) {
                GetQueue(endpoint).Enqueue(message);
                return true;
            }

            return false;
        }
    }
}
