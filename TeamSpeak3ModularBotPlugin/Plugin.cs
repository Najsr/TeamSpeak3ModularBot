using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBotPlugin
{
    public class Plugin : IDisposable
    {
        private readonly Dictionary<string, object> _config;

        protected Plugin(QueryRunner queryRunner)
        {
            Ts3Instance = queryRunner;
            OnLoad();
            using (var sr = new StreamReader(GetConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());
            }
            if(_config == null)
                _config = new Dictionary<string, object>();
        }

        public virtual void Dispose()
        {

        }

        protected virtual void OnLoad()
        {

        }

        public virtual string Author { get; }

        protected QueryRunner Ts3Instance { get; }

        private string GetConfigPath
        {
            get
            {
                var file = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/plugins/configs/" +
                           GetType().Name +
                           ".json");
                file.Directory?.Create();
                if (!file.Exists)
                {
                    var handle = file.CreateText();
                    handle.Write("{ }");
                    handle.Close();
                }

                return file.FullName;
            }
        }

        protected object GetConfigValue(string key)
        {
            return _config.ContainsKey(key) ? _config[key] : null;
        }

        protected void SetConfigValue(string key, object value)
        {
            _config[key] = value;
            using (var sw = new StreamWriter(GetConfigPath))
                sw.Write(JsonConvert.SerializeObject(_config, Formatting.Indented));
        }
    }
}
