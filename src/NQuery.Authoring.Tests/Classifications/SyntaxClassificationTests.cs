using System;
using System.Linq;

using Xunit;

using NQuery.Authoring.Classifications;
using NQuery.Text;

namespace NQuery.Authoring.UnitTests.Classifications
{
    public class SyntaxClassificationTests
    {
        [Fact]
        public void SyntaxClassification_Classifies()
        {
            var pieces = new[]
            {
                Tuple.Create("-- Single line comment", SyntaxClassification.Comment),
                Tuple.Create(Environment.NewLine, SyntaxClassification.Whitespace),
                Tuple.Create("SELECT", SyntaxClassification.Keyword),
                Tuple.Create("\t", SyntaxClassification.Whitespace),
                Tuple.Create("e", SyntaxClassification.Identifier),
                Tuple.Create(".", SyntaxClassification.Punctuation),
                Tuple.Create("\"FirstName\"", SyntaxClassification.Identifier),
                Tuple.Create(" ", SyntaxClassification.Whitespace),
                Tuple.Create("AS", SyntaxClassification.Keyword),
                Tuple.Create("[Full Name]", SyntaxClassification.Identifier),
                Tuple.Create(Environment.NewLine, SyntaxClassification.Whitespace),
                Tuple.Create("FROM", SyntaxClassification.Keyword),
                Tuple.Create(" ", SyntaxClassification.Whitespace),
                Tuple.Create("Employees", SyntaxClassification.Identifier),
                Tuple.Create(Environment.NewLine, SyntaxClassification.Whitespace),
                Tuple.Create("/* This a \r\n Comment \r that spans \n multiple lines */", SyntaxClassification.Comment),
                Tuple.Create("WHERE", SyntaxClassification.Keyword),
                Tuple.Create(" ", SyntaxClassification.Whitespace),
                Tuple.Create("FirsName", SyntaxClassification.Identifier),
                Tuple.Create("=", SyntaxClassification.Punctuation),
                Tuple.Create("'Andrew'", SyntaxClassification.StringLiteral),
                Tuple.Create(Environment.NewLine, SyntaxClassification.Whitespace),
                Tuple.Create("AND", SyntaxClassification.Keyword),
                Tuple.Create(" ", SyntaxClassification.Whitespace),
                Tuple.Create("ReportsTo", SyntaxClassification.Identifier),
                Tuple.Create("<", SyntaxClassification.Punctuation),
                Tuple.Create("4", SyntaxClassification.NumberLiteral),
            };

            var text = string.Concat(pieces.Select(t => t.Item1));
            var syntaxTree = SyntaxTree.ParseQuery(text);
            var classificationSpans = syntaxTree.Root.ClassifySyntax();

            Assert.Equal(pieces.Length, classificationSpans.Count);

            var position = 0;

            for (var i = 0; i < pieces.Length; i++)
            {
                var piecce = pieces[i];
                var pieceText = piecce.Item1;
                var pieceSpan = new TextSpan(position, pieceText.Length);
                var classification = piecce.Item2;

                Assert.Equal(pieceSpan, classificationSpans[i].Span);
                Assert.Equal(classification, classificationSpans[i].Classification);

                position = pieceSpan.End;
            }
        }
    }
}