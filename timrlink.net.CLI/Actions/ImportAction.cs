using Microsoft.Extensions.Logging;

namespace timrlink.net.CLI.Actions
{
    internal abstract class ImportAction
    {
        protected ILogger Logger { get; }

        protected ImportAction(ILogger logger)
        {
            Logger = logger;
        }
    }
}
