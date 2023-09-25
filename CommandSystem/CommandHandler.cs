using System.Reflection;
using System.Data;
using GNABasic;

namespace GNA.Core.CommandSystem
{
    public class CommandHandler
    {
        private static Dictionary<string, Type> commandClasses = new Dictionary<string, Type>();

        public static void RegisterCommands()
        {
            try
            {
                Type[] types = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "GNA.Core.CommandSystem.Commands");

                foreach (Type commandClass in types)
                {
                    if (commandClasses.Where((n) => n.Key.ToLower() == commandClass.Name.ToLower()).ToArray().Length > 0 || commandClass.Name.StartsWith("<>"))
                        continue;

                    Console.WriteLine($"{commandClass.Name.ToLower()} has been registered.");

                    MethodInfo method = commandClass.GetMethod("Execute");
                    IEnumerable<Alias> aliases = method.GetCustomAttributes<Alias>();

                    if (aliases.ToList().Count != 0)
                    {
                        foreach (Alias alias in aliases)
                        {
                            commandClasses.Add(alias.alias, commandClass);
                        }
                    }

                    commandClasses.Add(commandClass.Name.ToLower(), commandClass);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.InnerException != null ? e.InnerException.Message : "No innerexception found.");
                Console.WriteLine(e.InnerException != null ? e.InnerException.StackTrace : "No innerexception found.");      
            }
        }

        public static async Task<string> HandleCommand(CommandContext arguments)
        {
            if (!commandClasses.TryGetValue(arguments.Command, out Type commandClass))
                return "Command does not exist.";

            Type commandType = Type.GetType("GNA.Core.CommandSystem.Commands." + commandClass.Name);
            MethodInfo method = commandType.GetMethod("Execute");

            Permission? attribute = method.GetCustomAttribute<Permission>();

            if (attribute != null && attribute.defaultAccess != null && !PermissionValidator.HasPermission(arguments.Server, arguments.Executor, attribute.defaultAccess).Result)
            {
                return "You do not have the required role for this command.";
            }

            try {
                object obj = Activator.CreateInstance(commandType);
                try
                {
                    method.Invoke(obj, new object[] { arguments });
                } catch (TargetInvocationException e)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                    Console.WriteLine(e.InnerException.Source);
                } finally
                {
                    string argumentsStr = arguments.Arguments.Length > 0 ? $" with arguments ``{Utils.Join(arguments.Arguments, " ")}``." : ".";
                    arguments.Server.GetWebhook().SendMessage($":tada: {arguments.Executor.Name} executed command ``{arguments.Command}``{argumentsStr}", "https://discord.com/api/webhooks/1147704517278830623/e5DdQcqAaVZ4MgzX-3saZQ_3lCQgqfqcRWT6OKUQKYUtaYwOo4spkktF9EG8SXtoEls6");
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Source);
            }

            return "Success";
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
    }
}
