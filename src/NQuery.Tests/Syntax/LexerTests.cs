using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lex_Special_EndOfFile()
        {
            var token = Helpers.LexSingleToken(string.Empty);

            Assert.Equal(SyntaxKind.EndOfFileToken, token.Kind);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Theory]
        [InlineData("!")]
        [InlineData("?")]
        public void Lexer_Lex_Special_BadToken(string text)
        {
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.BadToken, token.Kind);
            Assert.True(token.IsTerminated());

            var diagnostic = Assert.Single(token.Diagnostics);
            Assert.Equal(DiagnosticId.IllegalInputCharacter, diagnostic.DiagnosticId);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Whitespace()
        {
            const string text = "\t ";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, trivia.Kind);
            Assert.True(trivia.IsTerminated());
            Assert.Equal(0, trivia.Diagnostics.Length);
        }

        [Theory]
        [InlineData("\r\n")]
        [InlineData("\n")]
        [InlineData("\r")]
        public void Lexer_Lex_Trivia_EndOfLine(string text)
        {
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.EndOfLineTrivia, trivia.Kind);
            Assert.True(trivia.IsTerminated());
            Assert.Equal(0, trivia.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_SingleLine_WithSlashes()
        {
            var text = "// test" + Environment.NewLine + "abc";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(2, token.LeadingTrivia.Length);

            var comment = token.LeadingTrivia[0];
            Assert.Equal("// test", comment.Text);
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, comment.Kind);
            Assert.True(comment.IsTerminated());
            Assert.Equal(0, comment.Diagnostics.Length);

            var lineBreak = token.LeadingTrivia[1];
            Assert.Equal(Environment.NewLine, lineBreak.Text);
            Assert.Equal(SyntaxKind.EndOfLineTrivia, lineBreak.Kind);
            Assert.True(lineBreak.IsTerminated());
            Assert.Equal(0, lineBreak.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_SingleLine_WithDashes()
        {
            var text = "-- test" + Environment.NewLine + "abc";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(2, token.LeadingTrivia.Length);

            var comment = token.LeadingTrivia[0];
            Assert.Equal("-- test", comment.Text);
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, comment.Kind);
            Assert.True(comment.IsTerminated());
            Assert.Equal(0, comment.Diagnostics.Length);

            var lineBreak = token.LeadingTrivia[1];
            Assert.Equal(Environment.NewLine, lineBreak.Text);
            Assert.Equal(SyntaxKind.EndOfLineTrivia, lineBreak.Kind);
            Assert.True(lineBreak.IsTerminated());
            Assert.Equal(0, lineBreak.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_SingleLine_WithoutEndOfLine()
        {
            var text = "-- test";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.EndOfFileToken, token.Kind);

            var comment = Assert.Single(token.LeadingTrivia);
            Assert.Equal("-- test", comment.Text);
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, comment.Kind);
            Assert.True(comment.IsTerminated());
            Assert.Equal(0, comment.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_MultiLine()
        {
            const string text = "/* test * */";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.True(trivia.IsTerminated());
            Assert.Equal(0, trivia.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_MultiLine_IfUnterminated()
        {
            const string text = "/* test";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.False(trivia.IsTerminated());
            Assert.Equal(1, trivia.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.Equal("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Trivia_Comment_MultiLine_IfUnterminatedAndEmpty()
        {
            const string text = "/*";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.False(trivia.IsTerminated());
            Assert.Equal(1, trivia.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.Equal("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Trivia_LeadingAndTrailing()
        {
            const string text = " -- Leading 1\n" +
                                "-- Leading 2\n" +
                                "identifier\t-- trailing\n" +
                                "-- unrelated";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(5, token.LeadingTrivia.Length);

            var leadingSpace = token.LeadingTrivia[0];
            Assert.Equal(SyntaxKind.WhitespaceTrivia, leadingSpace.Kind);
            Assert.Equal(" ", leadingSpace.Text);

            var leadingComment1 = token.LeadingTrivia[1];
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, leadingComment1.Kind);
            Assert.Equal("-- Leading 1", leadingComment1.Text);

            var leadingEndOfLine1 = token.LeadingTrivia[2];
            Assert.Equal(SyntaxKind.EndOfLineTrivia, leadingEndOfLine1.Kind);
            Assert.Equal("\n", leadingEndOfLine1.Text);

            var leadingComment2 = token.LeadingTrivia[3];
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, leadingComment2.Kind);
            Assert.Equal("-- Leading 2", leadingComment2.Text);

            var leadingEndOfLine2 = token.LeadingTrivia[4];
            Assert.Equal(SyntaxKind.EndOfLineTrivia, leadingEndOfLine2.Kind);
            Assert.Equal("\n", leadingEndOfLine2.Text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal("identifier", token.Text);

            Assert.Equal(3, token.TrailingTrivia.Length);

            var trailingSpace = token.TrailingTrivia[0];
            Assert.Equal(SyntaxKind.WhitespaceTrivia, trailingSpace.Kind);
            Assert.Equal("\t", trailingSpace.Text);

            var trailingComment = token.TrailingTrivia[1];
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, trailingComment.Kind);
            Assert.Equal("-- trailing", trailingComment.Text);

            var trailingEndOfLine = token.TrailingTrivia[2];
            Assert.Equal(SyntaxKind.EndOfLineTrivia, trailingEndOfLine.Kind);
            Assert.Equal("\n", trailingEndOfLine.Text);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_WithSpace()
        {
            LexBracketedIdentifier("lorem ipsum");
        }

        [Theory]
        [MemberData("GetReservedKeywordKinds")]
        public void Lexer_Lex_Identifier_Bracketed_WithReservedKeyword(SyntaxKind kind)
        {
            var keyword = kind.GetText();
            LexBracketedIdentifier(keyword);
        }

        private static void LexBracketedIdentifier(string identifierText)
        {
            var text = "[" + identifierText + "]";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(identifierText, token.Value);
            Assert.Equal(identifierText, token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfEscapedBracketAtStart()
        {
            const string text = "[]]test]";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("]test", token.Value);
            Assert.Equal("]test", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfEscapedBracketInTheMiddle()
        {
            const string text = "[te]]st]";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("te]st", token.Value);
            Assert.Equal("te]st", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfEscapedBracketAtEnd()
        {
            const string text = "[test]]]";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("test]", token.Value);
            Assert.Equal("test]", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfUnterminated()
        {
            const string text = "[aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfUnterminatedAndEmpty()
        {
            const string text = "[";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Bracketed_IfUnterminatedAndClosingBracketIsEscaped()
        {
            const string text = "[aaa[[";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_WithSpace()
        {
            LexQuotedIdentifier("lorem ipsum");
        }

        [Theory]
        [MemberData("GetReservedKeywordKinds")]
        public void Lexer_Lex_Identifier_Quoted_WithReservedKeyword(SyntaxKind kind)
        {
            var keyword = kind.GetText();
            LexQuotedIdentifier(keyword);
        }

        private static void LexQuotedIdentifier(string identifierText)
        {
            var text = "\"" + identifierText + "\"";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(identifierText, token.Value);
            Assert.Equal(identifierText, token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfEscapedBracketAtStart()
        {
            const string text = @"""""""test""";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal(@"""test", token.Value);
            Assert.Equal(@"""test", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfEscapedBracketInTheMiddle()
        {
            const string text = @"""te""""st""";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal(@"te""st", token.Value);
            Assert.Equal(@"te""st", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfEscapedBracketAtEnd()
        {
            const string text = @"""test""""""";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal(@"test""", token.Value);
            Assert.Equal(@"test""", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfUnterminated()
        {
            const string text = "\"aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfUnterminatedAndEmpty()
        {
            const string text = "\"";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Identifier_Quoted_IfUnterminatedAndClosingQuoteIsEscaped()
        {
            const string text = "\"aaa\"\"";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Literal_String()
        {
            const string text = "'lorem ipsum'";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("lorem ipsum", token.Value);
            Assert.Equal("lorem ipsum", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfEscapedQuoteAtStart()
        {
            const string text = "'''test'";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("'test", token.Value);
            Assert.Equal("'test", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfEscapedQuoteInTheMiddle()
        {
            const string text = "'te''st'";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("te'st", token.Value);
            Assert.Equal("te'st", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfEscapedQuoteAtEnd()
        {
            const string text = "'test'''";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(text, token.Text);
            Assert.Equal("test'", token.Value);
            Assert.Equal("test'", token.ValueText);
            Assert.True(token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfUnterminated()
        {
            const string text = "'aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal("aaa", token.Value);
            Assert.Equal("aaa", token.ValueText);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.Equal(0, token.Diagnostics[0].Span.Start);
            Assert.Equal(2, token.Diagnostics[0].Span.End);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfUnterminatedAndEmpty()
        {
            const string text = "'";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.Equal(0, token.Diagnostics[0].Span.Start);
            Assert.Equal(1, token.Diagnostics[0].Span.End);
        }

        [Fact]
        public void Lexer_Lex_Literal_String_IfUnterminatedAndClosingQuoteIsEscaped()
        {
            const string text = "'aaa''";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Literal_DateTime()
        {
            const string dateSource = "#03.14.1987#";
            var date = new DateTime(1987, 3, 14);

            var token = Helpers.LexSingleToken(dateSource);

            Assert.Equal(dateSource, token.Text);
            Assert.True(token.IsTerminated());
            Assert.Equal(date, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_DateTime_IfInvalid()
        {
            const string invalidDate = "#13.13.1212#";
            var token = Helpers.LexSingleToken(invalidDate);

            Assert.Equal(invalidDate, token.Text);
            Assert.True(token.IsTerminated());
            Assert.IsType<DateTime>(token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_DateTime_IfUnterminated()
        {
            const string text = "#12-04-1900";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(new DateTime(1900, 12, 4), token.Value);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Literal_DateTime_IfUnterminatedAndEmpty()
        {
            const string text = "#";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.False(token.IsTerminated());
            Assert.Equal(2, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInDecimal()
        {
            const int value = int.MaxValue;
            var text = value.ToString();
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<int>(token.Value);
            Assert.Equal(value, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInBinary()
        {
            const string text = "1010b";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<int>(token.Value);
            Assert.Equal(10, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInBinaryAndInvalid()
        {
            const string text = "1234b";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidBinary, token.Diagnostics[0].DiagnosticId);
            Assert.IsType<int>(token.Value);
            Assert.Equal(0, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInOctal()
        {
            const string text = "12345o";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<int>(token.Value);
            Assert.Equal(5349, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInOctalAndInvalid()
        {
            const string text = "78o";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidOctal, token.Diagnostics[0].DiagnosticId);
            Assert.IsType<int>(token.Value);
            Assert.Equal(0, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInHex()
        {
            const string text = "0ABCh";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<int>(token.Value);
            Assert.Equal(2748, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenInHexAndInvalid()
        {
            const string text = "0FGh";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidHex, token.Diagnostics[0].DiagnosticId);
            Assert.IsType<int>(token.Value);
            Assert.Equal(0, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int32_WhenNumberTooLargeForInt64()
        {
            const ulong value = ulong.MaxValue;
            var text = value.ToString();
            var token = Helpers.LexSingleToken(text);
            var diagnostics = token.Diagnostics;

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.IsType<int>(token.Value);
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.NumberTooLarge, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int64_WhenInDecimal()
        {
            var value = (long)int.MaxValue + 1;
            var text = value.ToString();
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<long>(token.Value);
            Assert.Equal(value, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int64_WhenInBinary()
        {
            const string text = "10101010101010101010101010101010b";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<long>(token.Value);
            Assert.Equal(2863311530L, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int64_WhenInOctal()
        {
            const string text = "12345671234567o";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<long>(token.Value);
            Assert.Equal(718046312823L, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Int64_WhenInHex()
        {
            const string text = "0FFFFFFFFFh";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<long>(token.Value);
            Assert.Equal(68719476735L, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double()
        {
            const string text = "1.0";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<double>(token.Value);
            Assert.Equal(1.0, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double_WithLeadingPeriod()
        {
            const string text = ".0";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<double>(token.Value);
            Assert.Equal(0.0, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double_WithExponentialSuffix()
        {
            const string text = "1e4";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<double>(token.Value);
            Assert.Equal(1e4, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double_WithExponentialSuffixWithNegativeSign()
        {
            const string text = "1e-4";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<double>(token.Value);
            Assert.Equal(1e-4, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double_WithExponentialSuffixWithPositiveSign()
        {
            const string text = "1e+4";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.IsType<double>(token.Value);
            Assert.Equal(1e+4, token.Value);
        }

        [Fact]
        public void Lexer_Lex_Literal_Double_WhenInvalid()
        {
            const string text = "123e123e";
            var token = Helpers.LexSingleToken(text);
            var diagnostics = token.Diagnostics;

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.IsType<double>(token.Value);
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidReal, diagnostics[0].DiagnosticId);
        }

        [Theory]
        [MemberData("GetPunctuationTokenKinds")]
        public void Lexer_Lex_Punctuation(SyntaxKind kind)
        {
            LexPunctuationOrOperator(kind);
        }

        [Theory]
        [MemberData("GetUnaryOperatorTokenKinds")]
        public void Lexer_Lex_Operator_Unary(SyntaxKind kind)
        {
            LexPunctuationOrOperator(kind);
        }

        [Theory]
        [MemberData("GetBinaryOperatorTokenKinds")]
        public void Lexer_Lex_Operator_Binary(SyntaxKind kind)
        {
            LexPunctuationOrOperator(kind);
        }

        private static void LexPunctuationOrOperator(SyntaxKind kind)
        {
            var text = kind.GetText();
            var token = Helpers.LexSingleToken(text);

            var expectedContextualKind = kind.IsKeyword()
                ? SyntaxKind.IdentifierToken
                : SyntaxKind.BadToken;

            Assert.Equal(kind, token.Kind);
            Assert.Equal(expectedContextualKind, token.ContextualKind);
            Assert.Equal(text, token.Text);
            Assert.Equal(true, token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        [Theory]
        [MemberData("GetReservedKeywordKinds")]
        public void Lexer_Lex_Keyword_Reserved(SyntaxKind kind)
        {
            LexKeyword(kind, isReserved:true);
        }

        [Theory]
        [MemberData("GetContextualKeywordKinds")]
        public void Lexer_Lex_Keyword_Contextual(SyntaxKind kind)
        {
            LexKeyword(kind, isReserved: false);
        }

        private static void LexKeyword(SyntaxKind kind, bool isReserved)
        {
            var text = kind.GetText();
            var token = Helpers.LexSingleToken(text);

            var expectedKind = isReserved
                ? kind
                : SyntaxKind.IdentifierToken;

            var expectedContextualKind = isReserved
                ? SyntaxKind.IdentifierToken
                : kind;

            Assert.Equal(expectedKind, token.Kind);
            Assert.Equal(expectedContextualKind, token.ContextualKind);
            Assert.Equal(text, token.Text);
            Assert.Equal(true, token.IsTerminated());
            Assert.Equal(0, token.Diagnostics.Length);
        }

        public static IEnumerable<object[]> GetPunctuationTokenKinds()
        {
            yield return new object[] {SyntaxKind.CommaToken};
            yield return new object[] {SyntaxKind.AtToken};
            yield return new object[] {SyntaxKind.DotToken};
            yield return new object[] {SyntaxKind.LeftParenthesisToken};
            yield return new object[] {SyntaxKind.RightParenthesisToken};
        }

        public static IEnumerable<object[]> GetUnaryOperatorTokenKinds()
        {
            return SyntaxFacts.GetUnaryExpressionTokenKinds()
                              .Select(t => new object[] { t });
        }

        public static IEnumerable<object[]> GetBinaryOperatorTokenKinds()
        {
            return SyntaxFacts.GetBinaryExpressionTokenKinds()
                              .Select(t => new object[] { t });
        }

        public static IEnumerable<object[]> GetReservedKeywordKinds()
        {
            return SyntaxFacts.GetReservedKeywordKinds()
                              .Select(t => new object[] { t });
        }

        public static IEnumerable<object[]> GetContextualKeywordKinds()
        {
            return SyntaxFacts.GetContextualKeywordKinds()
                              .Select(t => new object[] { t });
        }

        // Independence of current culture
    }
}
