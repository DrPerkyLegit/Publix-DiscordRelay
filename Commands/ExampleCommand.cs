using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace Publix_ExamplePlugin.Commands
{
    internal class ExampleCommand : Command
    {
        //this command doesnt use alias so its not added
        //public override IEnumerable<string> Aliases { get; } = new[] { "" };

        public ExampleCommand() : base("example", Perms.PLAYER, "an example command")
        {
            //AddParameter(new TypeString("command", false, "Command to see help information for"));
        }

        protected override void Execute(CallArgs args)
        {
            if (args.IsConsole)
            {
                Log.Info("Example Command Ran By Console"); //this will never run atm cause we dont support console commands yet
            }
            else
            {
                SendMessageToPlayer(args.Sender, "example command!!!!");
            }
        }
    }
}
