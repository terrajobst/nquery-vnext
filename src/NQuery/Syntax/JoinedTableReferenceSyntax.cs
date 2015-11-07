using System;

namespace NQuery.Syntax
{
    public abstract class JoinedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly TableReferenceSyntax _left;
        private readonly TableReferenceSyntax _right;

        internal JoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right)
            : base(syntaxTree)
        {
            _left = left;
            _right = right;
        }

        public TableReferenceSyntax Left
        {
            get { return _left; }
        }

        public TableReferenceSyntax Right
        {
            get { return _right; }
        }
    }
}