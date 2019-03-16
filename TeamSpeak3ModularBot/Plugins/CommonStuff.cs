using System;
using System.Linq;
using System.Text.RegularExpressions;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class CommonStuff : AdminPlugin
    {
        private PluginManager Manager { get; }
        public CommonStuff(QueryRunner queryRunner, PluginManager manager) : base(queryRunner, manager)
        {
            Manager = manager;
        }

        [ClientCommand("commands", "Simple list of available commands to you")]
        public void ListPlugins(string uniqueId, uint clId, MessageTarget target)
        {
            var clientDatabaseId = Ts3Instance
                .GetClientNameAndDatabaseIdByUniqueId(uniqueId).ClientDatabaseId;
            if (clientDatabaseId != null)
            {
                var groups = Ts3Instance.GetServerGroupsByClientId((uint)clientDatabaseId).Select(x => x.Name);
                var commands = PluginManager.CommandList.Where(x =>
                {
                    if (x.ServerGroups == null)
                        return x.ServerGroups == null;
                    return (x.ServerGroups?.Groups.Intersect(groups).Any()).Value;
                }).ToArray();
                if (commands.Any())
                    Ts3Instance.SendTextMessage(target, clId, "Available commands: " + string.Join(", ", commands.OrderBy(x => x.Command.CommandName).Select(x => "!" + x.Command.CommandName)));
                else
                    Ts3Instance.SendTextMessage(target, clId, "You have no commands available!");
            }
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("rename", "Renames the bot")]
        public void Rename(uint clId, string name, MessageTarget target)
        {
            var response = Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = name });
            if (!response.IsErroneous)
                Ts3Instance.SendTextMessage(target, clId, $"Changed name to {name}");
        }

        [ClientCommand("info", "Prints out info about a command(s)", MessageTarget.Client)]
        public void Info(uint clId, MessageTarget target, MessageReceivedEventArgs eventArgs, string command)
        {
            var clientDatabaseId = Ts3Instance
                .GetClientNameAndDatabaseIdByUniqueId(eventArgs.InvokerUniqueId).ClientDatabaseId;
            if (!clientDatabaseId.HasValue)
            {
                Ts3Instance.SendTextMessage(target, clId, "Error, please contact administrator!");
                return;
            }
            var serverGroups = Ts3Instance.GetServerGroupsByClientId((uint)clientDatabaseId).Select(x => x.Name);
            var commands = Manager.CommandList.Where(x => x.Command.CommandName.StartsWith(command, StringComparison.OrdinalIgnoreCase)
                                                          && (x.ServerGroups.Groups.Intersect(serverGroups).Any() || !x.ServerGroups.Groups.Any())).ToArray();
            if (commands.Length > 0)
            {
                var toWrite = "Available commands:" + Environment.NewLine;
                foreach (var methodInfo in commands.Select(x => x.Method))
                {
                    var clientCommands = (ClientCommand[])methodInfo.GetCustomAttributes(typeof(ClientCommand), false);
                    var parameters = methodInfo.GetParameters().Where(x => x.ParameterType == typeof(string)).ToArray();
                    var stringParameters = string.Empty;
                    foreach (var parameterInfo in parameters)
                    {
                        if (parameterInfo.HasDefaultValue && parameterInfo.DefaultValue != null)
                        {
                            var regex = Regex.Match((string)parameterInfo.DefaultValue, "^(?:{(?=.+})(?<name>.+)})?(?<parameters>.*)?",
                                RegexOptions.IgnoreCase);
                            if (regex.Groups["name"].Value != string.Empty)
                                stringParameters += regex.Groups["name"].Value;
                            else if (regex.Groups["parameters"].Value != string.Empty)
                            {
                                var allowedValues = new[] { regex.Groups["parameters"].Value };
                                if (allowedValues[0].Contains('|'))
                                    allowedValues = allowedValues[0].Split('|');
                                stringParameters += "< " + string.Join(" / ", allowedValues) + " >";
                            }
                            else
                                stringParameters += parameterInfo.Name;
                        }
                        if (!parameterInfo.HasDefaultValue && parameterInfo.Name != "uniqueid" && parameterInfo.Name != "clientnickname")
                            stringParameters += parameterInfo.Name;
                        stringParameters += " ";
                    }
                    if (parameters.Length == 0)
                        stringParameters += " ";
                    var descriptions = clientCommands.Select(x => x.CommandInfo).Where(x => x != null).ToArray();
                    if (descriptions.Length < 2)
                        toWrite += $"{string.Join(", ", clientCommands.Select(x => "[B]!" + x.CommandName + "[/B]"))} " +
                                   $"{stringParameters}" +
                                   $"- {(descriptions.Length == 1 ? descriptions[0] : "[I]Without description[/I]")}{Environment.NewLine}";
                    else
                    {
                        toWrite += $"{string.Join(", ", clientCommands.Select(x => "[B]!" + x.CommandName + "[/B]"))} " +
                                   $"{stringParameters}" +
                                   $"- {string.Join(", ", descriptions)}{Environment.NewLine}";
                    }
                }
                Ts3Instance.SendTextMessage(target, clId, toWrite);
            }
            else
            {
                Ts3Instance.SendTextMessage(target, clId, $"No commands found starting with \"{command}\"");
            }
        }
    }
}
