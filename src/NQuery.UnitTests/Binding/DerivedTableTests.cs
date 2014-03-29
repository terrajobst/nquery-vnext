using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class DerivedTableTests
    {
        [TestMethod]
        public void DerivedTables_CannotBindToRowObject()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT D FROM (SELECT 'foo') AS D");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidRowReference, diagnostics[0].DiagnosticId);
        }
    }
}