namespace EDennis.NetStandard.Base {
    public class RequestForwardingOptions {
        public bool ForwardQueryString { get; set; } = true;
        public string[] HeadersToForward { get; set; } = new string[] { CachedTransactionOptions.COOKIE_KEY, HeaderToClaimsOptions.HEADER_KEY };
        public string[] CookiesToForward { get; set; } = new string[] { CachedTransactionOptions.COOKIE_KEY };
        public bool ForwardRequestBody { get; set; } = false;
    }
}
