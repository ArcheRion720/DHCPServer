using System.Net;
using System.Net.NetworkInformation;

namespace DHCPServer.Shell.Commands
{
    public class LeasesCommand : IShellCommand
    {
        public string Command => "leases";

        public void Execute(DHCPShell shell, string[] data)
        {
            if(data.Length == 2)
            {
                var server = DHCPServersManager.Instance.Servers.FirstOrDefault(x => x.Name == data[1]);
                if(server != null)
                {
                    Console.WriteLine($"|   | {"Leased address",-15} | {"MAC",-15} | {"Lease started",-19} | {"Lease ends",-19} |");
                    foreach (var client in server.Clients.Where(x => x.State == LeaseState.Assigned))
                    {
                        IPAddress ip = new IPAddress(client.IPAddress!);
                        PhysicalAddress pa = new PhysicalAddress(client.HardwareAddress!.Take(6).ToArray());
                        Console.WriteLine($"| < | {ip,-15} | {pa,-15} | {client.LeaseStartTime,-19} | {client.LeaseEndTime,-19} |");
                    }

                    foreach (var client in server.Clients.Where(x => x.State == LeaseState.Static))
                    {
                        IPAddress ip = new IPAddress(client.IPAddress!);
                        PhysicalAddress pa = new PhysicalAddress(client.HardwareAddress!.Take(6).ToArray());
                        Console.WriteLine($"| + | {ip,-15} | {pa,-15} | {" ",-19} | {" ",-19} |");
                    }
                }
                else
                {
                    Console.WriteLine($"Server {data[1]} not found!");
                }
            }
            else
            {
                Console.WriteLine("Usage: leases [SERVER]");
            }
        }
    }
}
