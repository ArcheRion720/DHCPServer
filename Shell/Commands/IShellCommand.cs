namespace DHCPServer.Shell.Commands
{
    public interface IShellCommand
    {
        public string Command { get; }
        public void Execute(DHCPShell shell, string[] data);
    }
}
