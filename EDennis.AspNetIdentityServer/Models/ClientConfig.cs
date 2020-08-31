using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace EDennis.AspNetIdentityServer {
    public class ClientConfig : Client {

        private string _plainTextSecret;

        public string PlainTextSecret {
            get => _plainTextSecret; 
            set {
                _plainTextSecret = value;
                if(_plainTextSecret != null && _plainTextSecret.Length > 0)
                    ClientSecrets = new List<Secret> { new Secret(_plainTextSecret.Sha256()) };
            } 
        }

        public string[] _applications;

        public string[] Applications {
            get => _applications;
            set {
                _applications = value;
                if (_applications != null && _applications.Length > 0)
                    Properties = _applications.ToDictionary(a => a, a => "ApplicationResource");
            }
        }

    }
}
