using System.Collections.Generic;
using System.Linq;

namespace EDennis.NetStandard.Base {

	/// <summary>
	/// A model class that holds relevant information from an individual 
	/// launchSettings.json file for a web application.
	/// </summary>
    public class LaunchProfile {
		public string Name { get; set; }
		public string CommandName { get; set; }
		public string ApplicationUrl {
			get {
				if (ApplicationUrls == null)
					return null;
				else
					return string.Join(";", ApplicationUrls.Select(u => $"{u.Scheme}://{u.Host}:{u.Port}"));
			}
		}
		public bool LaunchBrowser { get; set; }
		public string LaunchUrl { get; set; }
		public string CommandLineArgs { 
			get {
				if (Args == null)
					return null;
				else
					return string.Join(" ", Args);
			}
		}
		public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
		public ApplicationUrl[] ApplicationUrls { get; set; }

		public string[] Args { get; set; }
	}
}
