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
            Execute(MessageMode.Server, e);
        }

        private void NotificationsOnChannelMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Execute(MessageMode.Channel, e);
        }

        private void NotificationsOnClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Execute(MessageMode.Private, e);
        }

        private void Execute(MessageMode messageMode, MessageReceivedEventArgs eArgs)
        {
            if (!IsItCommand(eArgs.Message))
                return;
            var message = new Message(eArgs.Message);

            var commands = PluginManager.CommandList
                .Where(x => (x.Command.MessageType & messageMode) == messageMode
                            && message.ParamString.StartsWith(x.Command.Message)).ToList();

            commands.ForEach(commandStruct =>
            {
                if (commandStruct.ServerGroups != null && commandStruct.ServerGroups.Groups.Length != 0)
                {
                    var databaseId = Ts3Bot.GetClientNameAndDatabaseIdByUniqueId(eArgs.InvokerUniqueId).ClientDatabaseId;
                    if (databaseId != null)
                    {
                        var clientGroups = Ts3Bot.GetServerGroupsByClientId(databaseId.Value)
                            .Select(x => new {x.Id, x.Name}).ToDictionary(x => (int)x.Id, x => x.Name);
                        if (!commandStruct.ServerGroups.Groups.Intersect(clientGroups.Select(x => x.Value)).Any())
                            return;
                    }
                }
                var msg = message;
                msg.Params = msg.Params.Skip(commandStruct.Command.Message.Count(y => y == ' ') + 1).ToArray();
                commandStruct.Invoke(eArgs, msg.Params);
            });
        }

        private bool IsItCommand(string message)
        {
            return message != Uid && message.StartsWith("!") && message.Length > 1;
        }
    }
}
