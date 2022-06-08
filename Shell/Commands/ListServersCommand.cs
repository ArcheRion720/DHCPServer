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
                var clients      = servers[i].Clients;
                var clientsCount = clients.Where(x => x.State == LeaseState.Assigned).Count();
                var reservations = clients.Where(x => x.State == LeaseState.Static).Count();

                Console.Write($"| {i,-2} | {servers[i].Name,-15} | ");

                if (reservations > 0)
                    Console.WriteLine($"{$"{clientsCount} [+{reservations}]",-12} |");
                else
                    Console.WriteLine($"{clientsCount,-12} |");
            }
        }
    }
}
