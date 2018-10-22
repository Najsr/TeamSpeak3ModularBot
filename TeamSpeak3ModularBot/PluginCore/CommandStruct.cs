using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    public sealed class CommandStruct : IDisposable
    {
        private QueryRunner Ts3Instance { get; }

        public Plugin Class { get; }

        private MethodInfo Method { get; }

        public ClientCommand Command { get; }

        public ServerGroups ServerGroups { get; }

        public void Dispose()
        {

        }

        ~CommandStruct()
        {
            Dispose();
        }

        public CommandStruct(QueryRunner instance, Plugin class_, MethodInfo methodName, ClientCommand command, ServerGroups serverGroups = null)
        {
            Ts3Instance = instance;
            Class = class_;
            Method = methodName;
            Command = command;
            ServerGroups = serverGroups;
        }

        internal void Invoke(MessageReceivedEventArgs eArgs, string[] inputStrings, MessageMode mode)
        {
            var parameters = Method.GetParameters();
            var stringCount = 0;
            var injectedParameters = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;
                if (type == typeof(MessageReceivedEventArgs))
                    injectedParameters[i] = eArgs;
                else if (type == typeof(string[]))
                    injectedParameters[i] = inputStrings;
                else if (type == typeof(List<string>))
                    injectedParameters[i] = inputStrings.ToList();
                else if (type == typeof(uint))
                    injectedParameters[i] = eArgs.InvokerClientId;
                else if (type == typeof(PluginManager))
                    injectedParameters[i] = this;
                else if (type == typeof(MessageTarget))
                {
                    switch (mode)
                    {
                        case MessageMode.Private:
                            injectedParameters[i] = MessageTarget.Client;
                            break;
                        case MessageMode.Server:
                            injectedParameters[i] = MessageTarget.Server;
                            break;
                        case MessageMode.Channel:
                            injectedParameters[i] = MessageTarget.Channel;
                            break;
                    }
                }
                else if (type == typeof(string))
                {
                    var canBeNull = parameter.HasDefaultValue && parameter.DefaultValue == null;
                    switch (parameter.Name.ToLower())
                    {
                        case "uniqueid":
                            injectedParameters[i] = eArgs.InvokerUniqueId;
                            continue;
                        case "clientnickname":
                            injectedParameters[i] = eArgs.InvokerNickname;
                            continue;
                    }
                    if (inputStrings.Length > stringCount)
                    {
                        injectedParameters[i] = inputStrings[stringCount];
                        stringCount++;
                    }
                    else
                    {
                        if (canBeNull)
                            injectedParameters[i] = null;
                        else
                        {
                            if (string.IsNullOrWhiteSpace(Command.OnFailedMessage))
                                return;
                            var output = Command.OnFailedMessage;
                            FormatString(ref output);
                            switch (mode)
                            {
                                case MessageMode.Private:
                                    Ts3Instance.SendTextMessage(MessageTarget.Client, eArgs.InvokerClientId, output);
                                    break;
                                case MessageMode.Server:
                                    Ts3Instance.SendTextMessage(MessageTarget.Server, 0, output);
                                    break;
                                case MessageMode.Channel:
                                    Ts3Instance.SendTextMessage(MessageTarget.Channel, 0, output);
                                    break;
                            }
                            return;
                        }
                    }
                }
                else
                    throw new Exception("Unknown parameter type");
            }
            Method.Invoke(Class, injectedParameters);
        }

        private void FormatString(ref string formattedString)
        {
            formattedString = formattedString.Replace("{command}", "!" + Command.Message);
        }
    }
}
