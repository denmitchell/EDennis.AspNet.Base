namespace EDennis.NetStandard.Base {


    public class SearchRow {
        public string FieldName { get; set; }
        public ComparisonOperator Operator { get; set; }
        public string FieldValue { get; set; }

        public string Expression {
            get {
                if (FieldValue == null)
                    return "";

                return Operator switch
                {
                    ComparisonOperator.Equals => $"{FieldName} eq {FieldValue}",
                    ComparisonOperator.In => $"{FieldName} in ({FieldValue.Replace("(", "").Replace(")", "")})",
                    ComparisonOperator.NotIn => $"not({FieldName} in ({FieldValue.Replace("(", "").Replace(")", "")}))",
                    ComparisonOperator.LessThan => $"{FieldName} lt {FieldValue}",
                    ComparisonOperator.LessOrEqual => $"{FieldName} le {FieldValue}",
                    ComparisonOperator.GreaterThan => $"{FieldName} gt {FieldValue}",
                    ComparisonOperator.GreaterOrEqual => $"{FieldName} ge {FieldValue}",
                    ComparisonOperator.StartsWith => $"{FieldName}.StartsWith({FieldValue})",
                    ComparisonOperator.EndsWith => $"{FieldName}.EndsWith({FieldValue})",
                    ComparisonOperator.Contains => $"{FieldName}.Contains({FieldValue})",
                    ComparisonOperator.Like => $"{FieldName}.Like({FieldValue})",
                    _ => default,
                };

            }
        }

    }
}
