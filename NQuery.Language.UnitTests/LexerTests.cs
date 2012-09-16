using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
            Assert.AreEqual(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("String is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedString_IfEmpty()
        {
            const string text = "'";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Count);
            Assert.AreEqual(DiagnosticId.UnterminatedString, token.Diagnostics[0].DiagnosticId);
            Assert.AreEqual("String is not properly terminated.", token.Diagnostics[0].Message);
        }

        [TestMethod]
        public void Lexer_DetectsUnterminatedString_IfClosingQuoteIsEscaped()
        {
            const string text = "'aaa''";
            var token = Helpers.LexSingleToken(text);

            Assert.AreEqual(text, token.Text);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.AreEqual(false, token.IsTerminated());
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(1, token.Diagnostics.Count);
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
            Assert.AreEqual(2, token.Diagnostics.Count);
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
            Assert.AreEqual(1, trivia.Diagnostics.Count);
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
            Assert.AreEqual(1, trivia.Diagnostics.Count);
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

        // Int32
        // Int64
        // double
        // float
        // decimal
        // binary, octal, hex
        // Independence of current culture
        // Exponential suffix
        // Exponential suffix with explicit sign
    }
}
