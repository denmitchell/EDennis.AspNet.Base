using EDennis.AspNet.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hr.PersonApi.Models {
    public class PersonHistory : Person { }
    public class Person : TemporalEntity {

        public int Id { get; set; }

        [StringLength(40, ErrorMessage = "First name must not exceed 40 characters.")]
        public string FirstName { get; set; }

        [StringLength(40, ErrorMessage = "Last name must not exceed 40 characters.")]
        public string LastName { get; set; }

        [DateOfBirthRange(ErrorMessage = "Date of birth must not be any earlier than 135 years ago and no later than today.")]
        public DateTime DateOfBirth { get; set; }

        public IEnumerable<Address> Addresses { get; set; }

    }
}
