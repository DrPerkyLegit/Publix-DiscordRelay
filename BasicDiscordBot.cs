using NitroxModel.Logger;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Publix_DiscordRelayPlugin
{
    public class BasicDiscordBot
    {
        private readonly string _token;
        private readonly ClientWebSocket _ws = new ClientWebSocket();
        private volatile int _heartbeatInterval;
        private volatile int _lastSequence = -1;
        private volatile bool _running;

        private Action _onReady = () => { };
        private Action<JsonElement> _onMessage = (root) => { };

        public BasicDiscordBot(string token)
        {
            _token = token;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                await _ws.ConnectAsync(new Uri("wss://gateway.discord.gg/?v=10&encoding=json"), CancellationToken.None);

                _ = ListenLoop();
                _ = HeartbeatLoop();

                int intents = 0;

                intents |= (1 << 15); // MESSAGE_CONTENT
                intents |= 512; // GUILD_MESSAGES

                await SendAsync(JsonSerializer.Serialize(new
                {
                    op = 2,
                    d = new
                    {
                        token = _token,
                        intents,
                        properties = new
                        {
                            os = (OperatingSystem.IsWindows() ? "windows" : (OperatingSystem.IsMacOS() ? "macos" : "linux")), //is this needed? i dont think so
                            browser = "Publix DiscordRelay",
                            device = "Publix DiscordRelay"
                        },
                        presence = new
                        {
                            status = "idle",
                            activities = new[] { new { name = "The Server Start", type = 3 } },
                        }
                    }
                }));
            });
        }

        public void Stop()
        {
            _running = false;
            Task.Run(async () =>
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bot stopped", CancellationToken.None);
                }
                catch { }
            });
        }

        public void OnReadyCallback(Action callback)
        {
            _onReady = callback;
        }

        public void OnMessageCallback(Action<JsonElement> callback)
        {
            _onMessage = callback;
        }

        public void SetActivityAndStatus(string name, string status = "online", int type = 0, long? since = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    var data = new
                    {
                        op = 3,
                        d = new
                        {
                            since,
                            activities = new[] { new { name, type } },
                            status,
                            afk = false
                        }
                    };
                    
                    var JsonParsed = JsonSerializer.Serialize(data);
                    await SendAsync(JsonParsed);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to Update Status: {ex.Message}");
                }
            });
        }

        public void SendMessage(string channelId, string message)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var http = new HttpClient();
                    http.DefaultRequestHeaders.Add("Authorization", $"Bot {_token}");
                    var content = new StringContent(JsonSerializer.Serialize(new
                    {
                        content = message,
                        allowed_mentions = new //no pings
                        {
                            parse = Array.Empty<string>()
                        }
                    }), Encoding.UTF8, "application/json");

                    await http.PostAsync($"https://discord.com/api/v10/channels/{channelId}/messages", content);
                } catch (Exception ex)
                {
                    Log.Error($"Failed to send message to Discord: {ex.Message}");
                }

            });
        }

        private async Task ListenLoop()
        {
            var buffer = new byte[8192];
            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                using var doc = JsonDocument.Parse(msg);
                var root = doc.RootElement;
                int op = root.GetProperty("op").GetInt32();

                //TODO: handle opcodes 11, 9, 7
                //TODO: basic command integration maybe (using messages)
                //Log.Info($"Recieved: {msg}");
                switch (op)
                {
                    case 0: // Event
                        _lastSequence = root.GetProperty("s").GetInt32();

                        string eventType = root.GetProperty("t").GetString();
                        if (eventType == "READY")
                        {
                            _running = true;
                            _onReady?.Invoke();
                        } else if (eventType == "MESSAGE_CREATE") 
                        {
                            _onMessage?.Invoke(root);
                        }
                            break;
                    case 10: // Hello
                        _heartbeatInterval = root.GetProperty("d").GetProperty("heartbeat_interval").GetInt32();
                        break;
                }
            }
        }

        private async Task HeartbeatLoop()
        {
            while (true)
            {
                if (_running)
                {
                    var heartbeat = new
                    {
                        op = 1,
                        d = (_lastSequence == -1 ? null : (int?)_lastSequence)
                    };
                    await SendAsync(JsonSerializer.Serialize(heartbeat));
                }
                await Task.Delay(_heartbeatInterval > 0 ? _heartbeatInterval : 30000);
            }
        }

        private async Task SendAsync(string json)
        {
            if (_ws.State != WebSocketState.Open)
                return;

            //Log.Info($"Sent: {json}");

            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}