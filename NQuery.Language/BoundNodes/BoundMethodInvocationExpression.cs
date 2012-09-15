using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language.Binding;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
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

        public override Symbol Symbol
        {
            get { return _result.Selected == null ? null : _result.Selected.Signature.Symbol; }
        }

        public override Type Type
        {
            get { return Symbol == null ? KnownTypes.Unknown : Symbol.Type; }
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
    }
}