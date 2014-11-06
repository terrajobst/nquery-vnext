using System;
using System.Collections.Immutable;

using Xunit;

namespace NQuery.UnitTests.Syntax
{
    public class InExpressionTests
    {
        [Fact]
        public void In_DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN ()");
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }

}