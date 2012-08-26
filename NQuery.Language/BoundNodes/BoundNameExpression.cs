using System;
using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundNameExpression : BoundExpression
    {
        private readonly Symbol _symbol;
        private readonly IEnumerable<Symbol> _candidates;

        public BoundNameExpression(Symbol symbol, IEnumerable<Symbol> candidates)
        {
            _symbol = symbol;
            _candidates = candidates;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.NameExpression; }
        }

        public override Symbol Symbol
        {
            get { return _symbol; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return _candidates; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }
    }
}