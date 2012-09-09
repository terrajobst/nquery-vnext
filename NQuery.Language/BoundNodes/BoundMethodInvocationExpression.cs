using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundMethodInvocationExpression : BoundExpression
    {
        private readonly BoundExpression _target;
        private readonly ReadOnlyCollection<BoundExpression> _arguments;
        private readonly MethodSymbol _methodSymbol;

        public BoundMethodInvocationExpression(BoundExpression target, IList<BoundExpression> arguments, MethodSymbol methodSymbol)
        {
            _target = target;
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
            _methodSymbol = methodSymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.MethodInvocationExpression; }
        }

        public override Symbol Symbol
        {
            get { return _methodSymbol; }
        }

        public override Type Type
        {
            get { return _methodSymbol.Type; }
        }

        public BoundExpression Target
        {
            get { return _target; }
        }

        public MethodSymbol MethodSymbol
        {
            get { return _methodSymbol; }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }
    }
}