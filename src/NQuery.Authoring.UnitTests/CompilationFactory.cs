using System;

using NQuery.Data.Samples;
using NQuery.Text;

namespace NQuery.Authoring.UnitTests
{
    internal static class CompilationFactory
    {
        private static readonly DataContext DataContext = DataContextFactory.CreateNorthwind();

        public static Compilation CreateQuery(string query)
        {
            var syntaxTree = SyntaxTree.ParseQuery(query);
            return new Compilation(DataContext, syntaxTree);
        }

        public static Compilation CreateQuery(string query, out int position)
        {
            position = query.IndexOf('|');
            if (position < 0)
                throw new ArgumentException("The position must be marked with a pipe, such as 'SELECT e.Empl|oyeeId'");

            query = query.Remove(position, 1);
            return CreateQuery(query);
        }

        public static Compilation CreateQuery(string query, out TextSpan span)
        {
            var start = query.IndexOf('{');
            var end = query.IndexOf('}') - 1;
            if (start < 0 || end < 0)
                throw new ArgumentException("The span must be marked with braces, such as 'SELECT {e.EmployeeId}'");

            span = TextSpan.FromBounds(start, end);
            query = query.Remove(start, 1).Remove(end, 1);
            return CreateQuery(query);
        }
    }
}