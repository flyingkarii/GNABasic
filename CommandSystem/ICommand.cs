namespace GNA.Core.CommandSystem
{
    public interface ICommand
    {
        public void Execute(CommandContext arguments);
    }
}
