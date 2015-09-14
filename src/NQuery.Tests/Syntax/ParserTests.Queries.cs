using System;

using Xunit;

namespace NQuery.Tests.Syntax
{
    partial class ParserTests
    {
        [Fact]
        public void Parser_Parse_Query_Alias_WithAsKeyword()
        {
            const string text = @"
                SELECT 1 {AS X}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"X");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Alias_WithoutAsKeyword()
        {
            const string text = @"
                SELECT 1 {X}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"X");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithAllClauses()
        {
            const string text = @"
                SELECT  *
                FROM    Employees
                WHERE   FALSE
                GROUP   BY EmployeeID
                HAVING  TRUE
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertNode(SyntaxKind.WhereClause);
                enumerator.AssertToken(SyntaxKind.WhereKeyword, @"WHERE");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.FalseKeyword, @"FALSE");
                enumerator.AssertNode(SyntaxKind.GroupByClause);
                enumerator.AssertToken(SyntaxKind.GroupKeyword, @"GROUP");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeID");
                enumerator.AssertNode(SyntaxKind.HavingClause);
                enumerator.AssertToken(SyntaxKind.HavingKeyword, @"HAVING");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.TrueKeyword, @"TRUE");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithSelectClause()
        {
            const string text = @"
                SELECT  1
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithFromClause()
        {
            const string text = @"
                SELECT  *
                FROM    Employees
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithWhereClause()
        {
            const string text = @"
                SELECT  *
                WHERE   FALSE
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.WhereClause);
                enumerator.AssertToken(SyntaxKind.WhereKeyword, @"WHERE");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.FalseKeyword, @"FALSE");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithGroupByClause()
        {
            const string text = @"
                SELECT  *
                GROUP   BY x
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.GroupByClause);
                enumerator.AssertToken(SyntaxKind.GroupKeyword, @"GROUP");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Select_WithHavingClause()
        {
            const string text = @"
                SELECT  *
                HAVING  FALSE
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertNode(SyntaxKind.HavingClause);
                enumerator.AssertToken(SyntaxKind.HavingKeyword, @"HAVING");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.FalseKeyword, @"FALSE");
            }
        }

        [Fact]
        public void Parser_Parse_Query_SelectClause()
        {
            const string text = @"
                SELECT x
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            }
        }

        [Fact]
        public void Parser_Parse_Query_SelectClause_WithDistinct()
        {
            const string text = @"
                SELECT DISTINCT x
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertToken(SyntaxKind.DistinctKeyword, @"DISTINCT");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            }
        }

        [Fact]
        public void Parser_Parse_Query_SelectClause_WithTopClause()
        {
            const string text = @"
                SELECT TOP 10 *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_SelectClause_WithMultipleColumns()
        {
            const string text = @"
                SELECT a, b
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"a");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"b");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause()
        {
            const string text = @"
                SELECT {TOP 10} *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithTies()
        {
            const string text = @"
                SELECT {TOP 10 WITH TIES} *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
                enumerator.AssertToken(SyntaxKind.WithKeyword, @"WITH");
                enumerator.AssertToken(SyntaxKind.TiesKeyword, @"TIES");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithTies_WithoutTiesKeyword()
        {
            const string text = @"
                SELECT {TOP 10 WITH }*
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
                enumerator.AssertToken(SyntaxKind.WithKeyword, @"WITH");
                enumerator.AssertTokenMissing(SyntaxKind.TiesKeyword);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found '*' but expected 'TIES'.");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithTies_WithoutWithKeyword()
        {
            const string text = @"
                SELECT {TOP 10 TIES} *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
                enumerator.AssertTokenMissing(SyntaxKind.WithKeyword);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found 'TIES' but expected 'WITH'.");
                enumerator.AssertToken(SyntaxKind.TiesKeyword, @"TIES");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithInvalidLimit_NumericLiteral()
        {
            const string text = @"
                SELECT {TOP 10.3} *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10.3");
                enumerator.AssertDiagnostic(DiagnosticId.InvalidInteger, @"'10.3' is not a valid integer.");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithInvalidLimit_DateLiteral()
        {
            const string text = @"
                SELECT {TOP }#10-10-1998# *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertTokenMissing(SyntaxKind.NumericLiteralToken);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found '#10-10-1998#' but expected '<numeric-literal>'.");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithInvalidLimit_StringLiteral()
        {
            const string text = @"
                SELECT {TOP }'1' *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertTokenMissing(SyntaxKind.NumericLiteralToken);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ''1'' but expected '<numeric-literal>'.");
            }
        }

        [Fact]
        public void Parser_Parse_Query_TopClause_WithMissingLimit()
        {
            const string text = @"
                SELECT {TOP }*
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.TopClause);
                enumerator.AssertToken(SyntaxKind.TopKeyword, @"TOP");
                enumerator.AssertTokenMissing(SyntaxKind.NumericLiteralToken);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found '*' but expected '<numeric-literal>'.");
            }
        }

        [Fact]
        public void Parser_Parse_Query_ExpressionSelectColumn_WithAlias()
        {
            const string text = @"
                SELECT  {1 AS x}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
                enumerator.AssertNode(SyntaxKind.Alias);
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            }
        }

        [Fact]
        public void Parser_Parse_Query_ExpressionSelectColumn_WithoutAlias()
        {
            const string text = @"
                SELECT  {1}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            }
        }

        [Fact]
        public void Parser_Parse_Query_WildcardSelectColumn_WithTableAlias()
        {
            const string text = @"
                SELECT  {t.*}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"t");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_WildcardSelectColumn_WithoutTableAlias()
        {
            const string text = @"
                SELECT  {*}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_FromClause()
        {
            const string text = @"
                SELECT  *
                {FROM    Employees}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
            }
        }

        [Fact]
        public void Parser_Parse_Query_FromClause_WithMultipleTableReferences()
        {
            const string text = @"
                SELECT  *
                {FROM    Employees, EmployeeTerritories}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.FromClause);
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Employees");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.NamedTableReference);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EmployeeTerritories");
            }
        }

        [Fact]
        public void Parser_Parse_Query_WhereClause()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                {WHERE   TRUE}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.WhereClause);
                enumerator.AssertToken(SyntaxKind.WhereKeyword, @"WHERE");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.TrueKeyword, @"TRUE");
            }
        }

        [Fact]
        public void Parser_Parse_Query_GroupByClause()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                {GROUP   BY e.Country}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.GroupByClause);
                enumerator.AssertToken(SyntaxKind.GroupKeyword, @"GROUP");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Country");
            }
        }

        [Fact]
        public void Parser_Parse_Query_GroupByClause_WithMultipleColumns()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                {GROUP   BY e.Country, e.City}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.GroupByClause);
                enumerator.AssertToken(SyntaxKind.GroupKeyword, @"GROUP");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Country");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"e");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"City");
            }
        }

        [Fact]
        public void Parser_Parse_Query_GroupByColumn()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                GROUP   BY {Country}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.GroupByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Country");
            }
        }

        [Fact]
        public void Parser_Parse_Query_HavingClause()
        {
            const string text = @"
                SELECT  *
                FROM    Employees e
                GROUP   BY e.Country, e.City
                {HAVING  TRUE}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.HavingClause);
                enumerator.AssertToken(SyntaxKind.HavingKeyword, @"HAVING");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.TrueKeyword, @"TRUE");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Ordered()
        {
            const string text = @"
                SELECT  *
                ORDER   BY 1
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OrderedQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.OrderKeyword, @"ORDER");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Ordered_WithMultipleColumns()
        {
            const string text = @"
                SELECT  *
                ORDER   BY 1, 2
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OrderedQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.OrderKeyword, @"ORDER");
                enumerator.AssertToken(SyntaxKind.ByKeyword, @"BY");
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            }
        }

        [Fact]
        public void Parser_Parse_Query_OrderByColumn_WithDescendingModifier()
        {
            const string text = @"
                SELECT  *
                FROM    Employees
                ORDER   BY {FirstName DESC}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"FirstName");
                enumerator.AssertToken(SyntaxKind.DescKeyword, @"DESC");
            }
        }

        [Fact]
        public void Parser_Parse_Query_OrderByColumn_WithAscendingModifier()
        {
            const string text = @"
                SELECT  *
                FROM    Employees
                ORDER   BY {FirstName ASC}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"FirstName");
                enumerator.AssertToken(SyntaxKind.AscKeyword, @"ASC");
            }
        }

        [Fact]
        public void Parser_Parse_Query_OrderByColumn_WithoutModifier()
        {
            const string text = @"
                SELECT  *
                FROM    Employees
                ORDER   BY {FirstName}
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.OrderByColumn);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"FirstName");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Parenthesized()
        {
            const string text = @"
                (SELECT *)
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ParenthesizedQuery);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Union_WithoutAllKeyword()
        {
            const string text = @"
                SELECT * UNION SELECT *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.UnionQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.UnionKeyword, @"UNION");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Union_WithAllKeyword()
        {
            const string text = @"
                SELECT * UNION ALL SELECT *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.UnionQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.UnionKeyword, @"UNION");
                enumerator.AssertToken(SyntaxKind.AllKeyword, @"ALL");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Intersect()
        {
            const string text = @"
                SELECT * INTERSECT SELECT *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.IntersectQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.IntersectKeyword, @"INTERSECT");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_Except()
        {
            const string text = @"
                SELECT * EXCEPT SELECT *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ExceptQuery);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.ExceptKeyword, @"EXCEPT");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_CommonTableExpression()
        {
            const string text = @"
                WITH CTE AS (
                    SELECT *
                )
                SELECT  *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionQuery);
                enumerator.AssertToken(SyntaxKind.WithKeyword, @"WITH");
                enumerator.AssertNode(SyntaxKind.CommonTableExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"CTE");
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_CommonTableExpression_WithMultipleExpressions()
        {
            const string text = @"
                WITH CTE1 AS (
                    SELECT *
                ), CTE2 AS (
                    SELECT *
                )
                SELECT  *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionQuery);
                enumerator.AssertToken(SyntaxKind.WithKeyword, @"WITH");
                enumerator.AssertNode(SyntaxKind.CommonTableExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"CTE1");
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.CommonTableExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"CTE2");
                enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
                enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            }
        }

        [Fact]
        public void Parser_Parse_Query_CommonTableExpressionColumnNameList_WithSingleColumn()
        {
            const string text = @"
                WITH CTE {(x)} AS (
                    SELECT *
                )
                SELECT  *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnNameList);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnName);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            }
        }

        [Fact]
        public void Parser_Parse_Query_CommonTableExpressionColumnNameList_WithMultipleColumns()
        {
            const string text = @"
                WITH CTE {(x, y)} AS (
                    SELECT *
                )
                SELECT  *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnNameList);
                enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnName);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
                enumerator.AssertToken(SyntaxKind.CommaToken, @",");
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnName);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"y");
                enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            }
        }

        [Fact]
        public void Parser_Parse_Query_CommonTableExpressionColumnName()
        {
            const string text = @"
                WITH CTE ({x}, y) AS (
                    SELECT *
                )
                SELECT  *
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.CommonTableExpressionColumnName);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            }
        }
    }
}