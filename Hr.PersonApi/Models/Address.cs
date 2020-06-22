using EDennis.AspNet.Base.Models;
using System.ComponentModel.DataAnnotations;

namespace Hr.PersonApi.Models {

    public class AddressHistory : Address { }
    public class Address : TemporalEntity {

        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "Street address must not exceed 100 characters.")]
        public string StreetAddress { get; set; }

        [StringLength(40, ErrorMessage = "City must not exceed 40 characters.")]
        public string City { get; set; }

        [StringLength(2, MinimumLength = 2,ErrorMessage = "State must be a two-letter abbreviation")]
        public string State { get; set; }

        [RegularExpression("[0-9]{5}", ErrorMessage = "Postal code must be a 5-digit number with leading zeroes, when necessary.")]
        public string PostalCode { get; set; }


        public Person Person { get; set; }
        public int PersonId { get; set; }


    }
}
