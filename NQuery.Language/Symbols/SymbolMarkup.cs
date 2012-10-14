using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Language.Symbols
{
    public sealed class SymbolMarkup
    {
        private readonly ReadOnlyCollection<SymbolMarkupNode> _nodes;

        public SymbolMarkup(IList<SymbolMarkupNode> nodes)
        {
            _nodes = new ReadOnlyCollection<SymbolMarkupNode>(nodes);
        }

        public ReadOnlyCollection<SymbolMarkupNode> Nodes
        {
            get { return _nodes; }
        }

        public override string ToString()
        {
            return string.Concat(_nodes.Select(n => n.Text));
        }

        public static SymbolMarkup FromSymbol(Symbol symbol)
        {
            var nodes = new List<SymbolMarkupNode>();
            nodes.AppendSymbol(symbol);
            return new SymbolMarkup(nodes);
        }
    }
}