using Nitrox_PublixExtension.Core;
using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Events.Attributes;
using Nitrox_PublixExtension.Core.Events.Base;
using NitroxModel.Logger;
using NitroxServer;
using NitroxServer.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publix_DiscordRelayPlugin.EventListeners
{
    public class ChatEventListener : Nitrox_PublixExtension.Core.Events.EventListener
    {
        public DiscordRelayPlugin plugin;
        public ChatEventListener(DiscordRelayPlugin plugin) 
        {
            this.plugin = plugin;
        }

        [ListenerMethod(ListenerType.PacketRecieved)]
        public void OnPlayerChatMessage(Event ev, Player player, NitroxModel.Packets.ChatMessage packet)
        {
            plugin.SendChatMessage(player.Name, packet.Text);
        }

        [ListenerMethod(ListenerType.PacketSentAll)]
        public void OnServerChatMessage(Event ev, NitroxModel.Packets.ChatMessage packet)
        {
            if (packet.PlayerId == ushort.MaxValue)
            {
                if (packet.Text != "Server is shutting down...") //block out the shutdown message to prevent double messages
                {
                    plugin.SendChatMessage("System", packet.Text, true);
                }
            }
        }
    }
}
