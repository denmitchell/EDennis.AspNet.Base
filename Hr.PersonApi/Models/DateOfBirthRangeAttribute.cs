using System;
using System.ComponentModel.DataAnnotations;

namespace Hr.PersonApi.Models {
    public class DateOfBirthRangeAttribute : RangeAttribute {
        public DateOfBirthRangeAttribute()
          : base(typeof(DateTime),
                  DateTime.Today.AddYears(-130).ToShortDateString(),
                  DateTime.Today.ToShortDateString()) { }
    }
}
