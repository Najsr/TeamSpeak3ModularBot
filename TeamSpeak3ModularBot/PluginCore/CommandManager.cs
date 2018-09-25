﻿using System.Linq;
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
            var commands = PluginManager.CommandList
                .Where(x => (x.Command.MessageType & messageMode) == messageMode
                            && string.Join(" ", message.Params).ToLower().StartsWith(x.Command.Message))
                .ToList();
            foreach (var commandStruct in commands)
            {
                if (commandStruct.Command.Groups.Length != 0)
                {
                    var databaseId = Ts3Bot.GetClientNameAndDatabaseIdByUniqueId(eArgs.InvokerUniqueId).ClientDatabaseId;
                    if (databaseId != null)
                    {
                        var clientGroups = Ts3Bot.GetServerGroupsByClientId((uint)databaseId).Select(x => (int)x.Id).ToArray();
                        if (!commandStruct.Command.Groups.Intersect(clientGroups).Any())
                            return;
                    }
                }
                var msg = message;
                msg.Params = msg.Params.Skip(commandStruct.Command.Message.Count(y => y == ' ') + 1).ToArray();
                commandStruct.Invoke(eArgs, msg.Params);
            }
        }

        private bool IsItCommand(MessageReceivedEventArgs eArgs)
        {
            return !(eArgs.InvokerUniqueId == Uid || !eArgs.Message.StartsWith("!") || eArgs.Message.Length < 2);
        }
    }
}
