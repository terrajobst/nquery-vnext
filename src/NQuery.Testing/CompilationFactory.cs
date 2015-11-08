using System;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery
{
    public static class CompilationFactory
    {
        private static readonly DataContext DataContext = NorthwindDataContext.Instance;

        public static Compilation CreateQuery(string query)
        {
            var syntaxTree = SyntaxTree.ParseQuery(query);
            return Compilation.Create(DataContext, syntaxTree);
        }

        public static Compilation CreateQuery(string textWithPipe, out int position)
        {
            var text = textWithPipe.ParseSinglePosition(out position);
            return CreateQuery(text);
        }

        public static Compilation CreateQuery(string textWithMarkers, out TextSpan span)
        {
            var text = textWithMarkers.ParseSingleSpan(out span);
            return CreateQuery(text);
        }

        public static Compilation CreateQuery(string textWithMarkers, out ImmutableArray<TextSpan> spans)
        {
            var text = textWithMarkers.ParseSpans(out spans);
            return CreateQuery(text);
        }

        public static Compilation CreateExpression(string text)
        {
            var syntaxTree = SyntaxTree.ParseExpression(text);
            return Compilation.Create(DataContext, syntaxTree);
        }

        public static Compilation CreateExpression(string textWithPipe, out int position)
        {
            var text = textWithPipe.ParseSinglePosition(out position);
            return CreateExpression(text);
        }

        public static Compilation CreateExpression(string textWithMarkers, out TextSpan span)
        {
            var text = textWithMarkers.ParseSingleSpan(out span);
            return CreateExpression(text);
        }

        public static Compilation CreateExpression(string textWithMarkers, out ImmutableArray<TextSpan> spans)
        {
            var text = textWithMarkers.ParseSpans(out spans);
            return CreateExpression(text);
        }
    }
}