using System;

namespace TeamSpeak3ModularBotPlugin.AttributeClasses
{
    [AttributeUsage(AttributeTargets.Method)]

    public class ServerGroups : Attribute
    {
        public string[] Groups { get; }

        public ServerGroups(params string[] groups)
        {
            Groups = groups;
        }
    }
}
