using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundNameExpression : BoundExpression
    {
        private readonly Symbol _symbol;
        private readonly ReadOnlyCollection<Symbol> _candidates;

        public BoundNameExpression(Symbol symbol)
            : this(symbol, new Symbol[0])
        {
        }

        public BoundNameExpression(Symbol symbol, IEnumerable<Symbol> candidates)
        {
            _symbol = symbol;
            _candidates = new ReadOnlyCollection<Symbol>(candidates.ToArray());
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.NameExpression; }
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public ReadOnlyCollection<Symbol> Candidates
        {
            get { return _candidates; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}