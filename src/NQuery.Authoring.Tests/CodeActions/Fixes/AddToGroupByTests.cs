using System;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Fixes;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Fixes
{
    public class AddToGroupByTests : CodeFixTests
    {
        protected override ICodeFixProvider CreateProvider()
        {
            return new AddToGroupByCodeFixProvider();
        }

        [Fact]
        public void AddToGroupBy_InsertsExpressionFromSelect()
        {
            var query = @"
                SELECT  e.City| + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City
            ";

            AssertFixes(query, fixedQuery, "Add 'e.City' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertsColumnFromSelect()
        {
            var query = @"
                SELECT  e.City| + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City + ', ' + e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.City + ', ' + e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertsExpressionFromHaving()
        {
            var query = @"
                SELECT  COUNT(*)
                FROM    Employees e
                HAVING  e.City| = 'London'
            ";

            var fixedQuery = @"
                SELECT  COUNT(*)
                FROM    Employees e
                GROUP   BY e.City
                HAVING  e.City = 'London'
            ";

            AssertFixes(query, fixedQuery, "Add 'e.City' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_DoesNotInsertColumnFromHaving()
        {
            var query = @"
                SELECT  COUNT(*)
                FROM    Employees e
                HAVING  e.City| = 'London'
            ";

            AssertDoesNotTrigger(query, "Add 'e.City = 'London'' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertsExpressionFromOrderBy()
        {
            var query = @"
                SELECT  COUNT(*)
                FROM    Employees e
                ORDER   BY e.City + ', ' + e.Country|
            ";

            var fixedQuery = @"
                SELECT  COUNT(*)
                FROM    Employees e
                GROUP   BY e.Country
                ORDER   BY e.City + ', ' + e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertsColumnFromOrderBy()
        {
            var query = @"
                SELECT  COUNT(*)
                FROM    Employees e
                ORDER   BY e.City + ', ' + e.Country|
            ";

            var fixedQuery = @"
                SELECT  COUNT(*)
                FROM    Employees e
                GROUP   BY e.City + ', ' + e.Country
                ORDER   BY e.City + ', ' + e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.City + ', ' + e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertReusesGroupBy()
        {
            var query = @"
                SELECT  e.City + ', ' + e.Country|,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertReusesTrailingComma()
        {
            var query = @"
                SELECT  e.City + ', ' + e.Country|,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City,
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City, e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertCommaAfterIncompleteExpression()
        {
            var query = @"
                SELECT  e.City + ', ' + e.Country|,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City.Substring(1
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY e.City.Substring(1, e.Country
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertAfterIncompleteFrom()
        {
            var query = @"
                SELECT  e.City + ', ' + e.Country|,
                        COUNT(*)
                FROM    Employees e
                            INNER JOIN
                ORDER   BY 1
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                            INNER JOIN
                GROUP   BY e.Country
                ORDER   BY 1
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }

        [Fact]
        public void AddToGroupBy_InsertAfterIncompleteWere()
        {
            var query = @"
                SELECT  e.City + ', ' + e.Country|,
                        COUNT(*)
                FROM    Employees e
                WHERE
                ORDER   BY 1
            ";

            var fixedQuery = @"
                SELECT  e.City + ', ' + e.Country,
                        COUNT(*)
                FROM    Employees e
                WHERE
                GROUP   BY e.Country
                ORDER   BY 1
            ";

            AssertFixes(query, fixedQuery, "Add 'e.Country' to GROUP BY");
        }
    }
}