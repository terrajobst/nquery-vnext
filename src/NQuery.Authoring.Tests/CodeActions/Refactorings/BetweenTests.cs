using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class BetweenTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new BetweenCodeRefactoringProvider();
        }

        [Fact]
        public void Between_DoesNotTrigger_ForDifferentExpressions()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   6 <= e.EmployeeID
                AND|     e.ReportsTo <= 7
                AND     e.City = 'London'
            ";

            AssertDoesNotTrigger(text);
        }

        [Fact]
        public void Between_DoesNotTrigger_ForWrongOperator()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   6 < e.EmployeeID
                AND|     e.EmployeeID < 7
                AND     e.City = 'London'
            ";

            AssertDoesNotTrigger(text);
        }

        [Fact]
        public void Between_DoesNotTrigger_ForWrongSides()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   6 <= e.EmployeeID
                AND|    7 <= e.EmployeeID
                AND     e.City = 'London'
            ";

            AssertDoesNotTrigger(text);
        }

        [Fact]
        public void Between_Replaces_LowerExpressionExpressionUpper()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   6 <= e.EmployeeID
                AND|     e.EmployeeID <= 7
                AND     e.City = 'London'
            ";

            const string expected = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID BETWEEN 6 AND 7
                AND     e.City = 'London'
            ";

            const string diagnostic = "Replace with BETWEEN";

            AssertFixes(text, expected, diagnostic);
        }

        [Fact]
        public void Between_Replaces_ExpressionLowerExpressionUpper()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID >= 6
                AND|     e.EmployeeID <= 7
                AND     e.City = 'London'
            ";

            const string expected = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID BETWEEN 6 AND 7
                AND     e.City = 'London'
            ";

            const string diagnostic = "Replace with BETWEEN";

            AssertFixes(text, expected, diagnostic);
        }

        [Fact]
        public void Between_Replaces_ExpressionLowerUpperExpression()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID >= 6
                AND|     7 >= e.EmployeeID
                AND     e.City = 'London'
            ";

            const string expected = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID BETWEEN 6 AND 7
                AND     e.City = 'London'
            ";

            const string diagnostic = "Replace with BETWEEN";

            AssertFixes(text, expected, diagnostic);
        }

        [Fact]
        public void Between_Replaces_UpperExpressionExpressionLower()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                WHERE   7 >= e.EmployeeID
                AND|     e.EmployeeID >= 6
                AND     e.City = 'London'
            ";

            const string expected = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeID BETWEEN 6 AND 7
                AND     e.City = 'London'
            ";

            const string diagnostic = "Replace with BETWEEN";

            AssertFixes(text, expected, diagnostic);
        }
    }
}