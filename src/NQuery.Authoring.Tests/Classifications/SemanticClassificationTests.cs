using System.Collections.Immutable;

using NQuery.Authoring.Classifications;
using NQuery.Symbols;

namespace NQuery.Authoring.Tests.Classifications
{
    public class SemanticClassificationTests
    {
        [Fact]
        public void SemanticClassification_Classifies()
        {
            var query = @"
                WITH Emps AS (
                    SELECT  e.*
                    FROM    Employees e
                )
                SELECT  COUNT(*),
                        SUM(d.EmployeeId)
                FROM    (
                            SELECT *,
                                   FirstName + ' ' + LastName AS FullName
                            FROM Emps
                        ) d
                WHERE   d.ReportsTo = @Manager
                AND     LEN(LastName) = LastName.Length
                AND     LastName.Substring(0, ReportsTo) = '2'
            ";

            var pieces = new (string Text, SemanticClassification Classification)[]
                         {
                             ("Emps", SemanticClassification.CommonTableExpression),
                             ("e", SemanticClassification.SchemaTable),
                             ("Employees", SemanticClassification.SchemaTable),
                             ("e", SemanticClassification.SchemaTable),
                             ("COUNT", SemanticClassification.Aggregate),
                             ("SUM", SemanticClassification.Aggregate),
                             ("d", SemanticClassification.DerivedTable),
                             ("EmployeeId", SemanticClassification.Column),
                             ("FirstName", SemanticClassification.Column),
                             ("LastName", SemanticClassification.Column),
                             ("FullName", SemanticClassification.Column),
                             ("Emps", SemanticClassification.CommonTableExpression),
                             ("d", SemanticClassification.DerivedTable),
                             ("d", SemanticClassification.DerivedTable),
                             ("ReportsTo", SemanticClassification.Column),
                             ("@Manager", SemanticClassification.Variable),
                             ("LEN", SemanticClassification.Function),
                             ("LastName", SemanticClassification.Column),
                             ("LastName", SemanticClassification.Column),
                             ("Length", SemanticClassification.Property),
                             ("LastName", SemanticClassification.Column),
                             ("Substring", SemanticClassification.Method),
                             ("ReportsTo", SemanticClassification.Column),
                         };

            var compilation = CompilationFactory.CreateQuery(query);
            var dataContext = compilation.DataContext.AddVariables(new VariableSymbol("Manager", typeof(int)));
            compilation = compilation.WithDataContext(dataContext);

            var syntaxTree = compilation.SyntaxTree;
            var semanticModel = compilation.GetSemanticModel();
            var classificationSpans = syntaxTree.Root.ClassifySemantics(semanticModel).ToImmutableArray();

            Assert.Equal(pieces.Length, classificationSpans.Length);

            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                var pieceText = piece.Text;
                var classification = piece.Classification;
                var classificationText = query.Substring(classificationSpans[i].Span);

                Assert.Equal(pieceText, classificationText);
                Assert.Equal(classification, classificationSpans[i].Classification);
            }
        }
    }
}