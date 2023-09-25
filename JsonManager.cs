using GNA.Core;
using Newtonsoft.Json;

namespace GNABasic
{
    public class JsonManager
    {
        string path = Program.CONFIG_FOLDER;

        public ServerConfig GetServerConfig(CustomServer server)
        {
            string filePath = path + $"\\{server}.json";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Writing config for " + server.ToString());
                try
                {
                    string jsonString = JsonConvert.SerializeObject(new ServerConfig(), Formatting.Indented);
                    File.WriteAllText(filePath, jsonString);
                } catch (Exception e)
                {
                    Console.WriteLine(e);

                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.InnerException);
                    }
                }
            }

            return OpenServerConfig(server);
        }

        public ServerConfig OpenServerConfig(CustomServer server)
        {
            using StreamReader reader = new(path + $"\\{server}.json");

            string json = reader.ReadToEnd();
            ServerConfig values = JsonConvert.DeserializeObject<ServerConfig>(json);

            return values;
        }
        public void SaveServerConfig(CustomServer server, ServerConfig serverConfig)
        {
            string filePath = path + $"\\{server}.json";
            string jsonString = JsonConvert.SerializeObject(serverConfig, Formatting.Indented);
            ServerConfig oldConfig = server.GetServerConfig();
            ServerConfig newConfig = null;

            try
            {
                File.WriteAllText(filePath, jsonString);
            } catch (Exception e)
            {
                newConfig = oldConfig;
                Console.WriteLine(e);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException);
                }
            } finally
            {
                if (newConfig == null)
                {
                    server.SetServerConfig();
                }
            }
        }

        public GlobalConfig GetGlobalConfig()
        {
    
            string filePath = path + "\\GlobalConfig.json";

            if (!File.Exists(filePath))
            {
                string jsonString = JsonConvert.SerializeObject(new GlobalConfig(), Formatting.Indented);
                File.WriteAllText(filePath, jsonString);
            }

            return OpenGlobalConfig();
        }

        private GlobalConfig OpenGlobalConfig()
        {
            using StreamReader reader = new(path + "\\GlobalConfig.json");

            string json = reader.ReadToEnd();
            GlobalConfig values = JsonConvert.DeserializeObject<GlobalConfig>(json);

            return values;
        }

        public void SaveGlobalConfig(GlobalConfig globalConfig)
        {
            string filePath = path + $"\\GlobalConfig.json";
            string jsonString = JsonConvert.SerializeObject(globalConfig, Formatting.Indented);
            GlobalConfig oldConfig = Program.GetGlobalConfig();
            GlobalConfig newConfig = null;

            try
            {
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception e)
            {
                newConfig = oldConfig;
                Console.WriteLine(e);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException);
                }
            }
            finally
            {
                if (newConfig == null)
                {
                    Program.SetGlobalConfig();
                }
            }
        }
    }

    public class GlobalConfig
    {
        public string LoadingText = "Welcome to <color=#ffa500>{serverName}</color>!\n<size=20>Join our discord with our redirect <color=#ffa500>gna.grimsyvr.com</color>!</size>";
        public string WelcomeText = "Welcome to <color=#ffa500>{serverName}</color>!\nJoin our discord with our redirect <color=#ffa500>gna.grimsyvr.com</color>!";

        public Dictionary<string, string> Staff = new()
        {
            {"76561198278722294", "admin"},
            {"76561198812647579", "admin"},
            {"76561198147837101", "admin" },
            {"76561198019461505", "admin" },
        };
        public Dictionary<string, string> Webhooks { get; set; } = new Dictionary<string, string>()
        {
            {"127.0.0.1-27050", "https://discord.com/api/webhooks/1146606537482195055/ZsWVzt6IrAn8a983YO4Rk3u6-7FusW-iD2j48ADcyWGskKocSWqGdtvgVPyXOGzXnh84"},
            {"127.0.0.1-27052", "https://discord.com/api/webhooks/1146606639399579688/KtK5Zt3XajS4PlTfU81sW_JBQK71WoGPTnqM9KMisKK7L6QmlzwuNqZCMq6Pb9xsIIz7"},
            {"127.0.0.1-27054", "https://discord.com/api/webhooks/1146606705581490287/hTF3ZteviODrETvRHOZB_xayI1GSRPgw-59wrDhlsQ8CGInOPiXZF7_hUmWiWZ2As9mS"},
            {"45.62.160.88-27056", "https://discord.com/api/webhooks/1147885992934653952/e-EA-HDHjKdw5eZ-Kf5uJhn4wxpZw9sz8Q90zThMuDyWsAGgcP2DezlOD0GOx0QB0VP4"},
        };

        public Dictionary<string, string> ReportWebhooks { get; set; } = new Dictionary<string, string>()
        {
            {"127.0.0.1-27050", "https://discord.com/api/webhooks/1147875646563942452/Rq6s6BGKI3lNv3PandZWb9ipnFR7DOhZpLso4vOCppwVt5FbJPQeBVpdOpJD1u2VofR7"},
            {"127.0.0.1-27052", "https://discord.com/api/webhooks/1147875728558403675/3RvGDUrbrqNoOw9uvfCxXf4HD2vJkeBFNh_zvaSgHFZ3p9Q1DgW1cC8minK3JUBag09Q"},
            {"127.0.0.1-27054", "https://discord.com/api/webhooks/1147875857155764309/2gNnE3VJR0t2BUCDfTSQ_BjiI3Of2FUjx5whhOzD-LmynYSEJsLRMKNWN9yhn49bcfac"},
            {"45.62.160.88-27056", "https://discord.com/api/webhooks/1147885992934653952/e-EA-HDHjKdw5eZ-Kf5uJhn4wxpZw9sz8Q90zThMuDyWsAGgcP2DezlOD0GOx0QB0VP4"},
        };

        public List<string> FilterBans = new();
    }

    public class ServerConfig
    {
        public Dictionary<string, bool> AllowedMaps = new();
        public Dictionary<string, bool> AllowedModes = new();
        public Dictionary<string, bool> AllowedWeapons = new();
        public Dictionary<string, bool> AllowedGadgets = new();

        public Dictionary<string, short> RoleLimits = new()
        {
            {"Leader", -1},
            {"Assault", -1},
            {"Medic", -1},
            {"Engineer", -1},
            {"Support", -1},
            {"Recon", -1},
        };

        public ServerSettings ServerSettings = new();
        public PlayerModifications PlayerModifications = new();
    }

    public class ServerSettings
    {
        public float DamageMultipler { get; set; } = 1.0f;
        public bool OnlyWinnerTeamCanVote { get; set; } = false;
        public bool PlayerCollision { get; set; } = false;
        public bool HideMapVotes { get; set; } = false;
        public bool CanVoteDay { get; set; } = true;
        public bool CanVoteNight { get; set; } = true;
        public byte MedicLimitPerSquad { get; set; } = 8;
        public byte EngineerLimitPerSquad { get; set; } = 8;
        public byte SupportLimitPerSquad { get; set; } = 8;
        public byte ReconLimitPerSquad { get; set; } = 8;
        public float TankSpawnDelayMultiplier { get; set; } = 1.0f;
        public float TransportSpawnDelayMultiplier { get; set; } = 1.0f;
        public float SeaVehicleSpawnDelayMultiplier { get; set; } = 1.0f;
        public float APCSpawnDelayMultiplier { get; set; } = 1.0f;
        public float HelicopterSpawnDelayMultiplier { get; set; } = 1.0f;
        public bool SquadRequiredToChangeRole { get; set; } = true;
    }

    public class PlayerModifications
    {
        public float RunningSpeedMultiplier = 1f;
        public float ReceiveDamageMultiplier = 1f;
        public float GiveDamageMultiplier = 1f;
        public float JumpHeightMultiplier = 1f;
        public float FallDamageMultiplier = 1f;
        public float ReloadSpeedMultiplier = 1f;
        public bool CanUseNightVision = true;
        public float DownTimeGiveUpTime = 60f;
        public bool AirStrafe = true;
        public bool CanDeploy = true;
        public bool CanSpectate = true;
        public float RespawnTime = 10f;
        public bool CanSuicide = true;
        public float MinDamageToStartBleeding = 10f;
        public float MinHpToStartBleeding = 40f;
        public float HPperBandage = 40f;
        public bool StaminaEnabled = false;
        public bool HitMarkersEnabled = true;
        public bool FriendlyHUDEnabled = true;
        public float CaptureFlagSpeedMultiplier = 1f;
        public bool PointLogHudEnabled = true;
        public bool KillFeed = false;
        public bool IsExposedOnMap = false;
        public bool Freeze = false;
        public float ReviveHP = 35f;
        public bool HideOnMap = false;
        public bool CanBleed = true;
    }
}
