using Nitrox_PublixExtension.Core.Events;
using Nitrox_PublixExtension.Core.Events.Attributes;
using Nitrox_PublixExtension.Core.Events.Base;
using NitroxModel.Logger;
using NitroxServer.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publix_ExamplePlugin.EventListeners
{
    public class ExampleEventListener : EventListener
    {
        public ExamplePlugin plugin;
        public ExampleEventListener(ExamplePlugin plugin) 
        {
            this.plugin = plugin;
        }

        [ListenerMethod(ListenerType.PacketEvent)]
        public void DoublePVPDamage(Event ev, INitroxConnection connection, NitroxModel.Packets.PvPAttack packet)
        {
            packet.Damage *= 2;
        }

        [ListenerMethod(ListenerType.PacketEvent)]
        public void ChatMessageEvent(Event ev, INitroxConnection connection, NitroxModel.Packets.ChatMessage packet)
        {
            plugin.GetLogger().Info($"Example Event Log: {packet.Text}");
        }
    }
}
