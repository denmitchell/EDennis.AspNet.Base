using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EDennis.NetStandard.Base {


    public class ScopedRequestMessage : HttpRequestMessage {

        public ScopedRequestMessage() {
            Properties.Add("QueryString", null);
        }


        public void AddHeader(string key, string value) {
            var matching = Headers.Where(h => h.Key == key).SelectMany(h=>h.Value);
            if (matching.Any(m => m == value))
                return;
            if (matching.Count() > 0) {
                Headers.Remove(key);
                Headers.Add(key, matching.Append(value));
            } else {
                Headers.Add(key, value);
            }                
        }

        public void AddHeader(string key, StringValues values) {
            var matching = Headers.Where(h => h.Key == key).SelectMany(h => h.Value);
            if (matching.Count() > 0) {
                Headers.Remove(key);
                Headers.Add(key, matching.Union(values));
            } else {
                Headers.Add(key, values.ToArray());
            }
        }


        public void AddCookie(string key, string value) {
            var matching = Headers.Where(h => h.Key == "Cookie").SelectMany(h => h.Value);
            if (matching.Any(m => m == value))
                return;
            if (matching.Count() > 0) {
                Headers.Remove(key);
                var cookies = string.Join(";", matching).UnPackDictionary(('=', ';'));
                if (cookies.ContainsKey(key))
                    cookies[key] = value;
                else
                    cookies.Add(key, value);
                Headers.Add("Cookie", cookies.Pack(('=', ';')));
            } else
                Headers.Add("Cookie", $"{key}={value}");
        }


        public void AddQueryString(string value) {
            Properties["QueryString"] = value;
        }



        public void AddHeaders(IHeaderDictionary headers) {
            foreach(var header in headers) {
                AddHeader(header.Key, header.Value);
            }
        }

        public void AddCookies(IRequestCookieCollection cookies) {
            foreach (var cookie in cookies)
                AddCookie(cookie.Key, cookie.Value);
        }



        public bool TryGetCookie(string key, out string value) {
            value = null;
            var cookies = Headers.Where(h => h.Key == "Cookie").SelectMany(x=>x.Value).FirstOrDefault();
            if (cookies == null)
                return false;
            
            var dict = cookies.UnPackDictionary(('=', ';'));
            return dict.TryGetValue(key, out value);
        }

        public bool TryGetHeader(string key, out string value) {
            var result = Headers.TryGetValues(key, out IEnumerable<string> values);
            value = values.FirstOrDefault();
            return result;
        }


    }
}