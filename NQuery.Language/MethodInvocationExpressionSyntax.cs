using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class MethodInvocationExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _target;
        private readonly SyntaxToken _dot;
        private readonly SyntaxToken _name;
        private readonly ArgumentListSyntax _argumentList;

        public MethodInvocationExpressionSyntax(ExpressionSyntax target, SyntaxToken dot, SyntaxToken name, ArgumentListSyntax argumentList)
        {
            _target = target;
            _dot = dot;
            _name = name;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.MethodInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _target;
            yield return _dot;
            yield return _name;
            yield return _argumentList;
        }

        public ExpressionSyntax Target
        {
            get { return _target; }
        }

        public SyntaxToken Dot
        {
            get { return _dot; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }
}