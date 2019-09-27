using Server.MessagePack;

namespace Server.Predefined.Commands
{
    public class PredefinedRecoveryCommand : PredefinedCommand {
        public PredefinedRecoveryCommand()
        {
            ForcePathObject("Packet").AsString = "recoveryPassword";
            ForcePathObject("Plugin").SetAsBytes(Properties.Resources.PluginRecovery);

            CommandName = "Password Recovery Command";
        }
    }
}
