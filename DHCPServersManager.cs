using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer
{
    public class DHCPServersManager
    {
        public List<DHCPServer> Servers { get; set; }

        private DHCPServersManager()
        {
            Servers = new List<DHCPServer>();
        }

        private static DHCPServersManager? instance = null;
        public static DHCPServersManager Instance
        {
            get => instance ??= new DHCPServersManager();
        }

        public void AddServer(DHCPConfig config)
        {
            DHCPServer server = new DHCPServer(config);

            if (server.Config.Reservations is not null)
            {
                foreach (var item in server.Config.Reservations)
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

        public void Stop()
        {
            foreach(var server in Servers)
            {
                server.Stop();
            }
        }
    }
}
