using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica;
using Nitrox_PublixExtension.Core;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Events.Attributes;
using Nitrox_PublixExtension.Core.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Publix_DiscordRelayPlugin.EventListeners
{
    public class ConnectionEventListener : EventListener
    {
        public DiscordRelayPlugin plugin;

        public Dictionary<ushort, string> playerNameCache = new Dictionary<ushort, string>();
        public ConnectionEventListener(DiscordRelayPlugin plugin) 
        {
            this.plugin = plugin;
        }

        [ListenerMethod(ListenerType.PacketSentOthers)]
        public void PlayerConnectPacket(Event ev, Player player, PlayerJoinedMultiplayerSession packet)
        {
            plugin.SendMessage($"{player.Name} Has Connected");
            plugin.UpdateActivity();

            playerNameCache.Add(player.Id, player.Name);
        }

        [ListenerMethod(ListenerType.PacketSentOthers)]
        public void PlayerDisconnectedPacket_Others(Event ev, Player player, Disconnect packet)
        {
            plugin.SendMessage($"{player.Name} Has Disconnected");
            plugin.UpdateActivity();

            playerNameCache.Remove(player.Id);
        }

        [ListenerMethod(ListenerType.PacketSentAll)]
        public void PlayerDisconnectedPacket_All(Event ev, Disconnect packet)
        {
            playerNameCache.TryGetValue(packet.PlayerId, out string playerNameCacheValue);
            if (playerNameCacheValue == null)
            {
                playerNameCacheValue = "Unknown Player";
            }

            plugin.SendMessage($"{playerNameCacheValue} Has Disconnected");
            plugin.UpdateActivity();

            playerNameCache.Remove(packet.PlayerId);
        }
    }
}
