using BattleBitAPI.Common;
using GNA.Core;

namespace GNABasic
{
    public class Utils
    {
        public static Dictionary<string, bool> Maps = new()
        {
            {"Azagor", true},
            {"Basra", true},
            {"Construction", true},
            {"District", true},
            {"DustyDew", true},
            {"Eduardovo", true},
            {"Frugis", true},
            {"Isle", true},
            {"Lonovo", true},
            {"MultuIslands", true},
            {"Namak", true},
            {"OilDunes", true},
            {"River", true},
            {"Salhan", true},
            {"SandySunset", true},
            {"TensaTown", true},
            {"Valley", true},
            {"Wakistan", true},
            {"WineParadise", true},
            {"Old_District", false},
            {"Old_Eduardovo", false},
            {"Old_MultuIslands", false},
            {"Old_Namak", false},
            {"Old_OilDunes", false},
        };

        public static Dictionary<string, bool> Modes = new()
        {
            {"CONQ", true},
            {"INFCONQ", true},
            {"DOMI", true},
            {"RUSH", true},
            {"CTF", true},
            {"FRONTLINE", true},
        };

        public static Dictionary<SkillLevel, short> SkillBased = new()
        {
            {SkillLevel.LOW, 0},
            {SkillLevel.MEDIUM, 0},
            {SkillLevel.HIGH, 0},
            {SkillLevel.INSANE, 0},
        };

        public static short GetPlayerCountInRoleOnTeam(CustomServer server, Team team, GameRole role)
        {
            int index = 0;
            short count = 0;
            IEnumerable<CustomPlayer> teamPlayers = team == Team.TeamA ? server.AllTeamAPlayers : server.AllTeamBPlayers;

            do
            {
                CustomPlayer player = teamPlayers.ElementAt(index);

                if (player.Role == role)
                    count++;
                index++;
            } while (index < teamPlayers.Count());

            return count;
        }

        public static List<CustomPlayer> GetPlayersInTeam(CustomServer server, Team team)
        {
            return team == Team.TeamA ? server.AllTeamAPlayers.ToList() : server.AllTeamBPlayers.ToList();
        }

        public static Roles? GetServerRole( ulong steamid)
        {
            Roles? staffRole = null;
            bool success = Program.GetGlobalConfig().Staff.TryGetValue(steamid.ToString(), out string roleName);

            if (success)
                switch (roleName)
                {
                    case "owner":
                    case "admin":
                        staffRole = Roles.Admin;
                        break;
                    case "moderator":
                    case "mod":
                        staffRole = Roles.Moderator;
                        break;
                    case "vip":
                        staffRole = Roles.Vip;
                        break;
                    case "special":
                        staffRole = Roles.Special;
                        break;
                }

            return staffRole;
        }

        public static string Join(string[] strings, string separator)
        {
            string result = "";

            for (int i = 0; i < strings.Length; i++)
            {
                if (i == strings.Length - 1)
                    separator = "";

                string value = strings[i];
                result += value + separator;
            }

            return result;
        }

        public static string Join(string[] strings, string separator, int fromIndex)
        {
            string result = "";

            for (int i = 0; i < strings.Length; i++)
            {

                if (i >= fromIndex)
                {
                    if (i == strings.Length - 1)
                        separator = "";

                    string value = strings[i];
                    result += value + separator;
                }
            }

            return result;
        }
    }

    public enum SkillLevel
    {
        LOW,
        MEDIUM,
        HIGH,
        INSANE
    }
}
