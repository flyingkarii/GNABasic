using BattleBitAPI.Common;
using GNABasic;
using System.Collections.Generic;
using System.Linq;

namespace GNA.Core.CommandSystem.Commands
{
    public class Modes : ICommand
    {
        [Permission(DefaultGroup.Everyone)]
        public void Execute(CommandContext ctx)
        {
            KeyValuePair<string, bool>[] values = ctx.Server.GetServerConfig().AllowedModes.Where(kv => kv.Value == true).ToArray();
            var dict = values.ToDictionary(kvp => kvp.Key);
            var list = dict.Keys.ToList();

            ctx.Reply("Allowed modes: " + Utils.Join(list.ToArray(), ", "));
        }
    }
}
