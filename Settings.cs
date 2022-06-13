using CommandLine;

namespace DHCPServer
{  
    [Verb("server", true, HelpText = "Server mode running configurations. For more help see \"server --help\" option")]
    public class ServerSettings
    {
        [Option('c', "config", HelpText = "The path of a configuration file")]
        public string? ConfigFile { get; set; }
        [Option('l', "log", HelpText = "The path of a logs file")]
        public string? LogFile { get; set; }
    }

    public enum ConfigOperation
    {
        CREATE = 1,
        SANITIZE = 2,        
    }

    [Verb("config", HelpText = "Configuration mode allowing creation of configurations. For more help see \"config --help\" option")]
    public class ConfigSettings
    {
        [Option('f', "file", Required = true, HelpText = "The path of a configuration file to manipulate")]
        public string? File { get; set; }

        [Option('o', "operation")]
        public ConfigOperation? Operation { get; set; }
        [Option('i', "interactive")]
        public bool Interactive { get; set; }
    }
}
