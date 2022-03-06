namespace NQuery.Tests.Binding
{
    partial class IntrinsicOperatorTests
    {
        private sealed class UnaryOperatorTestCase
        {
            public UnaryOperatorTestCase(string op, string argument, string expectedResult)
            {
                Op = op;
                Argument = argument;
                ExpectedResult = expectedResult;
            }

            public string Op { get; }

            public string Argument { get; }

            public string ExpectedResult { get; }
        }

        private static readonly UnaryOperatorTestCase[] UnaryTestCases =
        {
            new("+", "sbyte", "int"),
            new("+", "byte", "int"),
            new("+", "short", "int"),
            new("+", "ushort", "int"),
            new("+", "int", "int"),
            new("+", "uint", "uint"),
            new("+", "long", "long"),
            new("+", "ulong", "ulong"),
            new("+", "char", "int"),
            new("+", "float", "float"),
            new("+", "double", "double"),
            new("+", "decimal", "decimal"),
            new("+", "bool", "#inapplicable"),
            new("+", "string", "#inapplicable"),
            new("+", "object", "#inapplicable"),
            new("-", "sbyte", "int"),
            new("-", "byte", "int"),
            new("-", "short", "int"),
            new("-", "ushort", "int"),
            new("-", "int", "int"),
            new("-", "uint", "long"),
            new("-", "long", "long"),
            new("-", "ulong", "#ambiguous"), // TODO: C# actually marks it as #inapplicable
            new("-", "char", "int"),
            new("-", "float", "float"),
            new("-", "double", "double"),
            new("-", "decimal", "decimal"),
            new("-", "bool", "#inapplicable"),
            new("-", "string", "#inapplicable"),
            new("-", "object", "#inapplicable"),
            new("~", "sbyte", "int"),
            new("~", "byte", "int"),
            new("~", "short", "int"),
            new("~", "ushort", "int"),
            new("~", "int", "int"),
            new("~", "uint", "uint"),
            new("~", "long", "long"),
            new("~", "ulong", "ulong"),
            new("~", "char", "int"),
            new("~", "float", "#inapplicable"),
            new("~", "double", "#inapplicable"),
            new("~", "decimal", "#inapplicable"),
            new("~", "bool", "#inapplicable"),
            new("~", "string", "#inapplicable"),
            new("~", "object", "#inapplicable")
       };
    }
}