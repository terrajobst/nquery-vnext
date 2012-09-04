using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        private TextSpan? _span;
        private TextSpan? _fullSpan;

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

        public abstract IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens();

        public IEnumerable<SyntaxNode> ChildNodes()
        {
            return from n in ChildNodesAndTokens()
                   where n.IsNode
                   select n.AsNode();
        }

        public IEnumerable<SyntaxNode> DescendantNodes()
        {
            return DescendantNodesAndSelf().Skip(1);
        }

        public IEnumerable<SyntaxNode> DescendantNodesAndSelf()
        {
            return from n in DescendantNodesAndTokensAndSelf()
                   where n.IsNode
                   select n.AsNode();
        }
        
        public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokens()
        {
            return DescendantNodesAndTokensAndSelf().Skip(1);
        }
        
        public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensAndSelf()
        {
            var stack = new Stack<SyntaxNodeOrToken>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                if (!current.IsNode)
                    continue;

                foreach (var child in current.AsNode().ChildNodesAndTokens().Reverse())
                    stack.Push(child);
            }
        }

        public IEnumerable<SyntaxToken> DescendantTokens()
        {
            return from n in DescendantNodesAndTokens()
                   where n.IsToken
                   select n.AsToken();
        }

        public SyntaxToken FirstToken()
        {
            return DescendantTokens().First();
        }

        public SyntaxToken LastToken()
        {
            return DescendantTokens().Last();
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

        public void WriteTo(TextWriter writer)
        {
            foreach (var syntaxNode in ChildNodesAndTokens())
            {
                if (syntaxNode.IsToken)   
                    syntaxNode.AsToken().WriteTo(writer);
                else
                    syntaxNode.AsNode().WriteTo(writer);
            }
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}