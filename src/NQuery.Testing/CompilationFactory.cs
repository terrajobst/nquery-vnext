using System;

using NQuery.Text;

namespace NQuery
{
    public static class CompilationFactory
    {
        private static readonly DataContext DataContext = NorthwindDataContext.Instance;

        public static Compilation CreateQuery(string query)
        {
            var syntaxTree = SyntaxTree.ParseQuery(query);
            return new Compilation(DataContext, syntaxTree);
        }

        public static Compilation CreateQuery(string queryWithPipe, out int position)
        {
            var query = queryWithPipe.ParseSinglePosition(out position);
            return CreateQuery(query);
        }

        public static Compilation CreateQuery(string queryWithMarkers, out TextSpan span)
        {
            var query = queryWithMarkers.ParseSingleSpan(out span);
            return CreateQuery(query);
        }

        public static Compilation CreateExpression(string query)
        {
            var syntaxTree = SyntaxTree.ParseExpression(query);
            return new Compilation(DataContext, syntaxTree);
        }
    }
}