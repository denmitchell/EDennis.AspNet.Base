using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Launcher {

    public class Launchable {
        public int LaunchGroup { get; set; }
        public string ProjectDirectory { get; set; }
        public Action<string[]> ProgramMain { get; set; }
        public LaunchProfile LaunchProfile { get; set; }
        public NamedEventWaitHandle ReadyEvent { get; set; }
        public NamedEventWaitHandle AllSuspendEvent { get; set; }

    }
}
