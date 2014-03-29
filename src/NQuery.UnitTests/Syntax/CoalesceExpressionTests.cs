using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Syntax
{
    [TestClass]
    public class CoalesceExpressionTests
    {
        [TestMethod]
        public void Coalesce_DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE()");
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();
            Assert.AreEqual(3, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[2].DiagnosticId);
        }

        [TestMethod]
        public void Coalesce_DetectsTooFewArguments_WhenSingleArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1)");
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();
            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);
        }
    }
}