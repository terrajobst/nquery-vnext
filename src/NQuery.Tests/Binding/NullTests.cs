using System.Collections.Immutable;

using NQuery.Syntax;

namespace NQuery.Tests.Binding
{
    public class NullTests
    {
        [Fact]
        public void Null_SelectNullLiteral_IsTypedAsNull()
        {
            var text = "SELECT NULL";
            var compilation = CompilationFactory.CreateQuery(text);
            var syntaxTree = compilation.SyntaxTree;
            var semanticModel = compilation.GetSemanticModel();
            var query = (SelectQuerySyntax) syntaxTree.Root.Root;
            var columns = semanticModel.GetOutputColumns(query).ToImmutableArray();

            Assert.Single(columns);
            Assert.True(columns[0].Type.IsNull());
            Assert.Equal(string.Empty, columns[0].Name);
        }

        [Fact]
        public void Null_SelectNullExpression_IsTyped()
        {
            var text = "SELECT 1.0 + NULL";
            var compilation = CompilationFactory.CreateQuery(text);
            var syntaxTree = compilation.SyntaxTree;
            var semanticModel = compilation.GetSemanticModel();
            var query = (SelectQuerySyntax)syntaxTree.Root.Root;
            var columns = semanticModel.GetOutputColumns(query).ToImmutableArray();

            Assert.Single(columns);
            Assert.Equal(typeof(double), columns[0].Type);
            Assert.Equal(string.Empty, columns[0].Name);
        }

        [Fact]
        public void Null_ImplicitConversionsFail_WhenAmbiguous()
        {
            var compilation = CompilationFactory.CreateQuery("SELECT ROUND(NULL)");
            var syntaxTree = compilation.SyntaxTree;
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.AmbiguousInvocation, diagnostics[0].DiagnosticId);
        }

        // TODO: Add this test case
        //SELECT *
        //FROM   Employees
        //WHERE  NULL --> Expect no errors

        // TODO: Add this test case
        //SELECT *
        //FROM   Employees e
        //            INNER JOIN EmployeeTerritories et ON NULL --> Expect no errors

        // TODO: Add this test case
        //SELECT 1, 2, 3
        //FROM   Employees
        //ORDER  BY NULL --> A constant expression was encountered in the ORDER BY list.

        // TODO: Add this test case
        //SELECT 1, 2, 3
        //FROM   Employees
        //GROUP  BY NULL  --> Each GROUP BY expression must contain at least one column that is not an outer reference.

        // TODO: Add this test case
        //SELECT SUM(EmployeeId)
        //FROM   Employees e
        //HAVING NULL --> Expect no errors
    }
}