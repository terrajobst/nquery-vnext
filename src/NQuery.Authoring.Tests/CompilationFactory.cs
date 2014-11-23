using System;

using NQuery.Data.Samples;
using NQuery.Text;

namespace NQuery.Authoring.Tests
{
    internal static class CompilationFactory
    {
        private static readonly DataContext DataContext = DataContextFactory.CreateNorthwind();

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
    }
}