using System;

namespace NQuery.Syntax
{
    public abstract class JoinedTableReferenceSyntax : TableReferenceSyntax
    {
        internal JoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            Right = right;
        }

        public TableReferenceSyntax Left { get; }

        public TableReferenceSyntax Right { get; }
    }
}