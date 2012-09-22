using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundFunctionInvocationExpression : BoundExpression
    {
        private readonly ReadOnlyCollection<BoundExpression> _arguments;
        private readonly OverloadResolutionResult<FunctionSymbolSignature> _result;

        public BoundFunctionInvocationExpression(IList<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
            _result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.FunctionInvocationExpression; }
        }

        public override Type Type
        {
            get { return Symbol == null ? TypeFacts.Unknown : Symbol.Type; }
        }

        public FunctionSymbol Symbol
        {
            get { return _result.Selected == null ? null : _result.Selected.Signature.Symbol; }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }

        public OverloadResolutionResult<FunctionSymbolSignature> Result
        {
            get { return _result; }
        }

        public BoundExpression Update(BoundExpression[] newArguments)
        {
            return _arguments.SequenceEqual(newArguments)
                       ? this
                       : new BoundFunctionInvocationExpression(newArguments, _result);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Symbol.Name, string.Join(",", _arguments));
        }
    }
}