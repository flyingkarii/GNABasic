using GNABasic;

namespace GNA.Core.CommandSystem.Commands
{
    public class Blacklists : ICommand
    {
        [Permission(DefaultGroup.Everyone)]
        public void Execute(CommandContext ctx)
        {
            KeyValuePair<string, bool>[] values = ctx.Server.GetServerConfig().AllowedGadgets.Where(kv => kv.Value == false).ToArray();
            var dict = values.ToDictionary(kvp => kvp.Key);
            var gadgetsList = dict.Keys.ToList();

            KeyValuePair<string, bool>[] weapons = ctx.Server.GetServerConfig().AllowedWeapons.Where(kv => kv.Value == false).ToArray();
            var weaponsDict = values.ToDictionary(kvp => kvp.Key);
            var weaponsList = dict.Keys.ToList();

            string gadgetsBlacklisted = gadgetsList.Count > 0 ? Utils.Join(gadgetsList.ToArray(), ", ") : "None";
            string weaponsBlacklisted = weaponsList.Count > 0 ? Utils.Join(weaponsList.ToArray(), ", ") : "None";

            ctx.Executor.Message("<color=#ffa500>Blacklisted gadgets: </color>" + gadgetsBlacklisted + "\n\n<color=#ffa500>Blacklisted weapons: </color>" + weaponsBlacklisted);
        }
    }
}
