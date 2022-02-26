using Xunit;

namespace NQuery.Tests.Syntax
{
    partial class ParserTests
    {
        [Fact]
        public void Parser_Parse_Table_Named_WithoutAlias()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
            }
        }

        [Fact]
        public void Parser_Parse_Table_Named_WithAlias()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
            }
        }

        [Fact]
        public void Parser_Parse_Table_CrossJoined()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees
                            CROSS JOIN EmployeeTerritories}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CrossJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertToken(SyntaxKind.CrossKeyword, @"CROSS");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeTerritories");
            }
        }

        [Fact]
        public void Parser_Parse_Table_InnerJoined_WithJoinKeyword()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e
                            INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.InnerJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.InnerKeyword, @"INNER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeTerritories");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
            }
        }

        [Fact]
        public void Parser_Parse_Table_InnerJoined_WithInnerJoinKeywords()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e
                            JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.InnerJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeTerritories");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
            }
        }

        [Fact]
        public void Parser_Parse_Table_InnerJoined_WithComplexStructure()
        {
            const string text = @"
                SELECT  *
                FROM        {(Employees e
                                INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID)
                        INNER JOIN
                            (Territories t
                                INNER JOIN Region r ON r.RegionID = t.RegionID)
                        ON et.TerritoryID = t.TerritoryID}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.InnerJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.ParenthesizedTableReference);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.InnerJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.InnerKeyword, @"INNER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeTerritories");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertToken(SyntaxKind.InnerKeyword, @"INNER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.ParenthesizedTableReference);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.InnerJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Territories");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"t");
                enumerator.AssertToken(SyntaxKind.InnerKeyword, @"INNER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Region");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"r");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"r");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"RegionID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"t");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"RegionID");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"et");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"TerritoryID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"t");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"TerritoryID");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithLeftKeyword()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            LEFT JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.LeftKeyword, @"LEFT");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithLeftOuterKeywords()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            LEFT OUTER JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.LeftKeyword, @"LEFT");
                enumerator.AssertToken(SyntaxKind.OuterKeyword, @"OUTER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithRightKeyword()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            RIGHT JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.RightKeyword, @"RIGHT");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithRightOuterKeywords()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            RIGHT OUTER JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.RightKeyword, @"RIGHT");
                enumerator.AssertToken(SyntaxKind.OuterKeyword, @"OUTER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithFullKeyword()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            FULL JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.FullKeyword, @"FULL");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_OuterJoined_WithFullOuterKeywords()
        {
            const string text = @"
                SELECT  *
                FROM    {Employees e1
                            FULL OUTER JOIN Employees e2 ON e2.EmployeeID = e1.ReportsTo}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OuterJoinedTableReference);
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.FullKeyword, @"FULL");
                enumerator.AssertToken(SyntaxKind.OuterKeyword, @"OUTER");
                enumerator.AssertToken(SyntaxKind.JoinKeyword, @"JOIN");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.OnKeyword, @"ON");
                enumerator.AssertNode(SyntaxKind.EqualExpression);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e2");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertToken(SyntaxKind.EqualsToken, @"=");
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e1");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"ReportsTo");
            }
        }

        [Fact]
        public void Parser_Parse_Table_Derived()
        {
            const string text = @"
                SELECT  *
                FROM    {(SELECT * FROM Employees) d}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.DerivedTableReference);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"d");
            }
        }

        [Fact]
        public void Parser_Parse_Table_Derived_WithAsKeyword()
        {
            const string text = @"
                SELECT  *
                FROM    {(SELECT * FROM Employees) AS d}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.DerivedTableReference);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"d");
            }
        }

        [Fact]
        public void Parser_Parse_Table_Parenthesized_WithNamed()
        {
            const string text = @"
                SELECT  *
                FROM    {(Employees)}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ParenthesizedTableReference);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            }
        }
    }
}