using System;

namespace NQuery.UnitTests
{
    partial class IntrinsicOperatorTests
    {
        private sealed class UnaryOperatorTestCase
        {
            private readonly string _op;
            private readonly string _argument;
            private readonly string _expectedResult;

            public UnaryOperatorTestCase(string op, string argument, string expectedResult)
            {
                _op = op;
                _argument = argument;
                _expectedResult = expectedResult;
            }

            public string Op
            {
                get { return _op; }
            }

            public string Argument
            {
                get { return _argument; }
            }

            public string ExpectedResult
            {
                get { return _expectedResult; }
            }
        }

        private static readonly UnaryOperatorTestCase[] UnaryTestCases = new[]
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