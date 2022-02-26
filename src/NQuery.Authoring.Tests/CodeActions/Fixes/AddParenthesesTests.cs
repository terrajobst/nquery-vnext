using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Fixes;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Fixes
{
    public class AddParenthesesTests : CodeFixTests
    {
        protected override ICodeFixProvider CreateProvider()
        {
            return new AddParenthesesCodeFixProvider();
        }

        [Fact]
        public void AddParentheses_ToNameExpression()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.Address.StartsWith|
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.Address.StartsWith()
            ";

            AssertFixes(query, fixedQuery, "Add parentheses");
        }

        [Fact]
        public void AddParentheses_ToPropertyExpression()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   GETDATE| = e.Birthdate
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   GETDATE() = e.Birthdate
            ";

            AssertFixes(query, fixedQuery, "Add parentheses");
        }
    }
}