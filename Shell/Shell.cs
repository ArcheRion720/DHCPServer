using DHCPServer.Shell.Commands;
using System.Reflection;

namespace DHCPServer.Shell
{
    public class DHCPShell
    {
        public Dictionary<string, IShellCommand> Commands { get; private set; }
        private readonly CancellationTokenSource cancellationToken;
        private Thread? consoleThread;

        public DHCPShell()
        {
            this.Commands = new();
            this.consoleThread = null;
            cancellationToken = new CancellationTokenSource();
        }

        public Thread Start()
        {
            Thread consoleThread = new(async () =>
            {
                while (true)
                {
                    Console.Write("> ");
                    string? text = await Console.In.ReadLineAsync();

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (text is null)
                        continue;

                    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length == 0)
                        continue;

                    if(Commands.TryGetValue(words[0], out var action))
                    {
                        action.Execute(this, words);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            });

            consoleThread.Start();
            this.consoleThread = consoleThread;

            return consoleThread;
        }

        public void Stop()
        {
            cancellationToken.Cancel();
        }

        public void LoadCommands()
        {
            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IShellCommand).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();
            foreach(var command in commands)
            {
                IShellCommand? cmd = Activator.CreateInstance(command) as IShellCommand;
                if(cmd is not null)
                {
                    this.Commands.Add(cmd.Command, cmd);
                }
            }
        }
    }
}
