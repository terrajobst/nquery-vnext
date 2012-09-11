using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language.UnitTests
{
    internal static class Helpers
    {
        public static SyntaxToken LexSingleToken(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var token = tree.Root.FirstToken();
            return token;
        }

        public static SyntaxTrivia LexSingleTrivia(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var trivia = tree.Root.LastToken().LeadingTrivia.First();
            return trivia;
        }

        public static ExpressionSyntax ParseExpression(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            return (ExpressionSyntax) tree.Root.Root;
        }

        public static SyntaxToken CreateToken(SyntaxKind kind, string text = null)
        {
            var actualText = text ?? kind.GetText();
            var span = new TextSpan(0, actualText.Length);
            return new SyntaxToken(kind, SyntaxKind.BadToken, false, span, actualText, null, new SyntaxTrivia[0], new SyntaxTrivia[0], new Diagnostic[0]);
        }

        public static Conversion ClassifyConversion(Type souceType, Type targetType)
        {
            var semanticModel = Compilation.Empty.GetSemanticModel();
            var conversion = semanticModel.ClassifyConversion(souceType, targetType);
            return conversion;
        }
    }
}