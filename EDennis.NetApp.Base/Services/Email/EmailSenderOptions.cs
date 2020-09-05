namespace EDennis.NetApp.Base {
    public class EmailSenderOptions {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
