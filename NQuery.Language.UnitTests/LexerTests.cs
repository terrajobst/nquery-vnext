using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class LexerTests
    {
        private static SyntaxToken LexSingleToken(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var token = tree.Root.FirstToken();
            return token;
        }

        [TestMethod]
        public void InvalidDateRountripsAsTextAndHasValue()
        {
            const string invalidDate = "#13.13.1212#";
            var token = LexSingleToken(invalidDate);

            Assert.AreEqual(invalidDate, token.Text);
            Assert.IsInstanceOfType(token.Value, typeof(DateTime));
        }

        [TestMethod]
        public void ValidDateRountripsAsTextAndHasValue()
        {
            const string dateSource = "#03.14.1987#";
            var date = new DateTime(1987, 3, 14);

            var token = LexSingleToken(dateSource);

            Assert.AreEqual(dateSource, token.Text);
            Assert.AreEqual(date, token.Value);
        }

        [TestMethod]
        public void UnterminatedValidDateRoundtripsAsTextAndHasValue()
        {
            var date = new DateTime(1987, 3, 14);
            const string dateSource = "#03.14.1987";

            var token = LexSingleToken(dateSource);

            Assert.AreEqual(dateSource, token.Text);
            Assert.AreEqual(date, token.Value);
        }

        [TestMethod]
        public void UnterminatedStringRoundtripsAsTextAndHasValue()
        {
            const string text = "the value";
            const string unterminatedString = "'" + text;

            var token = LexSingleToken(unterminatedString);

            Assert.AreEqual(unterminatedString, token.Text);
            Assert.AreEqual(text, token.Value);
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
