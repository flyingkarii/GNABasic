using GNABasic;

namespace GNA.Core.CommandSystem.Commands
{
    public class Maps : ICommand
    {
        [Permission(DefaultGroup.Everyone)]
        public void Execute(CommandContext ctx)
        {
            KeyValuePair<string, bool>[] values = ctx.Server.GetServerConfig().AllowedMaps.Where(kv => kv.Value == true).ToArray();
            var dict = values.ToDictionary(kvp => kvp.Key);
            var list = dict.Keys.ToList();

            ctx.Reply("Allowed maps: " + Utils.Join(list.ToArray(), ", "));
        }
    }
}
