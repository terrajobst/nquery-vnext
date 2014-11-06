using System;

using NQuery.Data.Samples;

namespace NQuery.UnitTests
{
    internal static class CompilationFactory
    {
        private static readonly DataContext DataContext = DataContextFactory.CreateNorthwind();

        public static Compilation CreateQuery(string query)
        {
            var syntaxTree = SyntaxTree.ParseQuery(query);
            return new Compilation(DataContext, syntaxTree);
        }

        public static Compilation CreateExpression(string query)
        {
            var syntaxTree = SyntaxTree.ParseExpression(query);
            return new Compilation(DataContext, syntaxTree);
        }
    }
}