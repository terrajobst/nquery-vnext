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
            new UnaryOperatorTestCase("+", "sbyte", "int"),
            new UnaryOperatorTestCase("+", "byte", "int"),
            new UnaryOperatorTestCase("+", "short", "int"),
            new UnaryOperatorTestCase("+", "ushort", "int"),
            new UnaryOperatorTestCase("+", "int", "int"),
            new UnaryOperatorTestCase("+", "uint", "uint"),
            new UnaryOperatorTestCase("+", "long", "long"),
            new UnaryOperatorTestCase("+", "ulong", "ulong"),
            new UnaryOperatorTestCase("+", "char", "int"),
            new UnaryOperatorTestCase("+", "float", "float"),
            new UnaryOperatorTestCase("+", "double", "double"),
            new UnaryOperatorTestCase("+", "decimal", "decimal"),
            new UnaryOperatorTestCase("+", "bool", "#inapplicable"),
            new UnaryOperatorTestCase("+", "string", "#inapplicable"),
            new UnaryOperatorTestCase("+", "object", "#inapplicable"),
            new UnaryOperatorTestCase("-", "sbyte", "int"),
            new UnaryOperatorTestCase("-", "byte", "int"),
            new UnaryOperatorTestCase("-", "short", "int"),
            new UnaryOperatorTestCase("-", "ushort", "int"),
            new UnaryOperatorTestCase("-", "int", "int"),
            new UnaryOperatorTestCase("-", "uint", "long"),
            new UnaryOperatorTestCase("-", "long", "long"),
            new UnaryOperatorTestCase("-", "ulong", "#ambiguous"), // TODO: C# actually marks it as #inapplicable
            new UnaryOperatorTestCase("-", "char", "int"),
            new UnaryOperatorTestCase("-", "float", "float"),
            new UnaryOperatorTestCase("-", "double", "double"),
            new UnaryOperatorTestCase("-", "decimal", "decimal"),
            new UnaryOperatorTestCase("-", "bool", "#inapplicable"),
            new UnaryOperatorTestCase("-", "string", "#inapplicable"),
            new UnaryOperatorTestCase("-", "object", "#inapplicable"),
            new UnaryOperatorTestCase("~", "sbyte", "int"),
            new UnaryOperatorTestCase("~", "byte", "int"),
            new UnaryOperatorTestCase("~", "short", "int"),
            new UnaryOperatorTestCase("~", "ushort", "int"),
            new UnaryOperatorTestCase("~", "int", "int"),
            new UnaryOperatorTestCase("~", "uint", "uint"),
            new UnaryOperatorTestCase("~", "long", "long"),
            new UnaryOperatorTestCase("~", "ulong", "ulong"),
            new UnaryOperatorTestCase("~", "char", "int"),
            new UnaryOperatorTestCase("~", "float", "#inapplicable"),
            new UnaryOperatorTestCase("~", "double", "#inapplicable"),
            new UnaryOperatorTestCase("~", "decimal", "#inapplicable"),
            new UnaryOperatorTestCase("~", "bool", "#inapplicable"),
            new UnaryOperatorTestCase("~", "string", "#inapplicable"),
            new UnaryOperatorTestCase("~", "object", "#inapplicable")
       };
    }
}