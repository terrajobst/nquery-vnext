using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public class DerivedTableTests
    {
        [Fact]
        public void DerivedTables_CannotBindToRowObject()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT D FROM (SELECT 'foo') AS D");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidRowReference, diagnostics[0].DiagnosticId);
        }
    }
}