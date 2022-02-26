namespace NQuery.Tests.Evaluation
{
    public class InExpressionTests : EvaluationTest
    {
        [Fact]
        public void Evaluation_InExpression()
        {
            var text = @"
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID IN (1, 2, 3)
            ";

            var expected = new[] { 1, 2, 3 };

            AssertProduces(text, expected);
        }

        [Fact]
        public void Evaluation_InExpression_Negated()
        {
            var text = @"
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID NOT IN (1, 2, 3)
            ";

            var expected = new[] { 4, 5, 6, 7, 8, 9 };

            AssertProduces(text, expected);
        }
    }
}
