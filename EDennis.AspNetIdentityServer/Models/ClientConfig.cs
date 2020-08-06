using IdentityServer4.Models;
using System.Collections.Generic;

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


    }
}
