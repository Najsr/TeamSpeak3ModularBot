using System;

namespace TeamSpeak3ModularBotPlugin.Helper
{
    [AttributeUsage(AttributeTargets.Method)]

    public class ClientCommand : Attribute
    {
        public string Message { get; }

        public MessageMode MessageType { get; }

        public ClientCommand(string msg, MessageMode msgType = MessageMode.All)
        {
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
