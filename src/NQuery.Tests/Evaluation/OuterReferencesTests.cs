namespace NQuery.Tests.Evaluation
{
    public class OuterReferencesTests : EvaluationTest
    {
        [Fact]
        public void OuterReferences_AreNot_Removed()
        {
            var text = @"
                SELECT  (
                            SELECT  COUNT(*)
                            FROM    EmployeeTerritories et
                            WHERE   et.EmployeeID = e.EmployeeID
                        ) AS TerritoryCount
                FROM    Employees e
            ";

            var expected = new[] {
                2,
                7,
                4,
                3,
                7,
                5,
                10,
                4,
                7
            };

            AssertProduces(text, expected);
        }
    }
}