using System.Collections.Generic;
using System.Linq;

namespace GNA.Core.CommandSystem.Commands
{
    public class Message : ICommand
    {
        [Permission(DefaultGroup.Owner)]
        public void Execute(CommandContext arguments)
        {
            string result = string.Join<string>(" ", arguments.Arguments.ToList());
            arguments.Server.SayToAllChat(result);
        }
    }
}
