using System;
using System.Linq;

namespace NQuery.UnitTests
{
    internal static class Helpers
    {
        public static SyntaxToken LexSingleToken(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var token = tree.Root.FirstToken(true, true);
            return token;
        }

        public static SyntaxTrivia LexSingleTrivia(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var trivia = tree.Root.LastToken(true, true).LeadingTrivia.First();
            return trivia;
        }

        public static ExpressionSyntax ParseExpression(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            return (ExpressionSyntax) tree.Root.Root;
        }

        public static SyntaxToken CreateToken(SyntaxKind kind, string text = null)
        {
            return LexSingleToken(text ?? kind.GetText());
        }

        public static Conversion ClassifyConversion(Type souceType, Type targetType)
        {
            var semanticModel = Compilation.Empty.GetSemanticModel();
            var conversion = semanticModel.ClassifyConversion(souceType, targetType);
            return conversion;
        }
    }
}