using Microsoft.Extensions.Logging;

namespace EDennis.AspNet.Base.Launcher {
    
    /// <summary>
    /// Entry-point for the Launcher.
    /// </summary>
    public interface ILauncher {
        void Launch(string[] args, ILogger logger, bool launchBrowser = false, bool blockWithConsole = false);
    }
}