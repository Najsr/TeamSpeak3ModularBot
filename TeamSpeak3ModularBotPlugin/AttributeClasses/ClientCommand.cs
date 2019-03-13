using System;
using TS3QueryLib.Core.CommandHandling;

namespace TeamSpeak3ModularBotPlugin.AttributeClasses
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]

    public class ClientCommand : Attribute
    {
        public string Message { get; }

        public MessageTarget MessageType { get; }

        public string OnFailedMessage { get; }

        public ClientCommand(string msg, MessageTarget msgType = MessageTarget.All, string onFailedMessage = null)
        {
            OnFailedMessage = onFailedMessage;
            Message = msg;
            MessageType = msgType;
        }
    }
}
