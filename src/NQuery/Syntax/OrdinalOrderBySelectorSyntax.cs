using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class OrdinalOrderBySelectorSyntax : OrderBySelectorSyntax
    {
        private readonly SyntaxToken _ordinal;

        public OrdinalOrderBySelectorSyntax(SyntaxTree syntaxTree, SyntaxToken ordinal)
            : base(syntaxTree)
        {
            _ordinal = ordinal;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrdinalOrderBySelector; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _ordinal;
        }

        public SyntaxToken Ordinal
        {
            get { return _ordinal; }
        }
    }
}