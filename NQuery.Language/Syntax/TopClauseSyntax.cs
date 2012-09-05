using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class TopClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _topKeyword;
        private readonly SyntaxToken _value;
        private readonly SyntaxToken? _withKeyword;
        private readonly SyntaxToken? _tiesKeyword;

        public TopClauseSyntax(SyntaxTree syntaxTree, SyntaxToken topKeyword, SyntaxToken value, SyntaxToken? withKeyword, SyntaxToken? tiesKeyword)
            : base(syntaxTree)
        {
            _topKeyword = topKeyword.WithParent(this);
            _value = value.WithParent(this);
            _withKeyword = withKeyword.WithParent(this);
            _tiesKeyword = tiesKeyword.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TopClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _topKeyword;
            yield return _value;
            if (_withKeyword != null)
                yield return _withKeyword.Value;
            if (_tiesKeyword != null)
                yield return _tiesKeyword.Value;
        }

        public SyntaxToken TopKeyword
        {
            get { return _topKeyword; }
        }

        public SyntaxToken Value
        {
            get { return _value; }
        }

        public SyntaxToken? WithKeyword
        {
            get { return _withKeyword; }
        }

        public SyntaxToken? TiesKeyword
        {
            get { return _tiesKeyword; }
        }
    }
}