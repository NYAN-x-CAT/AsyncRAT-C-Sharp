using Server.MessagePack;

namespace Server.Predefined {
    public class PredefinedCommand : MsgPack {
        public string CommandName { get; protected set; }

    }
}
