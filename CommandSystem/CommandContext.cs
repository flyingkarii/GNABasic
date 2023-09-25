namespace GNA.Core.CommandSystem
{
    public struct CommandContext
    {
        public CustomServer Server;
        public CustomPlayer Executor;
        public string Command;
        public string[] Arguments;
        public string RawMessage;

        public void ErrorReply(string message)
        {
            if (Executor == null)
                return;

            Executor.SayToChat($"<color=#fa6666>[SERVER]</color> {message}");
        }

        public void SuccessReply(string message)
        {
            if (Executor == null)
                return;

            Executor.SayToChat($"<color=#66fa8e>[SERVER]</color> {message}");
        }

        public void Reply(string message)
        {
            if (Executor == null)
                return;

            Executor.SayToChat($"<color=#fa9f66>[SERVER]</color> {message}");
        }

        public bool TryGetArgumentAsPlayer(int index, out CustomPlayer? player)
        {
            if (index >= Arguments.Length)
            {
                player = null;
                return false;
            }

            CustomPlayer[] players = Server.SearchPlayerByName(Arguments[index]).ToArray();

            if (players.Length > 0)
            {
                player = players[0];
                return true;
            }

            player = null;
            return false;
        }

        public bool TryGetArgumentAsInteger(int index, out int number)
        {
            if (index >= Arguments.Length)
            {
                number = -1;
                return false;
            }

            if (int.TryParse(Arguments[index], out number))
                return true;

            number = -1;
            return false;
        }

        public bool CheckIfSteamId(int index)
        {
            if (index >= Arguments.Length)
            {
                return false;
            }

            if (Arguments[index].StartsWith("7656119"))
            {
                return true;
            }

            return false;
        }
    }
}
