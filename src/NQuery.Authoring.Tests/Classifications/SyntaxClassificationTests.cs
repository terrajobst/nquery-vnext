using NQuery.Authoring.Classifications;
using NQuery.Text;

namespace NQuery.Authoring.Tests.Classifications
{
    public class SyntaxClassificationTests
    {
        [Fact]
        public void SyntaxClassification_Classifies()
        {
            var pieces = new (string Text, SyntaxClassification Classification)[]
            {
                ("-- Single line comment", SyntaxClassification.Comment),
                (Environment.NewLine, SyntaxClassification.Whitespace),
                ("SELECT", SyntaxClassification.Keyword),
                ("\t", SyntaxClassification.Whitespace),
                ("e", SyntaxClassification.Identifier),
                (".", SyntaxClassification.Punctuation),
                ("\"FirstName\"", SyntaxClassification.Identifier),
                (" ", SyntaxClassification.Whitespace),
                ("AS", SyntaxClassification.Keyword),
                ("[Full Name]", SyntaxClassification.Identifier),
                (Environment.NewLine, SyntaxClassification.Whitespace),
                ("FROM", SyntaxClassification.Keyword),
                (" ", SyntaxClassification.Whitespace),
                ("Employees", SyntaxClassification.Identifier),
                (Environment.NewLine, SyntaxClassification.Whitespace),
                ("/* This a \r\n Comment \r that spans \n multiple lines */", SyntaxClassification.Comment),
                ("WHERE", SyntaxClassification.Keyword),
                (" ", SyntaxClassification.Whitespace),
                ("FirsName", SyntaxClassification.Identifier),
                ("=", SyntaxClassification.Punctuation),
                ("'Andrew'", SyntaxClassification.StringLiteral),
                (Environment.NewLine, SyntaxClassification.Whitespace),
                ("AND", SyntaxClassification.Keyword),
                (" ", SyntaxClassification.Whitespace),
                ("ReportsTo", SyntaxClassification.Identifier),
                ("<", SyntaxClassification.Punctuation),
                ("4", SyntaxClassification.NumberLiteral),
            };

            var text = string.Concat(pieces.Select(t => t.Text));
            var syntaxTree = SyntaxTree.ParseQuery(text);
            var classificationSpans = syntaxTree.Root.ClassifySyntax();

            Assert.Equal(pieces.Length, classificationSpans.Count);

            var position = 0;

            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                var pieceText = piece.Text;
                var pieceSpan = new TextSpan(position, pieceText.Length);
                var classification = piece.Classification;

                Assert.Equal(pieceSpan, classificationSpans[i].Span);
                Assert.Equal(classification, classificationSpans[i].Classification);

                position = pieceSpan.End;
            }
        }
    }
}