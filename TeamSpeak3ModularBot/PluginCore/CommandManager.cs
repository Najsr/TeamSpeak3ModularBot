using System.Linq;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    internal class CommandManager
    {
        private QueryRunner Ts3Bot { get; }

        private PluginManager PluginManager { get; }

        private string Uid { get; }

        public CommandManager(QueryRunner qr, PluginManager pm)
        {
            Ts3Bot = qr;
            PluginManager = pm;
            Uid = Ts3Bot.SendWhoAmI().ClientUniqueId;
            Ts3Bot.Notifications.ClientMessageReceived += NotificationsOnClientMessageReceived;
            Ts3Bot.Notifications.ChannelMessageReceived += NotificationsOnChannelMessageReceived;
            Ts3Bot.Notifications.ServerMessageReceived += NotificationsOnServerMessageReceived;
        }

        private void NotificationsOnServerMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!IsItCommand(e))
                return;
            var message = new Message(e.Message);
            Execute(ClientCommand.MessageMode.Server, e, message);
        }

        private void NotificationsOnChannelMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!IsItCommand(e))
                return;
            var message = new Message(e.Message);
            Execute(ClientCommand.MessageMode.Channel, e, message);
        }

        private void NotificationsOnClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!IsItCommand(e))
                return;
            var message = new Message(e.Message);
            Execute(ClientCommand.MessageMode.Private, e, message);
        }

        private void Execute(ClientCommand.MessageMode messageMode, MessageReceivedEventArgs eArgs, Message message)
        {
            PluginManager.CommandList
                .Where(x => (x.Command.MessageType & messageMode) == messageMode && x.Command.Message == message.Command)
                .ToList()
                .ForEach(x => x.Invoke(eArgs, message));
        }

        private bool IsItCommand(MessageReceivedEventArgs eArgs)
        {
            return !(eArgs.InvokerUniqueId == Uid || !eArgs.Message.StartsWith("!") || eArgs.Message.Length < 2);
        }
    }
}
