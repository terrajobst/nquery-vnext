using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraMethodInvocationExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _target;
        private readonly ReadOnlyCollection<AlgebraExpression> _arguments;
        private readonly MethodSymbol _symbol;

        public AlgebraMethodInvocationExpression(AlgebraExpression target, IList<AlgebraExpression> arguments, MethodSymbol symbol)
        {
            _target = target;
            _arguments = new ReadOnlyCollection<AlgebraExpression>(arguments);
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.MethodInvocationExpression; }
        }

        public AlgebraExpression Target
        {
            get { return _target; }
        }

        public ReadOnlyCollection<AlgebraExpression> Arguments
        {
            get { return _arguments; }
        }

        public MethodSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}