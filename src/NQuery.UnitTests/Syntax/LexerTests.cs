using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Syntax
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Lexer_DetectsUnterminatedBracketedIdentifer()
        {
            const string text = "[aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedBracketedIdentifer_IfEmpty()
        {
            const string text = "[";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedBracketedIdentifer_IfClosingBracketIsEscaped()
        {
            const string text = "[aaa[[";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedParenthesizedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Parenthesized identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedQuotedIdentifer()
        {
            const string text = "\"aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedQuotedIdentifer_IfEmpty()
        {
            const string text = "\"";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedQuotedIdentifer_IfClosingQuoteIsEscaped()
        {
            const string text = "\"aaa\"\"";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedQuotedIdentifier, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Quoted identifier is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedString()
        {
            const string text = "'aaa";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.AreEqual(0, token.Diagnostics[0].Span.Start);
            Assert.AreEqual(2, token.Diagnostics[0].Span.End);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedString_IfEmpty()
        {
            const string text = "'";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("String is not properly terminated.", token.Diagnostics[0].Message);
            Assert.AreEqual(0, token.Diagnostics[0].Span.Start);
            Assert.AreEqual(1, token.Diagnostics[0].Span.End);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedString_IfClosingQuoteIsEscaped()
        {
            const string text = "'aaa''";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("String is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedDateTime()
        {
            const string text = "#12-04-1900";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedDateTime_IfEmpty()
        {
            const string text = "#";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.DateLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(2, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedDate, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Date is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedMultilineComment()
        {
            const string text = "/* test";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.AreEqual(text, trivia.Text);
            Assert.AreEqual(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.AreEqual(false, trivia.IsTerminated());
            Assert.AreEqual(1, trivia.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedMultilineComment_IfEmpty()
        {
            const string text = "/*";
            var trivia = Helpers.LexSingleTrivia(text);

            Assert.AreEqual(text, trivia.Text);
            Assert.AreEqual(SyntaxKind.MultiLineCommentTrivia, trivia.Kind);
            Assert.AreEqual(false, trivia.IsTerminated());
            Assert.AreEqual(1, trivia.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UnterminatedComment, trivia.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("Comment is not properly terminated.", trivia.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_CanLexInvalidDate()
        {
            const string invalidDate = "#13.13.1212#";
            var token = Helpers.LexSingleToken(invalidDate);

            Assert.AreEqual(invalidDate, token.Text);
            Assert.AreEqual(true, token.IsTerminated());
            Assert.IsInstanceOfType(token.Value, typeof(DateTime));
        }

        [TestMethod]
        public void Lexer_CanLexValidDate()
        {
            const string dateSource = "#03.14.1987#";
            var date = new DateTime(1987, 3, 14);

            var token = Helpers.LexSingleToken(dateSource);

            Assert.AreEqual(dateSource, token.Text);
            Assert.AreEqual(true, token.IsTerminated());
            Assert.AreEqual(date, token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexUnterminatedValidDate()
        {
            var date = new DateTime(1987, 3, 14);
            const string dateSource = "#03.14.1987";

            var token = Helpers.LexSingleToken(dateSource);

            Assert.AreEqual(dateSource, token.Text);
            Assert.AreEqual(date, token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexUnterminatedString()
        {
            const string text = "the value";
            const string unterminatedString = "'" + text;

            var token = Helpers.LexSingleToken(unterminatedString);

            Assert.AreEqual(unterminatedString, token.Text);
            Assert.AreEqual(text, token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexString()
        {
            const string text = "the value";
            const string quotedText = "'" + text + "'";

            var token = Helpers.LexSingleToken(quotedText);

            Assert.AreEqual(quotedText, token.Text);
            Assert.AreEqual(text, token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexStringWithEscapedQuoteAtStart()
        {
            var input = "'''test'";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual("'test", token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexStringWithEscapedQuoteInTheMiddle()
        {
            var input = "'te''st'";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual("te'st", token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexStringWithEscapedQuoteAtEnd()
        {
            var input = "'test'''";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual("test'", token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexInt32()
        {
            var value = int.MaxValue;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(typeof(int), token.Value.GetType());
            Assert.AreEqual(value, token.Value);
        }

        [TestMethod]
        public void Lexer_CanLexInt64()
        {
            var value = (long)int.MaxValue + 1;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(typeof(long), token.Value.GetType());
            Assert.AreEqual(value, token.Value);
        }

        [TestMethod]
        public void Lexer_DetectsNumbersTooLargeForLong()
        {
            var value = ulong.MaxValue;
            var input = value.ToString();
            var token = Helpers.LexSingleToken(input);
            var diagnostics = token.Diagnostics;

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(typeof(int), token.Value.GetType());
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.NumberTooLarge, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Lexer_CanLexDouble()
        {
            var input = "1.0";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(1.0, token.Value);
            Assert.AreEqual(typeof(double), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexDouble_WhenHavingLeadingPeriod()
        {
            var input = ".0";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(0.0, token.Value);
            Assert.AreEqual(typeof(double), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffix()
        {
            var input = "1e4";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(1e4, token.Value);
            Assert.AreEqual(typeof(double), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffixWithNegativeSign()
        {
            var input = "1e-4";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(1e-4, token.Value);
            Assert.AreEqual(typeof(double), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexDouble_WhenHavingExponentialSuffixWithPositiveSign()
        {
            var input = "1e+4";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(1e+4, token.Value);
            Assert.AreEqual(typeof(double), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_DetectsInvalidReal()
        {
            var input = "123e123e";
            var token = Helpers.LexSingleToken(input);
            var diagnostics = token.Diagnostics;

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(typeof(double), token.Value.GetType());
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidReal, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Lexer_CanLexInt32_WhenInBinary()
        {
            var input = "1010b";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(10, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexInt64_WhenInBinary()
        {
            var input = "10101010101010101010101010101010b";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(2863311530L, token.Value);
            Assert.AreEqual(typeof(long), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_DetectsInvalidBinary()
        {
            var input = "1234b";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidBinary, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual(0, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexInt32_WhenInOctal()
        {
            var input = "12345o";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(5349, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexInt64_WhenInOctal()
        {
            var input = "12345671234567o";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(718046312823L, token.Value);
            Assert.AreEqual(typeof(long), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_DetectsInvalidOctal()
        {
            var input = "78o";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidOctal, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual(0, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexInt32_WhenInHex()
        {
            var input = "0ABCh";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(2748, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_CanLexInt64_WhenInHex()
        {
            var input = "0FFFFFFFFFh";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(0, token.Diagnostics.Length);
            Assert.AreEqual(68719476735L, token.Value);
            Assert.AreEqual(typeof(long), token.Value.GetType());
        }

        [TestMethod]
        public void Lexer_DetectsInvalidHex()
        {
            var input = "0FGh";
            var token = Helpers.LexSingleToken(input);

            Assert.AreEqual(input, token.Text);
            Assert.AreEqual(SyntaxKind.NumericLiteralToken, token.Kind);
            Assert.AreEqual(1, token.Diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidHex, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual(0, token.Value);
            Assert.AreEqual(typeof(int), token.Value.GetType());
        }

        // Independence of current culture
    }
}
