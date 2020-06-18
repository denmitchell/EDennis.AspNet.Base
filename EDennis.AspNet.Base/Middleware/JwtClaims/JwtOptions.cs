namespace EDennis.AspNet.Base.Middleware {
    public class JwtOptions {
        public string Issuer {get; set;}
        public string Kid { get; set; }
        public int ExpirationHours { get; set; }

    }
}