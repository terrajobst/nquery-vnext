using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundVariableExpression :  BoundExpression
    {
        private readonly VariableSymbol _variableSymbol;

        public BoundVariableExpression(VariableSymbol variableSymbol)
        {
            _variableSymbol = variableSymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.VariableExpression; }
        }

        public override Symbol Symbol
        {
            get { return _variableSymbol; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>(); }
        }

        public override Type Type
        {
            get { return _variableSymbol.Type; }
        }
    }
}