using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraFunctionInvocationExpression : AlgebraExpression
    {
        private readonly ReadOnlyCollection<AlgebraExpression> _arguments;
        private readonly FunctionSymbol _symbol;

        public AlgebraFunctionInvocationExpression(IList<AlgebraExpression> arguments, FunctionSymbol symbol)
        {
            _arguments = new ReadOnlyCollection<AlgebraExpression>(arguments);
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.FunctionInvocationExpression; }
        }

        public ReadOnlyCollection<AlgebraExpression> Arguments
        {
            get { return _arguments; }
        }

        public FunctionSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}