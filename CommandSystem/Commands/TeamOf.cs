using BattleBitAPI.Common;

namespace GNA.Core.CommandSystem.Commands
{
    public class TeamOf : ICommand 
    {
        [Permission(DefaultGroup.Moderator)]
        public void Execute(CommandContext arguments)
        {
            if (arguments.Arguments.Length < 1)
            {
                arguments.ErrorReply("Input a player name.");
                return;
            }

            if (!arguments.TryGetArgumentAsPlayer(0, out CustomPlayer player))
            {
                arguments.ErrorReply("No player found.");
                return;
            }

            string team = player.Team == Team.TeamA ? "Team A (USA)" : "Team B (RU)";
            string senderTeam = arguments.Executor.Team == Team.TeamA ? "Team A (USA)" : "Team B (RU)";
            arguments.SuccessReply("Player is on </color>" + team);
            arguments.SuccessReply("You are on </color>" + senderTeam);
        }
    }
}
