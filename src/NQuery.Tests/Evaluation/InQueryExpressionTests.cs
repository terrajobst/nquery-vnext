using Xunit;

namespace NQuery.Tests.Evaluation
{
    public class InQueryExpressionTests : EvaluationTest
    {
        [Fact]
        public void Evaluation_InQueryExpression()
        {
            var text = @"
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID IN (SELECT 1 UNION ALL SELECT 8)
            ";

            var expected = new[] { 1, 8 };

            AssertProduces(text, expected);
        }

        [Fact]
        public void Evaluation_InQueryExpression_Negated()
        {
            var text = @"
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID NOT IN (SELECT 1 UNION ALL SELECT 8)
            ";

            var expected = new[] { 2, 3, 4, 5, 6, 7, 9 };

            AssertProduces(text, expected);
        }
    }
}
