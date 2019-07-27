using System;
using System.Linq;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    internal class CommandManager : IDisposable
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
            Execute(MessageTarget.Server, e);
        }

        private void NotificationsOnChannelMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Execute(MessageTarget.Channel, e);
        }

        private void NotificationsOnClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Execute(MessageTarget.Client, e);
        }

        private void Execute(MessageTarget messageMode, MessageReceivedEventArgs eArgs)
        {
            if (!IsItCommand(eArgs.Message, eArgs.InvokerUniqueId))
                return;
            var message = new Message(eArgs.Message);

            var commands = PluginManager.CommandList
                .Where(x => (x.Command.MessageType & messageMode) == messageMode
                            && message.ParamString.StartsWith(x.Command.CommandName, StringComparison.OrdinalIgnoreCase)).ToList();

            var command = commands.OrderByDescending(x => x.Command.CommandName.Length).FirstOrDefault();
            if (command == null)
                return;
            if (command.ServerGroups != null && command.ServerGroups.Groups.Length != 0)
            {
                var databaseId = Ts3Bot.GetClientNameAndDatabaseIdByUniqueId(eArgs.InvokerUniqueId).ClientDatabaseId;
                if (databaseId != null)
                {
                    var clientGroups = Ts3Bot.GetServerGroupsByClientId(databaseId.Value)
                        .Select(x => new { x.Id, x.Name }).ToDictionary(x => (int)x.Id, x => x.Name);
                    if (!command.ServerGroups.Groups.Intersect(clientGroups.Select(x => x.Value)).Any())
                        return;
                }
            }
            var msg = message;
            msg.Params = msg.Params.Skip(command.Command.CommandName.Count(y => y == ' ') + 1).ToArray();
            command.Invoke(eArgs, msg.Params, messageMode);
        }

        private bool IsItCommand(string message, string uid)
        {
            return uid != Uid && message.StartsWith("!") && message.Length > 1;
        }

        public void Dispose()
        {
            Ts3Bot.Notifications.ClientMessageReceived -= NotificationsOnClientMessageReceived;
            Ts3Bot.Notifications.ChannelMessageReceived -= NotificationsOnChannelMessageReceived;
            Ts3Bot.Notifications.ServerMessageReceived -= NotificationsOnServerMessageReceived;
        }
    }
}
