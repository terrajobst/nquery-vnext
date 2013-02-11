using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundMethodInvocationExpression : BoundExpression
    {
        private readonly BoundExpression _target;
        private readonly ReadOnlyCollection<BoundExpression> _arguments;
        private readonly OverloadResolutionResult<MethodSymbolSignature> _result;

        public BoundMethodInvocationExpression(BoundExpression target, IList<BoundExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
        {
            _target = target;
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
            _result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.MethodInvocationExpression; }
        }

        public override Type Type
        {
            get { return Symbol == null ? TypeFacts.Unknown : Symbol.Type; }
        }

        public MethodSymbol Symbol
        {
            get { return _result.Selected == null ? null : _result.Selected.Signature.Symbol; }
        }

        public BoundExpression Target
        {
            get { return _target; }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }

        public OverloadResolutionResult<MethodSymbolSignature> Result
        {
            get { return _result; }
        }

        public BoundExpression Update(BoundExpression target, BoundExpression[] arguments)
        {
            return (_target == target && _arguments.SequenceEqual(arguments))
                       ? this
                       : new BoundMethodInvocationExpression(target, arguments, _result);
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}({2})", _target, Symbol.Name, string.Join(", ", _arguments));
        }
    }
}