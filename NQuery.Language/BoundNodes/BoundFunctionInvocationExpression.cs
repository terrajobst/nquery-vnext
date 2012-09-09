using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundFunctionInvocationExpression : BoundExpression
    {
        private readonly ReadOnlyCollection<BoundExpression> _arguments;
        private readonly FunctionSymbol _functionSymbol;

        public BoundFunctionInvocationExpression(IList<BoundExpression> arguments, FunctionSymbol functionSymbol)
        {
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
            _functionSymbol = functionSymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.FunctionInvocationExpression; }
        }

        public override Symbol Symbol
        {
            get { return _functionSymbol; }
        }

        public override Type Type
        {
            get { return _functionSymbol.Type; }
        }

        public FunctionSymbol FunctionSymbol
        {
            get { return _functionSymbol; }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }
    }
}