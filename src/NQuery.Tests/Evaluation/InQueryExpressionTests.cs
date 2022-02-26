namespace NQuery.Tests.Evaluation
{
    public class InQueryExpressionTests : EvaluationTest
    {
        [Fact]
        public void Evaluation_InQueryExpression()
        {
            var text = @"
                SELECT	e.EmployeeID,
                        e.ReportsTo
                FROM	Employees e
                WHERE	e.ReportsTo IN (SELECT RegionID FROM Region)
            ";

            var expected = new[] {
                (1, 2),
                (3, 2),
                (4, 2),
                (5, 2),
                (8, 2)
            };

            AssertProduces(text, expected);
        }

        [Fact]
        public void Evaluation_InQueryExpression_Negated()
        {
            var text = @"
                SELECT	e.EmployeeID,
                        e.ReportsTo
                FROM	Employees e
                WHERE	e.ReportsTo NOT IN (SELECT RegionID FROM Region)
            ";

            var expected = new[] {
                (6, 5),
                (7, 5),
                (9, 5)
            };

            AssertProduces(text, expected);
        }
    }
}
