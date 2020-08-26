using Microsoft.AspNetCore.Identity;

namespace EDennis.NetStandard.Base {
    public class NoEffectNormalizer : ILookupNormalizer {
        public string NormalizeEmail(string email) => email;

        public string NormalizeName(string name) => name;
    }
}
