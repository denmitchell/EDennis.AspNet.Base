using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hr.BlazorApp.Data.ApiClients {
    public class HrApiClient {
        public HttpClient HttpClient { get; set; } 
    }
}
