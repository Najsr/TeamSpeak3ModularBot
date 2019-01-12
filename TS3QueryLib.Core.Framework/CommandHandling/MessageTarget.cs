using System;

namespace TS3QueryLib.Core.CommandHandling
{
    [Flags]
    public enum MessageTarget
    {
        Client = 0x1,
        Channel = 0x2,
        Server = 0x4,
        All = 0x7
    }
}