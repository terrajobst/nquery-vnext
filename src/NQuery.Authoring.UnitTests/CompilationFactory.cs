using System;

using NQuery.Data.Samples;

namespace NQuery.Authoring.UnitTests
{
    internal static class CompilationFactory
    {
        private static readonly DataContext DataContext = DataContextFactory.CreateNorthwind();

        public static Compilation CreateQuery(string query)
        {
            int position;
            return CreateQuery(query, out position);
        }

        public static Compilation CreateQuery(string query, out int position)
        {
            position = query.IndexOf('|');
            if (position >= 0)
                query = query.Remove(position, 1);

            var syntaxTree = SyntaxTree.ParseQuery(query);
            return new Compilation(DataContext, syntaxTree);
        }
    }
}