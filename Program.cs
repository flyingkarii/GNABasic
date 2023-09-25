using BattleBitAPI;
using GNA.Core.CommandSystem;
using BattleBitAPI.Common;
using BattleBitAPI.Discord;
using BattleBitAPI.Features;
using BattleBitAPI.Server;
using GNABasic;
using System.Net;
using System.Reflection;

namespace GNA.Core
{
    public class Program 
    {
        public static string CONFIG_FOLDER = Directory.GetCurrentDirectory();
        public static string LOG_FOLDER = Directory.GetCurrentDirectory();
        public static Dictionary<string, CustomServer> Servers = new();
        public static Dictionary<string, SkillLevel> TempStorage = new();

        private static JsonManager _manager;
        private static GlobalConfig _globalConfig;

        public static void Main(string[] args)
        {
            if (!Directory.Exists(CONFIG_FOLDER + "\\Config"))
                Directory.CreateDirectory(CONFIG_FOLDER + "\\Config");

            if (!Directory.Exists(CONFIG_FOLDER + "\\Log"))
                Directory.CreateDirectory(CONFIG_FOLDER + "\\Log");

            CONFIG_FOLDER += "\\Config";
            LOG_FOLDER += "\\Log";

            _manager = new JsonManager();
            _globalConfig = _manager.GetGlobalConfig();

            var listener = new ServerListener<CustomPlayer, CustomServer>();

            listener.OnCreatingGameServerInstance += OnCreatingGameServer;
            listener.OnGameServerConnecting += OnServerConnecting;
            listener.OnValidateGameServerToken += OnValidateToken;
            listener.OnGameServerDisconnected += OnGameServerDisconnected;
            listener.OnCreatingPlayerInstance += OnPlayerCreating;
            listener.LogLevel = LogLevel.Sockets | LogLevel.GameServerErrors | LogLevel.GameServers;
            listener.OnLog += OnLog;

            listener.Start(27049);
            Thread.Sleep(-1);
        }

        private static async Task OnGameServerDisconnected(GameServer<CustomPlayer> server)
        {
            OnLog(LogLevel.GameServers, $"Disconnect reason: {server.TerminationReason}", null);
        }

        private static void OnLog(LogLevel level, string arg2, object? arg3)
        {
            string LOG_NAME = LOG_FOLDER + $"\\{(DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day : DateTime.Now.Day)}-{(DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month)}-{(DateTime.Now.Year < 10 ? "0" + DateTime.Now.Year : DateTime.Now.Year)}.txt";
            string LOG_MESSAGE = $"({level}): {arg2}";
            Console.WriteLine(LOG_MESSAGE);

            if (!File.Exists(LOG_NAME))
            {
                File.WriteAllText(LOG_NAME, LOG_MESSAGE);
                return;
            }

            File.AppendAllLines(LOG_NAME, new string[] {LOG_MESSAGE});
        }

        private static CustomPlayer OnPlayerCreating(ulong steamId)
        {
            CustomPlayer player = new();

            Roles? role = Utils.GetServerRole(steamId);

            if (role == Roles.Admin)
                player.IsAdmin = true;

            if (role != null)
                player.StaffRole = (Roles) role;

            bool hasSkillLevel = TempStorage.TryGetValue(steamId.ToString(), out SkillLevel skillLevel);

            if (hasSkillLevel)
                player.SkillLevel = skillLevel;

            return player;
        }

        private static CustomServer OnCreatingGameServer(IPAddress address, ushort port)
        {
            CustomServer server = new CustomServer();

            return server;
        }

        private static async Task<bool> OnServerConnecting(IPAddress address)
        {
            if (address.ToString() != "127.0.0.1" && address.ToString() != "45.62.160.88")
                return false;

            return true;
        }

        private static async Task<bool> OnValidateToken(IPAddress address, ushort port, string token)
        {
            return token == "grimsyarcum<3";
        }

        public static GlobalConfig GetGlobalConfig()
        {
            return _globalConfig;
        }
        public static void SetGlobalConfig()
        {
            _globalConfig = _manager.GetGlobalConfig();
        }

        public static JsonManager GetJsonManager()
        {
            return _manager;
        }

        public static bool ReloadGlobalConfig()
        {
            bool success = true;
            GlobalConfig old = _globalConfig;

            try
            {
                _globalConfig = _manager.GetGlobalConfig();
            }
            catch (Exception e)
            {
                success = false;

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return success;
        }
    }

    public class CustomPlayer : Player<CustomPlayer>
    {
        public bool IsAdmin = false;
        public Roles StaffRole = Roles.None;
        public SkillLevel SkillLevel = SkillLevel.LOW;

        public void ErrorReply(string message)
        {
            SayToChat($"<color=#fa6666>[SERVER]</color> {message}");
        }

        public void SuccessReply(string message)
        {
            SayToChat($"<color=#66fa8e>[SERVER]</color> {message}");
        }

        public void Reply(string message)
        {
            SayToChat($"<color=#fa9f66>SERVER</color> {message}");
        }
    }

    public class CustomServer : GameServer<CustomPlayer>
    {
        private JsonManager _manager;
        private ServerConfig _serverConfig;
        private DiscordWebhooks webhooks;

        private TeamSkill teamASkill = new TeamSkill(Team.TeamA);
        private TeamSkill teamBSkill = new(Team.TeamB);

        public override async Task OnConnected()
        {
            _manager = new JsonManager();
            _serverConfig = _manager.GetServerConfig(this);

            Program.Servers.Add(ServerName, this);

            Console.Write("Ensuring restriction configs...");

            if (_serverConfig.AllowedGadgets.Count == 0)
            {
                PopulateGadgets();
                _manager.SaveServerConfig(this, _serverConfig);
            }

            if (_serverConfig.AllowedWeapons.Count == 0)
            {
                PopulateWeapons();
                _manager.SaveServerConfig(this, _serverConfig);
            }

            if (_serverConfig.AllowedMaps.Count == 0)
            {
                PopulateMaps();
                _manager.SaveServerConfig(this, _serverConfig);
            }

            if (_serverConfig.AllowedModes.Count == 0)
            {
                PopulateModes();
                _manager.SaveServerConfig(this, _serverConfig);
            }

            Console.Write(" OK");
            Console.WriteLine("");

            SetServerConnectValues();

            webhooks = new(this);
            webhooks.SendMessage($":white_check_mark: {ServerName} has connected.");
            Console.WriteLine("Checking map and mode rotation...");

            if (Utils.Maps.Count != _serverConfig.AllowedMaps.Count || _serverConfig.AllowedMaps.Where((keyValuePair, i) => keyValuePair.Value == false).ToArray().Length > 0)
            {
                List<KeyValuePair<string, bool>> keyValuePairs = _serverConfig.AllowedMaps.Where((keyValue) => keyValue.Value == true).ToList();
                Console.Write("Maps: ");

                foreach (KeyValuePair<string, bool> keyValue in keyValuePairs)
                {
                    MapRotation.AddToRotation(keyValue.Key);
                    Console.Write(keyValue.Key + $"{(keyValue.Key != keyValuePairs.Last().Key ? ", " : " ")}");
                }
            }

            Console.WriteLine("");

            if (Utils.Modes.Count != _serverConfig.AllowedModes.Count || _serverConfig.AllowedModes.Where((keyValuePair, i) => keyValuePair.Value == false).ToArray().Length > 0)
            {
                List<KeyValuePair<string, bool>> keyValuePairs = _serverConfig.AllowedModes.Where((keyValue) => keyValue.Value == true).ToList();
                Console.Write("Modes: ");

                foreach (KeyValuePair<string, bool> keyValue in keyValuePairs)
                {
                    GamemodeRotation.AddToRotation(keyValue.Key);
                    Console.Write(keyValue.Key + $"{(keyValue.Key != keyValuePairs.Last().Key ? ", " : " ")}");
                }
            }

            string result = new Placeholders(Program.GetGlobalConfig().LoadingText)
                .AddParam("serverName", ServerName)
                .AddParam("playerCount", AllPlayers.Count())
                .Run();

            SetLoadingScreenText(result);

            CommandHandler.RegisterCommands();
        }

        public override async Task OnDisconnected()
        {
            _serverConfig = null;
            _manager = null;

            webhooks.SendMessage($":x: {ServerName} has disconnected.");
            Program.Servers.Remove(ServerName);
        }

        public override async Task OnPlayerConnected(CustomPlayer player)
        {
            if (Filter.IsFiltered(player.Name))
            {
                Program.GetGlobalConfig().FilterBans.Add(player.SteamID.ToString());
                player.Kick("You should really know better than having a slur in your name. You are now banned permanently.");
                _manager.SaveGlobalConfig(Program.GetGlobalConfig());
            }

            SetPlayerConnectValues(player);

            string result = new Placeholders(Program.GetGlobalConfig().WelcomeText)
                .AddParam("playerName", player.Name)
                .AddParam("playerCount", AllPlayers.Count())
                .AddParam("serverName", ServerName)
                .Run();

            player.SayToChat(result);
        }

        public override async Task OnPlayerDisconnected(CustomPlayer player)
        {
            TeamSkill teamSkill = player.Team == Team.TeamA ? teamASkill : teamBSkill;
            teamSkill.Decrement(player.SkillLevel);
        }

        public override async Task<bool> OnPlayerTypedMessage(CustomPlayer player, ChatChannel channel, string msg)
        {
            if (Filter.IsFiltered(msg))
            {
                Program.GetGlobalConfig().FilterBans.Add(player.SteamID.ToString());
                player.Kick("You were banned for attempting to say a slur.");
                _manager.SaveGlobalConfig(Program.GetGlobalConfig());
                return false;
            }

            if (msg.StartsWith("!"))
            {
                string[] messageSplit = msg.Split(' ');
                string command = messageSplit[0].Substring(1);
                string[] arguments = messageSplit.Where((argument, index) => index != 0).ToArray();

                CommandContext ctx = new CommandContext();
                ctx.Server = this;
                ctx.Executor = player;
                ctx.Command = command.ToLower();
                ctx.Arguments = arguments;
                string result = CommandHandler.HandleCommand(ctx).Result;

                if (result != "Success")
                {
                    player.ErrorReply(result);
                }

                return false;
            }

            webhooks.SendMessage($":speech_balloon: {player} ({channel}): {msg}");
            return true;
        }

        public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(CustomPlayer player, OnPlayerSpawnArguments request)
        {
            Dictionary<string, bool> AllowedWeapons = _serverConfig.AllowedWeapons;
            Dictionary<string, bool> AllowedGadgets = _serverConfig.AllowedGadgets;

            AllowedWeapons.TryGetValue(request.Loadout.PrimaryWeapon.ToolName, out bool primaryAllowed);
            AllowedWeapons.TryGetValue(request.Loadout.SecondaryWeapon.ToolName, out bool secondaryAllowed);

            AllowedGadgets.TryGetValue(request.Loadout.HeavyGadgetName, out bool heavyAllowed);
            AllowedGadgets.TryGetValue(request.Loadout.LightGadgetName, out bool lightAllowed);
            AllowedGadgets.TryGetValue(request.Loadout.ThrowableName, out bool throwableAllowed);

            string format = "Your <color=#ffa500>{0}</color> is blacklisted from use.";

            if (!primaryAllowed)
            {
                player.SayToChat(string.Format(format, request.Loadout.PrimaryWeapon.ToolName));
                return null;
            }

            if (!secondaryAllowed)
            {
                player.SayToChat(string.Format(format, request.Loadout.SecondaryWeapon.ToolName));
                return null;
            }

            if (!heavyAllowed)
            {
                player.SayToChat(string.Format(format, request.Loadout.HeavyGadgetName));
                return null;
            }

            if (!lightAllowed)
            {
                player.SayToChat(string.Format(format, request.Loadout.LightGadgetName));
                return null;
            }

            if (!throwableAllowed)
            {
                player.SayToChat(string.Format(format, request.Loadout.ThrowableName));
                return null;
            }

            return request;
        }

        public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
        {
            if (Program.GetGlobalConfig().FilterBans.Contains(steamID.ToString()))
            {
                args.Stats.IsBanned = true;
            }

            Roles? role = Utils.GetServerRole(steamID);

            if (role != null)
                args.Stats.Roles = (Roles) role;

            uint TotalLevel = (args.Stats.Progress.Prestige * 200) + args.Stats.Progress.Rank;
            SkillLevel skillLevel = SkillLevel.LOW;

            if (TotalLevel >= 51)
                skillLevel = SkillLevel.MEDIUM;
            else if (TotalLevel >= 200)
                skillLevel = SkillLevel.HIGH;
            else if (TotalLevel >= 600)
                skillLevel = SkillLevel.INSANE;

            Program.TempStorage.Add(steamID.ToString(), skillLevel);
            args.Team = teamASkill.GetCount(skillLevel) >= teamBSkill.GetCount(skillLevel) ? Team.TeamB : Team.TeamA;

            TeamSkill teamSkill = args.Team == Team.TeamA ? teamASkill : teamBSkill;
            teamSkill.Increment(skillLevel);
        }

        public override async Task<bool> OnPlayerRequestingToChangeRole(CustomPlayer player, GameRole role)
        {
            string roleName = role.ToString();

            if (roleName == null)
                roleName = "Recon";

            bool success = _serverConfig.RoleLimits.TryGetValue(roleName, out short max);
            int count = Utils.GetPlayerCountInRoleOnTeam(this, player.Team, role);

            if (success && count >= max && max != -1)
            {
                player.SayToChat($"Limit of <color=#ffa500>{role}s</color> reached.");
                return false;
            }

            return true;
        }

        public override async Task OnPlayerReported(CustomPlayer from, CustomPlayer to, ReportReason reason, string additional)
        {
            DiscordWebhooks webhooks = new DiscordWebhooks(this);
            webhooks.SendReportMessage($":imp: ``{from}`` reported ``{to}`` for {reason}. Additional info: ``{additional}``");
        }

        public ServerConfig GetServerConfig()
        {
            return _serverConfig;
        }

        public void SetServerConfig()
        {
            _serverConfig = _manager.GetServerConfig(this);
        }

        public bool ReloadServerConfig()
        {
            bool success = true;
            ServerConfig old = _serverConfig;

            try
            {
                _serverConfig = _manager.GetServerConfig(this);
                SetServerConnectValues();

                int index = 0;
                CustomPlayer currentPlayer = null;

                do
                {
                    currentPlayer = AllPlayers.ElementAt(index);
                    SetPlayerConnectValues(currentPlayer);
                    index++;
                } while (index < AllPlayers.Count());
            }
            catch (Exception e)
            {
                success = false;

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return success;
        }

        public void PopulateGadgets()
        {
            var members = typeof(Gadgets).GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)memberInfo;
                    if (field.FieldType == typeof(Gadget))
                    {
                        var gadget = (Gadget)field.GetValue(null);
                        _serverConfig.AllowedGadgets.Add(gadget.Name, true);
                    }
                }
            }

            _serverConfig.AllowedGadgets.Add("AntiGrenadeTrophy", true);
        }

        public void PopulateWeapons()
        {
            var members = typeof(Weapons).GetMembers(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)memberInfo;

                    if (field.FieldType == typeof(Weapon))
                    {
                        var weapon = (Weapon)field.GetValue(null);
                        _serverConfig.AllowedWeapons.Add(weapon.Name, true);
                    }
                }
            }

            _serverConfig.AllowedWeapons.Add("G3", true);
            _serverConfig.AllowedWeapons.Add("F2000", true);
        }

        public void PopulateMaps()
        {
            foreach (KeyValuePair<string, bool> entry in Utils.Maps)
            {
                if (entry.Value)
                {
                    _serverConfig.AllowedMaps.Add(entry.Key, entry.Value);
                }
            }
        }

        public void PopulateModes()
        {
            foreach (KeyValuePair<string, bool> entry in Utils.Modes)
            {
                if (entry.Value)
                {
                    _serverConfig.AllowedModes.Add(entry.Key, entry.Value);
                }
            }
        }

        public SkillLevel GetSkillLevel(CustomPlayer player)
        {
            return player.SkillLevel;
        }

        public DiscordWebhooks GetWebhook()
        {
            return webhooks;
        }

        private void SetServerConnectValues()
        {
            try
            {
                ServerSettings.CanVoteDay = _serverConfig.ServerSettings.CanVoteDay;
                ServerSettings.CanVoteNight = _serverConfig.ServerSettings.CanVoteNight;
                ServerSettings.DamageMultiplier = _serverConfig.ServerSettings.DamageMultipler;
                ServerSettings.OnlyWinnerTeamCanVote = _serverConfig.ServerSettings.OnlyWinnerTeamCanVote;
                ServerSettings.PlayerCollision = _serverConfig.ServerSettings.PlayerCollision;
                ServerSettings.HideMapVotes = _serverConfig.ServerSettings.HideMapVotes;
                ServerSettings.CanVoteDay = _serverConfig.ServerSettings.CanVoteDay;
                ServerSettings.ReconLimitPerSquad = _serverConfig.ServerSettings.ReconLimitPerSquad;
                ServerSettings.EngineerLimitPerSquad = _serverConfig.ServerSettings.EngineerLimitPerSquad;
                ServerSettings.SupportLimitPerSquad = _serverConfig.ServerSettings.SupportLimitPerSquad;
                ServerSettings.MedicLimitPerSquad = _serverConfig.ServerSettings.MedicLimitPerSquad;
                ServerSettings.APCSpawnDelayMultipler = _serverConfig.ServerSettings.APCSpawnDelayMultiplier;
                ServerSettings.TankSpawnDelayMultipler = _serverConfig.ServerSettings.TankSpawnDelayMultiplier;
                ServerSettings.TransportSpawnDelayMultipler = _serverConfig.ServerSettings.TransportSpawnDelayMultiplier;
                ServerSettings.SeaVehicleSpawnDelayMultipler = _serverConfig.ServerSettings.SeaVehicleSpawnDelayMultiplier;
                ServerSettings.HelicopterSpawnDelayMultipler = _serverConfig.ServerSettings.HelicopterSpawnDelayMultiplier;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private void SetPlayerConnectValues(CustomPlayer player)
        {
            try
            {
                player.Modifications.AirStrafe = _serverConfig.PlayerModifications.AirStrafe;
                player.Modifications.HpPerBandage = _serverConfig.PlayerModifications.HPperBandage;
                player.Modifications.RespawnTime = _serverConfig.PlayerModifications.RespawnTime;
                player.Modifications.ReviveHP = _serverConfig.PlayerModifications.ReviveHP;
                player.Modifications.ReloadSpeedMultiplier = _serverConfig.PlayerModifications.ReloadSpeedMultiplier;
                player.Modifications.MinimumHpToStartBleeding = _serverConfig.PlayerModifications.MinHpToStartBleeding;
                player.Modifications.MinimumDamageToStartBleeding = _serverConfig.PlayerModifications.MinDamageToStartBleeding;
                player.Modifications.CanSuicide = _serverConfig.PlayerModifications.CanSuicide;
                player.Modifications.CanSpectate = _serverConfig.PlayerModifications.CanSpectate;
                player.Modifications.StaminaEnabled = _serverConfig.PlayerModifications.StaminaEnabled;
                player.Modifications.PointLogHudEnabled = _serverConfig.PlayerModifications.PointLogHudEnabled;
                player.Modifications.KillFeed = _serverConfig.PlayerModifications.KillFeed;
                player.Modifications.CanDeploy = _serverConfig.PlayerModifications.CanDeploy;
                player.Modifications.CanUseNightVision = _serverConfig.PlayerModifications.CanUseNightVision;
                player.Modifications.FriendlyHUDEnabled = _serverConfig.PlayerModifications.FriendlyHUDEnabled;
                player.Modifications.JumpHeightMultiplier = _serverConfig.PlayerModifications.JumpHeightMultiplier;
                player.Modifications.RunningSpeedMultiplier = _serverConfig.PlayerModifications.RunningSpeedMultiplier;
                player.Modifications.GiveDamageMultiplier = _serverConfig.PlayerModifications.GiveDamageMultiplier;
                player.Modifications.ReceiveDamageMultiplier = _serverConfig.PlayerModifications.ReceiveDamageMultiplier;
                player.Modifications.IsExposedOnMap = _serverConfig.PlayerModifications.IsExposedOnMap;
                player.Modifications.HitMarkersEnabled = _serverConfig.PlayerModifications.HitMarkersEnabled;
                player.Modifications.CaptureFlagSpeedMultiplier = _serverConfig.PlayerModifications.CaptureFlagSpeedMultiplier;
                player.Modifications.FallDamageMultiplier = _serverConfig.PlayerModifications.FallDamageMultiplier;

                if (!_serverConfig.PlayerModifications.CanSpectate)
                {
                    if (player.IsAdmin)
                    {
                        player.Modifications.CanSpectate = true;
                    }
                }

                if (!_serverConfig.PlayerModifications.CanBleed)
                    player.Modifications.DisableBleeding();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public override string ToString()
        {
            return GameIP + "-" + GamePort;
        }
    }

    public class TeamSkill
    {
        public Team team;
        public Dictionary<SkillLevel, int> skillCount;

        public TeamSkill(Team team)
        {
            this.team = team;

            skillCount = new()
            {
                {SkillLevel.LOW, 0},
                {SkillLevel.MEDIUM, 0},
                {SkillLevel.HIGH, 0},
                {SkillLevel.INSANE, 0},
            };
        }

        public void Increment(SkillLevel skillLevel)
        {
            skillCount[skillLevel] += 1;
        }

        public void Decrement(SkillLevel skillLevel)
        {
            skillCount[skillLevel] -= 1;
        }

        public int GetCount(SkillLevel skillLevel)
        {
            return skillCount[skillLevel];
        }

        public void Reset()
        {
            skillCount[SkillLevel.LOW] = 0;
            skillCount[SkillLevel.MEDIUM] = 0;
            skillCount[SkillLevel.HIGH] = 0;
            skillCount[SkillLevel.INSANE] = 0;
        }
    }
}