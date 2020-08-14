namespace EDennis.Samples.ColorApp {
    public static class Expr {
        public static string Build(SearchOperator so, string operandName, int? operandValue) {
            return so switch
            {
                SearchOperator.Equals => $"{operandName} eq {operandValue}",
                SearchOperator.StartsWithOrLT => $"{operandName} lt {operandValue}",
                SearchOperator.ContainsOrGE => $"{operandName} ge {operandValue}",
                _ => default,
            };
        }
        public static string Build(SearchOperator so, string operandName, string operandValue) {
            return so switch
            {
                SearchOperator.Equals => $"{operandName}.Equals(\"{operandValue}\",StringComparison.OrdinalIgnoreCase)",
                SearchOperator.StartsWithOrLT => $"{operandName}.StartsWith(\"{operandValue}\",StringComparison.OrdinalIgnoreCase)",
                SearchOperator.ContainsOrGE => $"{operandName}.Contains(\"{operandValue}\",StringComparison.OrdinalIgnoreCase)",
                _ => default,
            };
        }
    }
}
