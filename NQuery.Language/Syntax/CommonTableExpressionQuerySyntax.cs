using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _withKeyword;
        private readonly ReadOnlyCollection<CommonTableExpressionSyntax> _commonTableExpressions;
        private readonly QuerySyntax _query;

        public CommonTableExpressionQuerySyntax(SyntaxToken withKeyword, IList<CommonTableExpressionSyntax> commonTableExpressions, QuerySyntax query)
        {
            _withKeyword = withKeyword;
            _query = query;
            _commonTableExpressions = new ReadOnlyCollection<CommonTableExpressionSyntax>(commonTableExpressions);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _withKeyword;

            foreach (var commonTableExpression in _commonTableExpressions)
                yield return commonTableExpression;

            yield return _query;
        }

        public SyntaxToken WithKeyword
        {
            get { return _withKeyword; }
        }

        public ReadOnlyCollection<CommonTableExpressionSyntax> CommonTableExpressions
        {
            get { return _commonTableExpressions; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }
    }
}