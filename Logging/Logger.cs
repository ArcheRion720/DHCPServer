namespace DHCPServer.Logging
{
    public abstract class LoggerBase
    {
        protected readonly object sync = new();
        public abstract void Log(LogLevel level, string server, string message);
        public abstract Task LogAsync(LogLevel level, string server, string message);
    }

    public enum LogLevel
    {
        INFO,
        ERROR
    }

    public class FileLogger : LoggerBase
    {
        public string Path { get; set; }
        public FileLogger(string path)
        {
            Path = path;
        }

        public override void Log(LogLevel level, string message, string? server = null)
        {
            lock (sync)
            {
                string log = $"[{level,-5}] [{DateTime.Now:ddd, dd MMM, yyyy HH:mm:ss}] {((server is null) ? "[GENERAL]" : $"[{server}]"),-8} {message}";
                using StreamWriter writer = new(Path, true);
                writer.WriteLine(log);
            }
        }

        public override async Task LogAsync(LogLevel level, string message, string? server = null)
        {
            string log = $"[{level,-5}] [{DateTime.Now:ddd, dd MMM, yyyy HH:mm:ss}] {((server is null) ? "[GENERAL]" : $"[{server}]"),-8}] {message}";
            using StreamWriter writer = new(Path, true);
            await writer.WriteLineAsync(log);
        }
    }
}
