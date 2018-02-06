﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using Newtonsoft.Json;
using TeamSpeak3ModularBot.Config;

namespace TeamSpeak3ModularBot
{
    internal class Program
    {
        private static Ts3Config _config;

        private static TeamSpeak3Bot _teamSpeak3Bot;

        private static ConsoleEventDelegate _handler;

        private delegate bool ConsoleEventDelegate(int eventType);

	    private static Timer _timer;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static void Main()
        {
            _handler = ConsoleEventCallback;
            SetConsoleCtrlHandler(_handler, true);
			_timer = new Timer(1000);
			_timer.Elapsed += _timer_Elapsed;
	        _timer.Enabled = true;
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

		private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			GC.Collect();
			Console.Title = "TS3ModularBot (" + Process.GetCurrentProcess().WorkingSet64 / 1048576 + " MB)";
		}

		private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
                _teamSpeak3Bot.Dispose();
            return false;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled exception: {0}", e.ExceptionObject);
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
