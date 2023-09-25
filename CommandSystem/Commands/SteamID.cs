namespace GNA.Core.CommandSystem.Commands
{
    public class SteamID : ICommand
    {
        [Permission(DefaultGroup.Moderator)]
        public void Execute(CommandContext a)
        {
            if (a.Arguments.Length < 1)
            {
                a.ErrorReply("Input a player name!");
                return;
            }

            bool success = a.TryGetArgumentAsPlayer(0, out CustomPlayer player);

            if (success)
            {
                a.SuccessReply("Success!</color> Sent SteamID to chatlog webhook!");
                a.Server.GetWebhook().SendMessage($":star: Successfully found user: {player}");
            } else
            {
                a.ErrorReply("No player found with name " + a.Arguments[0]);
                return;
            }
        }
    }
}
