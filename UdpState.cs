using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer
{
    public struct UdpState
    {
        public IPEndPoint endpoint;
        public UdpClient udpClient;
    }
}
