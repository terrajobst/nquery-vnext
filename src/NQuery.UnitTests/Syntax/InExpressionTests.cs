using System;
using System.Collections.Immutable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Syntax
{
    [TestClass]
    public class InExpressionTests
    {
        [TestMethod]
        public void In_DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN ()");
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }

}