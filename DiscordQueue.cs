using BattleBitAPI.Common;
using GNA.Core;
using System.Text;
using System.Text.Json;

namespace BattleBitAPI.Discord
{
    public class DiscordWebhooks
    {
        private CustomServer Server;
        private Queue<DiscordMessage> discordMessageQueue = new();
        private HttpClient httpClient = new HttpClient();

        public DiscordWebhooks(CustomServer server)
        {
            Server = server;
        }

        public void SendMessage(string message, string webhookURL)
        {
            if (webhookURL is not null)
            {
                Task.Run(() => sendWebhookMessage(webhookURL, message));
            }
            else
            {
                discordMessageQueue.Enqueue(new RawTextMessage(message));
            }
        }

        public void SendMessage(string message)
        {
            Task.Run(() => sendWebhookMessage(WebhookConfiguration.GetWebhook(Server), message));
        }

        public void SendReportMessage(string message)
        {
            Task.Run(() => sendWebhookMessage(WebhookConfiguration.GetReportsWebhook(Server), message));
        }

        private async Task sendChatMessagesToDiscord()
        {
            do
            {
                List<DiscordMessage> messages = new();
                do
                {
                    try
                    {
                        while (discordMessageQueue.TryDequeue(out DiscordMessage? message))
                        {
                            if (message == null)
                            {
                                continue;
                            }

                            messages.Add(message);
                        }


                        if (messages.Count > 0)
                        {
                            await sendWebhookMessage(WebhookConfiguration.GetWebhook(Server), string.Join(Environment.NewLine, messages.Select(message => message.ToString())));
                        }

                        messages.Clear();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"EXCEPTION IN DISCORD MESSAGE QUEUING:{Environment.NewLine}{ex}");
                        await Task.Delay(500);
                    }
                } while (messages.Count > 0);

                await Task.Delay(250);
            } while (Server?.IsConnected == true);
        }

        private async Task sendWebhookMessage(string webhookUrl, string message)
        {
            bool success = false;
            while (!success)
            {
                var payload = new
                {
                    content = message
                };

                var payloadJson = JsonSerializer.Serialize(payload);
                var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error sending webhook message. Status Code: {response.StatusCode}");
                }

                success = response.IsSuccessStatusCode;
            }
        }

    }

    internal class DiscordMessage
    {
    }

    internal class RawTextMessage : DiscordMessage
    {
        public string Message { get; set; }

        public RawTextMessage(string message)
        {
            this.Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    internal class ChatMessage : DiscordMessage
    {
        public string PlayerName { get; set; } = string.Empty;

        public ChatMessage(string playerName, ulong steamID, ChatChannel channel, string message)
        {
            PlayerName = playerName;
            SteamID = steamID;
            Channel = channel;
            Message = message;
        }

        public ulong SteamID { get; set; }
        public ChatChannel Channel { get; set; }
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return $":speech_balloon: [{SteamID}] {PlayerName}: {Message}";
        }
    }

    internal class JoinAndLeaveMessage : DiscordMessage
    {
        public int PlayerCount { get; set; }

        public JoinAndLeaveMessage(int playerCount, string playerName, ulong steamID, bool joined)
        {
            PlayerCount = playerCount;
            PlayerName = playerName;
            SteamID = steamID;
            Joined = joined;
        }

        public string PlayerName { get; set; } = string.Empty;
        public ulong SteamID { get; set; }
        public bool Joined { get; set; }

        public override string ToString()
        {
            return $"{(Joined ? ":arrow_right:" : ":arrow_left:")} [{this.SteamID}] {this.PlayerName} {(this.Joined ? "joined" : "left")} ({this.PlayerCount} players)";
        }
    }

    internal class WarningMessage : DiscordMessage
    {
        public WarningMessage(string message)
        {
            Message = message;
        }

        public bool IsError { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $":warning: {Message}";
        }
    }

    internal class WebhookConfiguration
    {
        private static Dictionary<string, string> webhooks = Program.GetGlobalConfig().Webhooks;
        private static Dictionary<string, string> reportWebhooks = Program.GetGlobalConfig().ReportWebhooks;

        public static string GetWebhook(CustomServer server)
        {
            return webhooks[server.ToString()];
        }

        public static string GetReportsWebhook(CustomServer server)
        {
            return reportWebhooks[server.ToString()];
        }
    }
}