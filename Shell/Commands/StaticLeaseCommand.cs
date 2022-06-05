using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Shell.Commands
{
    public class StaticLeaseCommand : IShellCommand
    {
        public string Command => "reservation";

        public void Execute(DHCPShell shell, string[] data)
        {
            Console.WriteLine("Reservation command implemented!");
        }
    }
}
