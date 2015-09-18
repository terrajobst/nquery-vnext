using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundFunctionInvocationExpression : BoundExpression
    {
        private readonly ImmutableArray<BoundExpression> _arguments;
        private readonly OverloadResolutionResult<FunctionSymbolSignature> _result;

        public BoundFunctionInvocationExpression(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            _arguments = arguments.ToImmutableArray();
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

        public ImmutableArray<BoundExpression> Arguments
        {
            get { return _arguments; }
        }

        public OverloadResolutionResult<FunctionSymbolSignature> Result
        {
            get { return _result; }
        }

        public BoundFunctionInvocationExpression Update(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (newArguments == _arguments && result == _result)
                return this;

            return new BoundFunctionInvocationExpression(newArguments, result);
        }

        public override string ToString()
        {
            return $"{Symbol.Name}({string.Join(",", _arguments)})";
        }
    }
}