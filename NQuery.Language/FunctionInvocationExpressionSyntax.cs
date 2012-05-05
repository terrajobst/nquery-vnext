using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class FunctionInvocationExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly ArgumentListSyntax _arguments;

        public FunctionInvocationExpressionSyntax(SyntaxToken identifier, ArgumentListSyntax arguments)
        {
            _identifier = identifier;
            _arguments = arguments;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FunctionInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            yield return _arguments;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public ArgumentListSyntax Arguments
        {
            get { return _arguments; }
        }
    }
}