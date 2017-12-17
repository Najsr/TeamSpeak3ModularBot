using System;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBotPlugin
{
    public interface IPlugin : IDisposable
    {
        void OnLoad();

        string Author { get; }

        Version Version { get; }

        QueryRunner Ts3Instance { get; set; }
    }
}
