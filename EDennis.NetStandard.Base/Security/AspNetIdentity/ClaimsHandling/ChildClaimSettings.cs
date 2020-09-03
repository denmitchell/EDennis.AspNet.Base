using System.Collections.Generic;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Flexible, compact settings for ChildClaims as used
    /// by a ChildClaimCache.
    /// </summary>
    public class ChildClaimSettings {
        public string ParentType { get; set; }
        public string ChildType { get; set; }
        public string[][] Data { get; set; }

        public IEnumerable<ChildClaim> GetChildClaims() {
            var list = new List<ChildClaim>();
            var offset1 = (ParentType == null ? 0 : -1);
            var offset2 = offset1 + (ChildType == null ? 0 : -1);
            for (int i = 0; i < Data.Length; i++) {
                list.Add(
                    new ChildClaim {
                        ParentType = ParentType ?? Data[i][0],
                        ParentValue = Data[i][1 + offset1],
                        ChildType = ChildType ?? Data[i][2 + offset1],
                        ChildValue = Data[i][3 + offset2]
                    });
             }
            return list;
        }
    }
}
