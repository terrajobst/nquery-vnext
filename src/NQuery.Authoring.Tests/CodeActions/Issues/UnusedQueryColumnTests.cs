using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.Tests.CodeActions.Issues
{
    public class UnusedQueryColumnTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new UnusedQueryColumnCodeIssueProvider();
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression()
        {
            var query = @"
                SELECT  d.FirstName + ' ' + d.LastName AS Name
                FROM    (
                            SELECT  e.EmployeeId,
                                    e.FirstName,
                                    e.LastName
                            FROM    Employees e
                            WHERE   e.City = 'London'
                        ) d
            ";

            var issues = GetIssues(query);

            Assert.Single(issues);
            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal("e.EmployeeId", query.Substring(issues[0].Span));
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_UnlessRooted()
        {
            var query = @"
                SELECT  e.EmployeeId,
                        e.FirstName,
                        e.LastName
                FROM    Employees e
                WHERE   e.City = 'London'
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_UnlessReferenced()
        {
            var query = @"
                SELECT  d.FirstName + ' ' + d.LastName AS Name
                FROM    (
                            SELECT  e.EmployeeId,
                                    e.FirstName,
                                    e.LastName
                            FROM    Employees e
                            WHERE   e.City = 'London'
                        ) d
                WHERE   d.EmployeeId > 10
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_InCTE()
        {
            var query = @"
                WITH LondonEmployees AS (
                    SELECT  e.EmployeeId,
                            e.FirstName,
                            e.LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  d.FirstName + ' ' + d.LastName AS Name
                FROM    LondonEmployees AS d
            ";

            var issues = GetIssues(query);

            Assert.Single(issues);
            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal("e.EmployeeId", query.Substring(issues[0].Span));
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_InCTE_UnlessWildcardIncluded()
        {
            var query = @"
                WITH UnionEmployees AS (
                    SELECT  e1.EmployeeID,
                            e1.LastName,
                            e1.FirstName
                    FROM    Employees e1
                    WHERE   City = 'Redmond'

                    UNION   ALL

                    SELECT  e2.EmployeeID,
                            e2.LastName,
                            e2.FirstName
                    FROM    Employees e2
                    WHERE   City = 'London'
                )
                SELECT  *
                FROM    UnionEmployees
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }

        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_InCTE_UnlessReferenced()
        {
            var query = @"
                WITH LondonEmployees AS (
                    SELECT  e.EmployeeId,
                            e.FirstName,
                            e.LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  d.FirstName + ' ' + d.LastName AS Name
                FROM    LondonEmployees AS d
                WHERE   d.EmployeeId > 1
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }
        
        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_InUnion()
        {
            var query = @"
                SELECT  D.EmployeeID
                FROM    (
                            SELECT  e1.EmployeeId,
                                    e1.FirstName
                            FROM    Employees e1
                            UNION ALL
                            SELECT  e2.EmployeeId,
                                    e2.FirstName
                            FROM    Employees e2
                        ) AS D
            ";

            var issues = GetIssues(query);

            Assert.Equal(2, issues.Length);

            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal("e1.FirstName", query.Substring(issues[0].Span));
            
            Assert.Equal(CodeIssueKind.Unnecessary, issues[1].Kind);
            Assert.Equal("e2.FirstName", query.Substring(issues[1].Span));
        }
        
        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_InUnion_UnlessNotUnionAll()
        {
            var query = @"
                SELECT  D.EmployeeID
                FROM    (
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                            UNION
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                        ) AS D
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }
        
        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_UnlessInExcept()
        {
            var query = @"
                SELECT  D.EmployeeID
                FROM    (
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                            EXCEPT
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                        ) AS D
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }
        
        [Fact]
        public void UnusedQueryColumn_FindsSelectExpression_UnlessInIntersect()
        {
            var query = @"
                SELECT  D.EmployeeID
                FROM    (
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                            INTERSECT
                            SELECT  e.EmployeeID,
                                    e.FirstName
                            FROM    Employees e
                        ) AS D
            ";

            var issues = GetIssues(query);

            Assert.Empty(issues);
        }
    }
}