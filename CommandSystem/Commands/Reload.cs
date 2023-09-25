namespace GNA.Core.CommandSystem.Commands
{
    public class Reload : ICommand
    { 
        [Permission(DefaultGroup.Admin)]
        public void Execute(CommandContext ctx)
        {
            if (ctx.Arguments.Length > 0)
            {
                switch (ctx.Arguments.First().ToLower())
                {
                    case "all":
                        foreach (CustomServer server in Program.Servers.Values)
                        {
                            if (!server.IsConnected)
                                return;

                            bool serverReloaded = server.ReloadServerConfig();

                            if (serverReloaded)
                            {
                                ctx.SuccessReply("Successfully reloaded configuration file for server " + ctx.Server.ServerName);
                            } else
                            {
                                ctx.ErrorReply("Failed to reload server config.");
                            }
                        }

                        break;
                    case "global":
                        bool globalReloaded = Program.ReloadGlobalConfig();

                        if (globalReloaded)
                        {
                            ctx.SuccessReply("Successfully reloaded global config.");
                        } else
                        {
                            ctx.ErrorReply("Failed to reload global config.");
                        }

                        break;
                    default:
                        ctx.ErrorReply("Usage: !reload [global/all]");
                        break;
                }
            } else
            {
                bool success = ctx.Server.ReloadServerConfig();

                if (success) 
                    ctx.SuccessReply("Successfully reloaded configuration file for server " + ctx.Server.ServerName);
                else
                    ctx.ErrorReply("Failed to reload server config.");
            }
        }
    }
}
