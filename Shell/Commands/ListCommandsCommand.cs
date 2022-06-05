namespace DHCPServer.Shell.Commands
{
    public class ListCommandsCommand : IShellCommand
    {
        public string Command => "commands";

        public void Execute(DHCPShell shell, string[] data)
        {
            int index = 0;
            foreach(var item in shell.Commands.Keys)
            {
                Console.WriteLine($"| {index++,3} | {item,-15} |");
            }
        }
    }
}
