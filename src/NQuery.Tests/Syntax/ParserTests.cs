using System;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public partial class ParserTests
    {
        [Fact]
        public void Parser_Error_SkipsBadTokens()
        {
            const string text = @"
                'First' + !'Last'
            ";

            using (var enumerator = AssertingEnumerator.ForExpression(text))
            {
                enumerator.AssertNode(SyntaxKind.AddExpression);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'First'");
                enumerator.AssertToken(SyntaxKind.PlusToken, @"+");
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertNode(SyntaxKind.SkippedTokensTrivia);
                enumerator.AssertToken(SyntaxKind.BadToken, @"!");
                enumerator.AssertDiagnostic(DiagnosticId.IllegalInputCharacter, @"Invalid character in input '!'.");
                enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'Last'");
            }
        }

        [Fact]
        public void Parser_Error_DetectsErrorAtEnd()
        {
            const string text = @"
                SELECT 'foo' WITH
            ";

            var syntaxTree = SyntaxTree.ParseQuery(text);

            using (var enumerator = AssertingEnumerator.ForNode(syntaxTree.Root))
            {
                enumerator.AssertNode(SyntaxKind.CompilationUnit);
                enumerator.AssertNode(SyntaxKind.SelectQuery);
                enumerator.AssertNode(SyntaxKind.SelectClause);
                enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.LiteralExpression);
                enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'foo'");
                enumerator.AssertNode(SyntaxKind.SkippedTokensTrivia);
                enumerator.AssertToken(SyntaxKind.WithKeyword, @"WITH");
                enumerator.AssertToken(SyntaxKind.EndOfFileToken, @"");
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found 'WITH' but expected '<end-of-file>'.");
            }
        }

        [Fact]
        public void Parser_Error_MissingIdentifier_IsInserted_IfKeywordOnNextLine()
        {
            const string text = @"
                SELECT   {o.
                FROM}     Orders
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"o");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found 'FROM' but expected '<identifier>'.");
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
            }
        }

        [Fact]
        public void Parser_Error_MissingIdentifier_IsInserted_AndSkipsKeyword_IfKeywordOnSameLine()
        {
            const string text = @"
                SELECT   {o.Or
                FROM}     Orders
            ";

            using (var enumerator = AssertingEnumerator.ForQuery(text))
            {
                enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
                enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, @"o");
                enumerator.AssertToken(SyntaxKind.DotToken, @".");
                enumerator.AssertNode(SyntaxKind.SkippedTokensTrivia);
                enumerator.AssertToken(SyntaxKind.OrKeyword, @"Or");
                enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
                enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found 'Or' but expected '<identifier>'.");
                enumerator.AssertToken(SyntaxKind.FromKeyword, @"FROM");
            }
        }
    }
}