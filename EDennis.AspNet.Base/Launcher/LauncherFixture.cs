using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;

namespace EDennis.AspNet.Base.Launcher {

    /// <summary>
    /// This class can be used as an xUnit class fixture, which starts all web apps
    /// configured in a Launcher project, where that project has a Program clas
    /// that implements ILauncher (probably extending LauncherBase).
    /// </summary>
    /// <typeparam name="TLauncher"></typeparam>
    public class LauncherFixture<TLauncher> : IDisposable
        where TLauncher : ILauncher, new() {

        private readonly NamedEventWaitHandle ewh;

        /// <summary>
        /// Setup a NamedEventWaitHandle that will be used to signal the end of
        /// all tests and the need to shutdown all launched servers.
        /// </summary>
        public LauncherFixture() {
            string ewhAllSuspendName = Guid.NewGuid().ToString();
            var args = new string[] { $"ewhAllSuspend={ewhAllSuspendName}" };
            ewh = new NamedEventWaitHandle(false, EventResetMode.ManualReset, ewhAllSuspendName);
            new TLauncher().Launch(args,NullLogger.Instance,false,false);
        }

        /// <summary>
        /// Signal the end of all tests and the need to shutdown all launched servers
        /// </summary>
        public void Dispose() {
            ewh.Set();
        }
    }
}
