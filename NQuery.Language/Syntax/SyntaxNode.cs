using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        private readonly SyntaxTree _syntaxTree;
        private TextSpan? _span;
        private TextSpan? _fullSpan;

        protected SyntaxNode(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

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

        public IEnumerable<SyntaxNode> Ancestors()
        {
            var node = this;
            while (node.Parent != null)
            {
                yield return node.Parent;
                node = node.Parent;
            }
        }

        public IEnumerable<SyntaxNode> AncestorsAndSelf()
        {
            var node = this;
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
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

        public SyntaxToken FindToken(int position)
        {
            // TODO: We should consider a different contract for this method.
            //
            // Right now we use the following contract:
            //
            // (1) If the position matches the full span of any token, this token is returned.
            // (2) If position is at the end, the EndOfFileToken is returned.
            //
            // Virtually all our callers want to match the last token in situations like this:
            //
            // SELECT Foo, Bar,|EndOfFile
            //
            // (the bar indicates the position). However, they get the EoF token.
            //
            // We may want to introduce FindTouchedToken(int position).

            // TODO: The following would be more correct, but doesn't work because our current IntelliSense implementation blows up.
            //
            // The reason is that we currently don't gurantee to have the latest syntax tree when any of our editor extensions
            // are invoked.
            //
            //if (FullSpan.End == position)
            //{
            //    var compilationUnit = this as CompilationUnitSyntax;
            //    if (compilationUnit != null)
            //        return compilationUnit.EndOfFileToken;
            //}
            //
            //if (!FullSpan.Contains(position))
            //    throw new ArgumentOutOfRangeException("position");
            //
            // So instead we do this:
            if (!FullSpan.Contains(position))
                return SyntaxTree.Root.EndOfFileToken;

            var child = (from nodeOrToken in ChildNodesAndTokens()
                         where nodeOrToken.FullSpan.Contains(position)
                         select nodeOrToken).First();
            
            return child.IsToken
                       ? child.AsToken()
                       : child.AsNode().FindToken(position);
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public SyntaxNode Parent
        {
            get { return _syntaxTree.GetParent(this); }
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