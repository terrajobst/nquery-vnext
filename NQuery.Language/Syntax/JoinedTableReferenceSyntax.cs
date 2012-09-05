using System;

namespace NQuery.Language
{
    public abstract class JoinedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly TableReferenceSyntax _left;
        private readonly TableReferenceSyntax _right;

        protected JoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right, SyntaxToken? commaToken)
            : base(syntaxTree, commaToken)
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