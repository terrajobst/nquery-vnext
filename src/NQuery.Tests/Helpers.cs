using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Tests
{
    internal static class Helpers
    {
        public static SyntaxToken LexSingleToken(string text)
        {
            var tree = SyntaxTree.ParseExpression(text);
            var token = tree.Root.DescendantTokens(true)
                                 .Where(t => !t.IsMissing)
                                 .ToImmutableArray();

            if (token.Length == 1)
                return token.Single();

            return token.First().Kind == SyntaxKind.EndOfFileToken
                ? token.Last()
                : token.First();
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