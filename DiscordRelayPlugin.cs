using Nitrox_PublixExtension.Core;
using Nitrox_PublixExtension.Core.Plugin;
using Nitrox_PublixExtension.Core.Plugin.Attributes;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Publix_DiscordRelayPlugin;
using Publix_DiscordRelayPlugin.EventListeners;

namespace Publix_DiscordRelayPlugin
{
    [PluginInfo("dev.drperky.discordrelay", "Discord Relay", "0.1", "1.0.3")]
    [PluginDescription("Discord <-> Subnautica Relay")] //Description isnt used at the time of writing this, so its optional
    public class DiscordRelayPlugin : BasePlugin
    {
        public class DiscordBotConfig
        {
            public string BotToken { get; set; } = "BOT_TOKEN_HERE";
            public string ReplayChannelID { get; set; } = "CHANNEL_ID_HERE";
        }

        public BasicDiscordBot bot { get; private set; }
        public DiscordBotConfig botConfig { get; private set; }
        private long startTime = 0;
        private bool canRelay = false;

        public override void OnLoad()
        {
            botConfig = GetConfigManager().GetConfig<DiscordBotConfig>();

            if (botConfig.BotToken == "BOT_TOKEN_HERE")
            {
                GetLogger().Error("Please set your bot token in the config file before using the plugin.");
                return;
            }

            if (botConfig.ReplayChannelID == "CHANNEL_ID_HERE")
            {
                GetLogger().Error("Please set your replay channel id in the config file before using the plugin.");
                return;
            }

            bot = new BasicDiscordBot(botConfig.BotToken);
            bot.OnReadyCallback(() =>
            {
                UpdateActivity();
            });
            bot.OnMessageCallback((root) =>
            {
                if (!canRelay)
                    return;

                var data = root.GetProperty("d");

                if (data.GetProperty("channel_id").GetString() != null)
                {
                    string channelId = data.GetProperty("channel_id").GetString();

                    if (channelId == botConfig.ReplayChannelID)
                    {
                        var author = data.GetProperty("author");
                        bool isBot = false;

                        try
                        {
                            isBot = author.GetProperty("bot").GetBoolean();
                        }
                        catch
                        {
                        }


                        if (!isBot) //ignore bot messages
                        {
                            string content = data.GetProperty("content").GetString();
                            string username = author.GetProperty("username").GetString();

                            ChatMessage chatPacket = new ChatMessage(ushort.MaxValue, $"<{username}> {content}");

                            Publix.getPlayerManager().internalPlayerManager.GetConnectedPlayers().ForEach(player =>
                            {
                                player.SendPacket(chatPacket);
                            });
                        }
                    }
                }
            });
            bot.Start();
        }

        public override void OnEnable()
        {
            Publix.getEventManager().Register(this, new ConnectionEventListener(this));
            Publix.getEventManager().Register(this, new ChatEventListener(this));

            startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            SendChatMessage("System", "Server is online", true);
            UpdateActivity();

            canRelay = true;
        }

        public override void OnDisable()
        {
            if (bot != null)
            {
                SendChatMessage("System", "Server is offline", true);
                bot.Stop();
            }
        }

        public void UpdateActivity()
        {
            try
            {
                int ConnectedPlayers = Publix.getPlayerManager().internalPlayerManager.GetConnectedPlayers().Count;
                int MaxPlayers = Publix.GetSubnauticaServerConfig().MaxConnections;

                Log.Info($"{ConnectedPlayers}/{MaxPlayers} Survivors");

                bot.SetActivityAndStatus($"{ConnectedPlayers}/{MaxPlayers} Survivors", "online", 3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime);
            } catch
            {

            }
            
        }

        public void SendChatMessage(string playerName, string message, bool bold = false)
        {
            bot.SendMessage(botConfig.ReplayChannelID, $"<{(bold ? "**" : "")}{playerName}{(bold ? "**" : "")}> {message}");
        }

        public void SendMessage(string message)
        {
            bot.SendMessage(botConfig.ReplayChannelID, message);
        }
    }
}
