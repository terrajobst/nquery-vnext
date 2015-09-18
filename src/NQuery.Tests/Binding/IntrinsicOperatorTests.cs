using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed partial class IntrinsicOperatorTests
    {
        [Fact]
        public void IntrinsicOperator_UnarySignaturesAreCorrect()
        {
            var issues = new List<string>();

            foreach (var testCase in UnaryTestCases)
            {
                var opText = testCase.Op;
                var argument = GetValue(testCase.Argument);
                var source = $"{opText} {argument}";
                var syntaxTree = SyntaxTree.ParseExpression(source);
                var syntaxTreeSource = syntaxTree.Root.ToString();
                if (syntaxTreeSource != source)
                    Assert.True(false, $"Source should have been {syntaxTreeSource} but is {source}");

                var expression = (UnaryExpressionSyntax)syntaxTree.Root.Root;
                var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
                var semanticModel = compilation.GetSemanticModel();

                var argumentType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Expression));
                if (testCase.Argument != argumentType)
                    Assert.True(false, $"Left should be of type '{testCase.Argument}' but has type '{argumentType}");

                var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
                var expressionType = semanticModel.GetExpressionType(expression);
                var result = diagnostic == null
                                 ? GetExpressionTypeString(expressionType)
                                 : GetErrorString(diagnostic.DiagnosticId);

                if (result != testCase.ExpectedResult)
                {
                    var issue = $"Expression {source} should have evaluated to '{testCase.ExpectedResult}' but was '{result}'";
                    issues.Add(issue);
                }
            }

            if (issues.Count > 0)
            {
                issues.Insert(0, $"{issues.Count} errors:");
                var issueText = string.Join(Environment.NewLine, issues);
                Assert.True(false, issueText);
            }
        }

        [Fact]
        public void IntrinsicOperator_BinarySignaturesAreCorrect()
        {
            var issues = new List<string>();

            foreach(var testCase in BinaryTestCases)
            {
                var opText = testCase.Op;
                var left = GetValue(testCase.Left);
                var right = GetValue(testCase.Right);
                var source = $"{left} {opText} {right}";
                var syntaxTree = SyntaxTree.ParseExpression(source);
                var syntaxTreeSource = syntaxTree.Root.ToString();
                if (syntaxTreeSource != source)
                    Assert.True(false, $"Source should have been {syntaxTreeSource} but is {source}");

                var expression = (BinaryExpressionSyntax) syntaxTree.Root.Root;
                var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
                var semanticModel = compilation.GetSemanticModel();

                var leftType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Left));
                if (testCase.Left != leftType)
                    Assert.True(false, $"Left should be of type '{testCase.Left}' but has type '{leftType}");

                var rightType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Right));
                if (testCase.Right != rightType)
                    Assert.True(false, $"Right should be of type '{testCase.Right}' but has type '{rightType}");

                var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
                var expressionType = semanticModel.GetExpressionType(expression);
                var result = diagnostic == null
                                 ? GetExpressionTypeString(expressionType)
                                 : GetErrorString(diagnostic.DiagnosticId);

                if (result != testCase.ExpectedResult)
                {
                    var issue = $"Expression {source} should have evaluated to '{testCase.ExpectedResult}' but was '{result}'";
                    issues.Add(issue);
                }
            }

            if (issues.Count > 0)
            {
                issues.Insert(0, $"{issues.Count} errors:");
                var issueText = string.Join(Environment.NewLine, issues);
                Assert.True(false, issueText);
            }
        }

        private static string GetExpressionTypeString(Type type)
        {
            if (type == typeof(byte))
                return "byte";

            if (type == typeof(sbyte))
                return "sbyte";

            if (type == typeof(char))
                return "char";

            if (type == typeof(short))
                return "short";

            if (type == typeof(ushort))
                return "ushort";

            if (type == typeof(int))
                return "int";

            if (type == typeof(uint))
                return "uint";

            if (type == typeof(long))
                return "long";

            if (type == typeof(ulong))
                return "ulong";

            if (type == typeof(float))
                return "float";

            if (type == typeof(double))
                return "double";

            if (type == typeof(decimal))
                return "decimal";

            if (type == typeof(bool))
                return "bool";

            if (type == typeof(string))
                return "string";

            if (type == typeof(object))
                return "object";

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        private static string GetErrorString(DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.CannotApplyUnaryOperator:
                case DiagnosticId.CannotApplyBinaryOperator:
                    return "#inapplicable";
                case DiagnosticId.AmbiguousUnaryOperator:
                case DiagnosticId.AmbiguousBinaryOperator:
                    return "#ambiguous";
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticId));
            }
        }

        private static string GetValue(string type)
        {
            switch (type)
            {
                case "byte":
                    return "CAST(1 AS byte)";

                case "sbyte":
                    return "CAST(1 AS sbyte)";

                case "char":
                    return "CAST(65 AS char)";

                case "short":
                    return "CAST(1 AS short)";

                case "ushort":
                    return "CAST(1 AS ushort)";

                case "int":
                    return "1";

                case "uint":
                    return "CAST(1 AS uint)";

                case "long":
                    return "CAST(1 AS long)";

                case "ulong":
                    return "CAST(1 AS ulong)";

                case "float":
                    return "CAST(1.0 AS single)";

                case "double":
                    return "1.0";

                case "decimal":
                    return "CAST(1.0 AS decimal)";

                case "bool":
                    return "false";

                case "string":
                    return "'s'";

                case "object":
                    return "CAST(null AS object)";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}