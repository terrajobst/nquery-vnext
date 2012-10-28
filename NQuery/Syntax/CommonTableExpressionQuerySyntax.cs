using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _withKeyword;
        private readonly SeparatedSyntaxList<CommonTableExpressionSyntax> _commonTableExpressions;
        private readonly QuerySyntax _query;

        public CommonTableExpressionQuerySyntax(SyntaxTree syntaxTree, SyntaxToken withKeyword, SeparatedSyntaxList<CommonTableExpressionSyntax> commonTableExpressions, QuerySyntax query)
            : base(syntaxTree)
        {
            _withKeyword = withKeyword;
            _query = query;
            _commonTableExpressions = commonTableExpressions;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _withKeyword;

            foreach (var nodeOrToken in _commonTableExpressions.GetWithSeparators())
                yield return nodeOrToken;

            yield return _query;
        }

        public SyntaxToken WithKeyword
        {
            get { return _withKeyword; }
        }

        public SeparatedSyntaxList<CommonTableExpressionSyntax> CommonTableExpressions
        {
            get { return _commonTableExpressions; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }
    }
}