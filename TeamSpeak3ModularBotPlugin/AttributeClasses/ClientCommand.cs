using System;
using TS3QueryLib.Core.CommandHandling;

namespace TeamSpeak3ModularBotPlugin.AttributeClasses
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]

    public class ClientCommand : Attribute
    {
        public string CommandName { get; }

        public MessageTarget MessageType { get; }

        public string OnFailedMessage { get; }

        public string CommandInfo { get; }

        public ClientCommand(string commandName, MessageTarget msgType = MessageTarget.All, string onFailedMessage = null)
        {
            OnFailedMessage = onFailedMessage;
            CommandName = commandName;
            MessageType = msgType;
        }

        public ClientCommand(string commandName, string commandInfo, MessageTarget msgType = MessageTarget.All, string onFailedMessage = null)
        {
            OnFailedMessage = onFailedMessage;
            CommandName = commandName;
            MessageType = msgType;
            CommandInfo = commandInfo;
        }
    }
}
