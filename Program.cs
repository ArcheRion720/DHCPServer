using CommandLine;
using CommandLine.Text;
using DHCPServer.Logging;
using DHCPServer.Shell;
using Newtonsoft.Json;

namespace DHCPServer
{ 
    public class Program
    {
        public static void Main(String[] args)
        {
            var parser = Parser.Default.ParseArguments<ServerSettings, ConfigSettings>(args);
            parser.WithParsed<ServerSettings>(x => StartDHCPServer(x));
            parser.WithParsed<ConfigSettings>(x => StartConfigSetup(x));
            parser.WithNotParsed(errs => DisplayHelp(errs, parser));
        }

        private static void DisplayHelp(IEnumerable<Error> errs, ParserResult<object> parser)
        {
            var help = HelpText.AutoBuild(parser, h => h, x => x);

            return;
        }

        private static void StartDHCPServer(ServerSettings settings)
        {
            List<DHCPConfigStub>? configStubs = null;
            var configFile = settings.ConfigFile ?? "config.json";

            if (!File.Exists(configFile))
            {
                Console.WriteLine($"Failed to find configuration file {Path.GetFullPath(configFile)}");
            }
            else
            {
                JsonSerializerSettings serializerSettings = new();
                serializerSettings.Error += (x, y) => Console.WriteLine("Failed to load configuration file (Malformed or missing fields)");
                serializerSettings.NullValueHandling = NullValueHandling.Ignore;

                configStubs = JsonConvert.DeserializeObject<List<DHCPConfigStub>>(File.ReadAllText(configFile), serializerSettings);
            }

            if (settings.LogFile is not null)
            {
                DHCPServersManager.Instance.AddLogging(settings.LogFile);
            }

            if (configStubs is not null)
            {
                foreach (DHCPConfigStub configStub in configStubs)
                {
                    if (DHCPConfig.CreateConfig(configStub, out DHCPConfig conf))
                    {
                        DHCPServersManager.Instance.AddServer(conf);
                    }
                    else
                    {
                        string message = $"Failed to load configuration: {configStub.Name}";
                        Console.WriteLine(message);
                        DHCPServersManager.Instance.Logger?.Log(LogLevel.ERROR, message);
                    }
                }
            }

            DHCPShell shell = new();
            shell.LoadCommands();
            shell.Start().Join();

            DHCPServersManager.Instance.Stop();
        }

        private static void StartConfigSetup(ConfigSettings settings)
        {

        }
    }
}
