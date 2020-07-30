namespace EDennis.NetStandard.Base.Launcher {
	/// <summary>
	/// Holds parsed URL information for a launchable web application
	/// </summary>
	public class ApplicationUrl {
		public string Scheme { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public ApplicationUrl() { }
		public ApplicationUrl(string url) {
			var components = url.Split(':');
			Scheme = components[0];
			Host = components[1].Substring(2);
			Port = int.Parse(components[2]);
		}
	}

}
