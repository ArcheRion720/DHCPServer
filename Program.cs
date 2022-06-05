using DHCPServer.Shell;
using Newtonsoft.Json;

namespace DHCPServer
{ 
    public class Program
    {
        public static void Main(String[] args)
        {
            List<DHCPConfigStub>? configStubs = new List<DHCPConfigStub>();
            if(File.Exists("Config.txt"))
            {
                configStubs = JsonConvert.DeserializeObject<List<DHCPConfigStub>>(File.ReadAllText("Config.txt"));
            }
            
            if(configStubs is null)
            {
                Console.WriteLine("Failed to load configurations!");
                return;
            }

            foreach (DHCPConfigStub configStub in configStubs)
            {
                if (DHCPConfig.CreateConfig(configStub, out DHCPConfig conf))
                {
                    DHCPServersManager.Instance.AddServer(conf);
                }
            }

            DHCPShell shell = new();
            shell.LoadCommands();
            shell.Start().Join();

            DHCPServersManager.Instance.Stop();
        }
    }
}
