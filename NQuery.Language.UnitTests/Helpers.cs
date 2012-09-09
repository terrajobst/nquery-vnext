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
    }
}