using System;

namespace TeamSpeak3ModularBotPlugin.Helper
{
    [AttributeUsage(AttributeTargets.Method)]

    public class ClientCommand : Attribute
    {
        public string Message { get; }

        public MessageMode MessageType { get; }

        public int[] Groups { get; }

        public ClientCommand(string msg, MessageMode msgType, int group = -1)
        {
            Message = msg;
            MessageType = msgType;
            Groups = @group == -1 ? new int[0] : new[] { @group };
        }

        public ClientCommand(string msg, MessageMode msgType, int[] groups)
        {
            Message = msg;
            MessageType = msgType;
            Groups = groups;
        }

        [Flags]
        public enum MessageMode
        {
            Private = 0x1,
            Channel = 0x2,
            Server = 0x4
        }
    }
}
