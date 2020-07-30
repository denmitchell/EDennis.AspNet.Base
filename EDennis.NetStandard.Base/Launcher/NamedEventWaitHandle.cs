using System;
using System.Threading;

namespace EDennis.NetStandard.Base.Launcher {

    /// <summary>
    /// This class is used by LauncherBase and LauncherFixture to listen across threads/processes
    /// for (a) each launched web app being ready for use (TCP-pingable) and (b) the calling program
    /// being done with all launched apps (and hence the processes/threads associated with these
    /// apps can be suspended).
    /// </summary>
    public class NamedEventWaitHandle : EventWaitHandle {
        public string Name { get; set; }

        public NamedEventWaitHandle(bool initialState = true, EventResetMode mode = EventResetMode.ManualReset)
            : this(initialState, mode, Guid.NewGuid().ToString()) {
        }
        public NamedEventWaitHandle(bool initialState = true, EventResetMode mode = EventResetMode.ManualReset, string name = "") 
            : base(initialState, mode, name) {
        }
    }
}
