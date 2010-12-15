using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coversant.SoapBox.Core;
using Coversant.SoapBox.Base;
using Squared.Task;

namespace ShootBlues.Script {
    public static class Jabber {
        public static Future<Session> AsyncLogin (
            string username, string password, string resource, 
            bool enableNonSASLAuth, ConnectionOptions options
        ) {
            var f = new Future<Session>();
            Session.BeginLogin(
                username, password, resource, enableNonSASLAuth, options,
                (_) => {
                    try {
                        f.Complete(Session.EndLogin(_));
                    } catch (Exception ex) {
                        f.Fail(ex);
                    }
                }, null
            );
            return f;
        }

        public static Future<Packet> AsyncSend (
            this Session session, Packet packet
        ) {
            var f = new Future<Packet>();
            session.BeginSend(packet, (_) => {
                try {
                    f.Complete(session.EndSend(_));
                } catch (Exception ex) {
                    f.Fail(ex);
                }
            });
            return f;
        }
    }
}
