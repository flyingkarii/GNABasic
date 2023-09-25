namespace GNA.Core.CommandSystem.Commands
{
    public class GlobalMessage : ICommand
    {
        [Permission(DefaultGroup.Owner)]
        public void Execute(CommandContext arguments)
        {
            string result = string.Join<string>(" ", arguments.Arguments.ToList());
            arguments.Executor.SayToChat("Success!");

            try
            {
                foreach (CustomServer server in Program.Servers.Values)
                {
                    server.SayToAllChat(result);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
