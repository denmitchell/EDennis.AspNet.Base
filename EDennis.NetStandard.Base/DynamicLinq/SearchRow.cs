using System.Collections.Generic;

namespace EDennis.NetStandard.Base {


    public class SearchRow<TEntity> where TEntity : class {
        public string FieldName { get; set; }
        public ComparisonOperator Operator { get; set; }
        public string FieldValue { get; set; }

        public string Expression {
            get {
                if (FieldValue == null)
                    return "";

                var isStringProp = StringPropertyAccessorComponentCache<TEntity>.IsStringProperty(FieldName);

                return Operator switch
                {
                    ComparisonOperator.Equals => $"{FieldName} eq {EnsureQuotes(FieldValue,isStringProp)}",
                    ComparisonOperator.In => $"{FieldName} in ({EnsureQuotesInList(FieldValue,isStringProp)})",
                    ComparisonOperator.NotIn => $"not({FieldName} in ({EnsureQuotesInList(FieldValue, isStringProp)}))",
                    ComparisonOperator.LessThan => $"{FieldName} lt {FieldValue}",
                    ComparisonOperator.LessOrEqual => $"{FieldName} le {FieldValue}",
                    ComparisonOperator.GreaterThan => $"{FieldName} gt {FieldValue}",
                    ComparisonOperator.GreaterOrEqual => $"{FieldName} ge {FieldValue}",
                    //quotes not needed with below items, as they use my extension methods
                    ComparisonOperator.StartsWith => $"{FieldName}.StartsWith({FieldValue})",
                    ComparisonOperator.EndsWith => $"{FieldName}.EndsWith({FieldValue})",
                    ComparisonOperator.Contains => $"{FieldName}.Contains({FieldValue})",
                    ComparisonOperator.Like => $"{FieldName}.Like({FieldValue})",
                    _ => default,
                };

            }
        }

        private static string EnsureQuotes(string str, bool isStringProp) {
            if (!isStringProp)
                return str;
            if (str.StartsWith("\"") && str.EndsWith("\""))
                return str;
            else
                return "\"" + str + "\"";
        }

        private static string EnsureQuotesInList(string strList, bool isStringProp) {
            if (!isStringProp)
                return strList;
            if (strList.StartsWith("\"") && strList.EndsWith("\""))
                return strList;
            else
                return "\"" + string.Join("\",\"", strList.Split(",")) + "\""; 
        }



    }
}
