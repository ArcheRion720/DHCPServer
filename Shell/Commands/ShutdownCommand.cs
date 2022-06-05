using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPServer.Shell.Commands
{
    public class ShutdownCommand : IShellCommand
    {
        public string Command => "shutdown";

        public void Execute(DHCPShell shell, string[] data)
        {
            shell.Stop();
        }
    }
}
