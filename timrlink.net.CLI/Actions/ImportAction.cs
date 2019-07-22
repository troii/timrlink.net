using Microsoft.Extensions.Logging;

namespace timrlink.net.CLI.Actions
{
    public abstract class ImportAction
    {
        protected string Filename { get; }

        protected ILogger Logger { get; }

        protected ImportAction(string filename, ILogger logger)
        {
            Filename = filename;
            Logger = logger;
        }

        public abstract System.Threading.Tasks.Task Execute();
    }
}
