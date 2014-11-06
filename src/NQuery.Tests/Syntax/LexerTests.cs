using System;

using Xunit;

namespace NQuery.UnitTests.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_DetectsUnterminatedBracketedIdentifer()
        {
            const string text = "[aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedBracketedIdentifer_IfEmpty()
        {
            const string text = "[";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedBracketedIdentifer_IfClosingBracketIsEscaped()
        {
            const string text = "[aaa[[";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedQuotedIdentifer()
        {
            const string text = "\"aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedQuotedIdentifer_IfEmpty()
        {
            const string text = "\"";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedQuotedIdentifer_IfClosingQuoteIsEscaped()
        {
            const string text = "\"aaa\"\"";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedString()
        {
            const string text = "'aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.Equal(0, token.Diagnostics[0].Span.Start);
            Assert.Equal(2, token.Diagnostics[0].Span.End);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedString_IfEmpty()
        {
            const string text = "'";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.Equal(0, token.Diagnostics[0].Span.Start);
            Assert.Equal(1, token.Diagnostics[0].Span.End);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedString_IfClosingQuoteIsEscaped()
        {
            const string text = "'aaa''";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("String is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedDateTime()
        {
            const string text = "#12-04-1900";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedDateTime_IfEmpty()
        {
            const string text = "#";
            var token = Helpers.LexSingleToken(text);

            Assert.Equal(text, token.Text);
            Assert.Equal(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.Equal(false, token.IsTerminated());
            Assert.Equal(2, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.Equal("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedMultilineComment()
        {
            const string text = "/* test";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.Equal(false, trivia.IsTerminated());
            Assert.Equal(1, trivia.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.Equal("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_DetectsUnterminatedMultilineComment_IfEmpty()
        {
            const string text = "/*";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.Equal(text, trivia.Text);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.Equal(false, trivia.IsTerminated());
            Assert.Equal(1, trivia.Diagnostics.Length);
            Assert.Equal(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.Equal("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [Fact]
        public void Lexer_CanLexInvalidDate()
        {
            const string invalidDate = "#13.13.1212#";
            var token = Helpers.LexSingleToken(invalidDate);

            Assert.Equal(invalidDate, token.Text);
            Assert.Equal(true, token.IsTerminated());
            Assert.IsType<DateTime>(token.Value);
        }

        [Fact]
        public void Lexer_CanLexValidDate()
        {
            const string dateSource = "#03.14.1987#";
            var date = new DateTime(1987, 3, 14);

            var token = Helpers.LexSingleToken(dateSource);

            Assert.Equal(dateSource, token.Text);
            Assert.Equal(true, token.IsTerminated());
            Assert.Equal(date, token.Value);
        }

        [Fact]
        public void Lexer_CanLexUnterminatedValidDate()
        {
            var date = new DateTime(1987, 3, 14);
            const string dateSource = "#03.14.1987";

            var token = Helpers.LexSingleToken(dateSource);

            Assert.Equal(dateSource, token.Text);
            Assert.Equal(date, token.Value);
        }

        [Fact]
        public void Lexer_CanLexUnterminatedString()
        {
            const string text = "the value";
            const string unterminatedString = "'" + text;

            var token = Helpers.LexSingleToken(unterminatedString);

            Assert.Equal(unterminatedString, token.Text);
            Assert.Equal(text, token.Value);
        }

        [Fact]
        public void Lexer_CanLexString()
        {
            const string text = "the value";
            const string quotedText = "'" + text + "'";

            var token = Helpers.LexSingleToken(quotedText);

            Assert.Equal(quotedText, token.Text);
            Assert.Equal(text, token.Value);
        }

        [Fact]
        public void Lexer_CanLexStringWithEscapedQuoteAtStart()
        {
            var input = "'''test'";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal("'test", token.Value);
        }

        [Fact]
        public void Lexer_CanLexStringWithEscapedQuoteInTheMiddle()
        {
            var input = "'te''st'";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal("te'st", token.Value);
        }

        [Fact]
        public void Lexer_CanLexStringWithEscapedQuoteAtEnd()
        {
            var input = "'test'''";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal("test'", token.Value);
        }

        [Fact]
        public void Lexer_CanLexInt32()
        {
            var value = int.MaxValue;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(typeof(int), token.Value.GetType());
            Assert.Equal(value, token.Value);
        }

        [Fact]
        public void Lexer_CanLexInt64()
        {
            var value = (long)int.MaxValue + 1;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(typeof(long), token.Value.GetType());
            Assert.Equal(value, token.Value);
        }

        [Fact]
        public void Lexer_DetectsNumbersTooLargeForLong()
        {
            var value = ulong.MaxValue;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);
            var diagnostics = token.Diagnostics;

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(typeof(int), token.Value.GetType());
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.NumberTooLarge, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Lexer_CanLexDouble()
        {
            var input = "1.0";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(1.0, token.Value);
            Assert.Equal(typeof(double), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexDouble_WhenHavingLeadingPeriod()
        {
            var input = ".0";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(0.0, token.Value);
            Assert.Equal(typeof(double), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffix()
        {
            var input = "1e4";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(1e4, token.Value);
            Assert.Equal(typeof(double), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffixWithNegativeSign()
        {
            var input = "1e-4";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(1e-4, token.Value);
            Assert.Equal(typeof(double), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffixWithPositiveSign()
        {
            var input = "1e+4";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(1e+4, token.Value);
            Assert.Equal(typeof(double), token.Value.GetType());
        }

        [Fact]
        public void Lexer_DetectsInvalidReal()
        {
            var input = "123e123e";
            var token = Helpers.LexSingleToken(input);
            var diagnostics = token.Diagnostics;

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(typeof(double), token.Value.GetType());
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidReal, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Lexer_CanLexInt32_WhenInBinary()
        {
            var input = "1010b";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(10, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexInt64_WhenInBinary()
        {
            var input = "10101010101010101010101010101010b";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(2863311530L, token.Value);
            Assert.Equal(typeof(long), token.Value.GetType());
        }

        [Fact]
        public void Lexer_DetectsInvalidBinary()
        {
            var input = "1234b";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidBinary, token.Diagnostics[0].DiagnosticId);
            Assert.Equal(0, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexInt32_WhenInOctal()
        {
            var input = "12345o";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(5349, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexInt64_WhenInOctal()
        {
            var input = "12345671234567o";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(718046312823L, token.Value);
            Assert.Equal(typeof(long), token.Value.GetType());
        }

        [Fact]
        public void Lexer_DetectsInvalidOctal()
        {
            var input = "78o";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidOctal, token.Diagnostics[0].DiagnosticId);
            Assert.Equal(0, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexInt32_WhenInHex()
        {
            var input = "0ABCh";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(2748, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        [Fact]
        public void Lexer_CanLexInt64_WhenInHex()
        {
            var input = "0FFFFFFFFFh";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(0, token.Diagnostics.Length);
            Assert.Equal(68719476735L, token.Value);
            Assert.Equal(typeof(long), token.Value.GetType());
        }

        [Fact]
        public void Lexer_DetectsInvalidHex()
        {
            var input = "0FGh";
            var token = Helpers.LexSingleToken(input);

            Assert.Equal(input, token.Text);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.Equal(1, token.Diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidHex, token.Diagnostics[0].DiagnosticId);
            Assert.Equal(0, token.Value);
            Assert.Equal(typeof(int), token.Value.GetType());
        }

        // Independence of current culture
    }
}
