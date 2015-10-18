using System;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class RemoveRedundantParenthesisTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new RemoveRedundantParenthesisCodeRefactoringProvider();
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_IsPropertyAccess()
        {
            var query = @"
                SELECT  |(e.FirstName + ' ' + e.LastName).Length
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_IsMethodInvocation()
        {
            var query = @"
                SELECT  |(e.FirstName + ' ' + e.LastName).ToString()
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_IsNullCheck()
        {
            var query = @"
                SELECT  |(e.FirstName + ' ' + e.LastName) IS NULL
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_BindsStronger_Binary()
        {
            var query = @"
                SELECT  2 * |(3 + 4)
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_BindsStronger_Unary()
        {
            var query = @"
                SELECT  - |(3 + 4)
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_BindsStronger_Ternary()
        {
            var query = @"
                SELECT  |(1 & 2) BETWEEN 2 AND 3
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_DoesNotTrigger_WhenParent_BindsSame_ButOnRight()
        {
            var query = @"
                SELECT  2 + |(3 + 4)
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenParent_IsNoExpression()
        {
            var query = @"
                SELECT  |(2 + 3 * 4)
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  2 + 3 * 4
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenParent_BindsWeaker_Binary()
        {
            var query = @"
                SELECT  2 + |(3 * 4)
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  2 + 3 * 4
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenParent_BindsWeaker_Unary()
        {
            var query = @"
                SELECT  NOT |(3 = 4)
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  NOT 3 = 4
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenParent_BindsWeaker_Ternary()
        {
            var query = @"
                SELECT  |(1 + 2) BETWEEN 3 AND 4
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  1 + 2 BETWEEN 3 AND 4
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenParent_BindsSame_AndOnLeft()
        {
            var query = @"
                SELECT  |(2 + 3) + 4
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  2 + 3 + 4
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }

        [Fact]
        public void RemoveRedundantParenthesis_Triggers_WhenChild_IsPrimary()
        {
            var query = @"
                SELECT  |(2) + 3
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  2 + 3
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant parenthesis");
        }
    }
}