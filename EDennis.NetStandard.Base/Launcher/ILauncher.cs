using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base.Launcher {
    
    /// <summary>
    /// Entry-point for the Launcher.
    /// </summary>
    public interface ILauncher {
        void Launch(string[] args, bool launchBrowser = false, bool blockWithConsole = false);
    }
}