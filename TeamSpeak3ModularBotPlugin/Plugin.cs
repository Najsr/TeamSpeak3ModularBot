using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBotPlugin
{
    public class Plugin : MarshalByRefObject, IDisposable
    {
        private readonly Dictionary<string, object> _config;

        private readonly FileInfo ConfigFile;

        public virtual string Author { get; }

        protected QueryRunner Ts3Instance { get; }

        protected Plugin(QueryRunner queryRunner)
        {
            Ts3Instance = queryRunner;
            ConfigFile = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/plugins/configs/" +
                           GetType().Name +
                           ".json");

            using (var sr = new StreamReader(GetConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd()) ?? new Dictionary<string, object>();
            }
        }

        public virtual void Dispose()
        {
            SaveConfig();
        }

        private string GetConfigPath
        {
            get
            {
                ConfigFile.Directory?.Create();
                if (!ConfigFile.Exists)
                {
                    var handle = ConfigFile.CreateText();
                    handle.Write("{ }");
                    handle.Close();
                }

                return ConfigFile.FullName;
            }
        }

        protected object GetConfigValue(string key)
        {
            return _config.ContainsKey(key) ? _config[key] : null;
        }

        protected bool KeyExists(string key)
        {
            return _config.ContainsKey(key);
        }

        protected void SetConfigValue(string key, object value, bool save = true)
        {
            _config[key] = value;
            if (save)
                SaveConfig();
        }

        protected void SaveConfig()
        {
            using (var sw = new StreamWriter(GetConfigPath))
                sw.Write(JsonConvert.SerializeObject(_config));
        }
    }
}
