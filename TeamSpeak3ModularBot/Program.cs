using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using TeamSpeak3ModularBot.Config;

namespace TeamSpeak3ModularBot
{
    internal class Program
    {
        private static Ts3Config _config;

        private static TeamSpeak3Bot _teamSpeak3Bot;

        private static void Main()
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            Console.SetOut(new PrefixedWriter());
            Console.Title = "TS3ModularBot";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                _config = JsonConvert.DeserializeObject<Ts3Config>(GetConfig());
            } catch
            {
                Console.WriteLine("Config file is corrupted / has not been found!");
                Console.ReadLine();
                Environment.Exit(0);
            }

            _teamSpeak3Bot = new TeamSpeak3Bot(_config);
            _teamSpeak3Bot.Run();

            while (_teamSpeak3Bot.Connected)
            {
                Console.ReadKey(true);
            }

            _teamSpeak3Bot = null;
            Console.ReadLine();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _teamSpeak3Bot.Dispose();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled exception: {0}", e.ExceptionObject.ToString());
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static string GetConfig()
        {
            using (var sr = new StreamReader("config.json"))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
