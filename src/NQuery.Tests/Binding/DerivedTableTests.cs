using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Binding
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

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidRowReference, diagnostics[0].DiagnosticId);
        }
    }
}