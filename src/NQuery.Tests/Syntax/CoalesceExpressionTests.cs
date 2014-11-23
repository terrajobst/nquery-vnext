using System;
using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public class CoalesceExpressionTests
    {
        [Fact]
        public void Coalesce_DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE()");
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();
            Assert.Equal(3, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[2].DiagnosticId);
        }

        [Fact]
        public void Coalesce_DetectsTooFewArguments_WhenSingleArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1)");
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();
            Assert.Equal(2, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);
        }
    }
}