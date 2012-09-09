using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class OrderedQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _orderKeyword;
        private readonly SyntaxToken _byKeyword;
        private readonly SeparatedSyntaxList<OrderByColumnSyntax> _columns;

        public OrderedQuerySyntax(SyntaxTree syntaxTree, QuerySyntax query, SyntaxToken orderKeyword, SyntaxToken byKeyword, SeparatedSyntaxList<OrderByColumnSyntax> columns)
            : base(syntaxTree)
        {
            _query = query;
            _orderKeyword = orderKeyword.WithParent(this);
            _byKeyword = byKeyword.WithParent(this);
            _columns = columns.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _query;
            yield return _orderKeyword;
            yield return _byKeyword;
            foreach (var nodeOrToken in _columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken OrderKeyword
        {
            get { return _orderKeyword; }
        }

        public SyntaxToken ByKeyword
        {
            get { return _byKeyword; }
        }

        public SeparatedSyntaxList<OrderByColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}