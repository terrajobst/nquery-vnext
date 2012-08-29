using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        private TextSpan? _span;
        private TextSpan? _fullSpan;

        public abstract IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens(); 

        private TextSpan ComputeSpan()
        {
            if (!ChildNodesAndTokens().Any())
                return new TextSpan(0, 0);
            
            var start = ChildNodesAndTokens().First().Span.Start;
            var end = ChildNodesAndTokens().Last().Span.End;
            return TextSpan.FromBounds(start, end);
        }

        private TextSpan ComputeFullSpan()
        {
            if (!ChildNodesAndTokens().Any())
                return new TextSpan(0, 0);
            
            var start = ChildNodesAndTokens().First().FullSpan.Start;
            var end = ChildNodesAndTokens().Last().FullSpan.End;
            return TextSpan.FromBounds(start, end);
        }

        public abstract SyntaxKind Kind { get; }

        public TextSpan Span
        {
            get
            {
                if (_span == null)
                    _span = ComputeSpan();

                return _span.Value;
            }
        }

        public TextSpan FullSpan
        {
            get
            {
                if (_fullSpan == null)
                    _fullSpan = ComputeFullSpan();

                return _fullSpan.Value;
            }
        }
    }
}