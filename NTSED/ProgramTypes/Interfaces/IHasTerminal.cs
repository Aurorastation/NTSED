namespace NTSED.ProgramTypes.Interfaces
{
    public interface IHasTerminal
    {
        public Task HandleTopic(string hash, string data);

        public string GetTerminalBuffer();
    }
}
