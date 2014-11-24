using System;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Fixes;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Fixes
{
    public class AddOrderByToSelectDistinctTests : CodeFixTests
    {
        protected override ICodeFixProvider CreateProvider()
        {
            return new AddOrderByToSelectDistinctCodeFixProvider();
        }

        [Fact]
        public void AddOrderByToSelectDistinct_InsertsExpression()
        {
            var query = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName
                FROM    Employees e
                ORDER   BY e.BirthDate|
            ";

            var fixedQuery = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName, e.BirthDate
                FROM    Employees e
                ORDER   BY e.BirthDate
            ";

            AssertFixes(query, fixedQuery, "Add e.BirthDate to SELECT list");
        }
    }
}