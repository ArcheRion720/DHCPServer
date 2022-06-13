using DHCPServer.Logging;
using System.Net;
using System.Net.NetworkInformation;

namespace DHCPServer
{
    public class DHCPServersManager
    {
        public List<DHCPServer> Servers { get; set; }
        public FileLogger? Logger { get; private set; }

        private DHCPServersManager()
        {
            Servers = new List<DHCPServer>();
            Logger = null;
        }

        private static DHCPServersManager? instance = null;
        public static DHCPServersManager Instance
        {
            get => instance ??= new DHCPServersManager();
        }

        public void AddServer(DHCPConfig config)
        {
            DHCPServer server = new DHCPServer(config, Logger);

            if (server.Config.Data.Reservations is not null)
            {
                foreach (var item in server.Config.Data.Reservations)
                {
                    if (PhysicalAddress.TryParse(item.Hardware, out var pa) && IPAddress.TryParse(item.IPAddress, out var ip))
                    {
                        server.ReserveAddress(pa.GetAddressBytes(), ip);
                    }
                }
            }

            server.Start();
            Servers.Add(server);
        }

        public void AddLogging(string file)
        {
            Logger = new FileLogger(file);
        }

        public void Stop()
        {
            foreach(var server in Servers)
            {
                server.Stop();
            }
        }
    }
}
