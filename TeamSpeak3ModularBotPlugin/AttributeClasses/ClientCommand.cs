using System;

namespace TeamSpeak3ModularBotPlugin.AttributeClasses
{
    [AttributeUsage(AttributeTargets.Method)]

    public class ClientCommand : Attribute
    {
        public string Message { get; }

        public MessageMode MessageType { get; }

        public string OnFailedMessage { get; }

        public ClientCommand(string msg, MessageMode msgType = MessageMode.All, string onFailedMessage = null)
        {
            OnFailedMessage = onFailedMessage;
            Message = msg;
            MessageType = msgType;
        }
    }

    [Flags]
    public enum MessageMode
    {
        Private = 0x1,
        Channel = 0x2,
        Server = 0x4,
        All = 0x7
    }
}
