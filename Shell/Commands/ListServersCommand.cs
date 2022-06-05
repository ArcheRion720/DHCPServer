namespace DHCPServer.Shell.Commands
{
    public class ListServersCommand : IShellCommand
    {
        public string Command => "list";

        public void Execute(DHCPShell shell, string[] data)
        {
            var servers = DHCPServersManager.Instance.Servers;

            Console.WriteLine($"| {"Id",-2} | {"Name", -15} | {"Client count", -12} |");
            for (int i = 0; i < servers.Count; i++)
            {
                var clientsCount = servers[i].Clients.Where(x => x.State == LeaseState.Assigned).Count();
                Console.WriteLine($"| {i, -2} | {servers[i].Name, -15} | {clientsCount, -12} |");
            }
        }
    }
}
