using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public abstract class SyntaxNode
    {
        private bool? _isMissing;
        private TextSpan? _span;
        private TextSpan? _fullSpan;

        internal SyntaxNode(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        private TextSpan ComputeSpan()
        {
            var start = ChildNodesAndTokens().First().Span.Start;
            var end = ChildNodesAndTokens().Last().Span.End;
            return TextSpan.FromBounds(start, end);
        }

        private TextSpan ComputeFullSpan()
        {
            var start = ChildNodesAndTokens().First().FullSpan.Start;
            var end = ChildNodesAndTokens().Last().FullSpan.End;
            return TextSpan.FromBounds(start, end);
        }

        private bool ComputeIsMissing()
        {
            return ChildNodesAndTokens().All(n => n.IsMissing);
        }

        public IEnumerable<SyntaxNode> Ancestors(bool ascendOutOfTrivia = true)
        {
            return AncestorsAndSelf(ascendOutOfTrivia).Skip(1);
        }

        public IEnumerable<SyntaxNode> AncestorsAndSelf(bool ascendOutOfTrivia = true)
        {
            var node = this;
            while (node is not null)
            {
                yield return node;

                if (!ascendOutOfTrivia || node is not StructuredTriviaSyntax structuredTrivia)
                {
                    node = node.Parent;
                }
                else
                {
                    // The parent of structured trivia is actually null, which is
                    // arguably the correct result for nodes that are trivia.
                    // So in order to ascend out of those, we need to navigate from
                    // the structure to its containing token which we can then use
                    // to return the logical parent.
                    var parentTrivia = structuredTrivia.ParentTrivia;
                    var parentToken = parentTrivia.Parent;
                    node = parentToken.Parent;
                }
            }
        }

        public abstract IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens();

        public IEnumerable<SyntaxNode> ChildNodes()
        {
            return from n in ChildNodesAndTokens()
                   where n.IsNode
                   select n.AsNode();
        }

        public IEnumerable<SyntaxToken> ChildTokens()
        {
            return from n in ChildNodesAndTokens()
                   where n.IsToken
                   select n.AsToken();
        }

        public IEnumerable<SyntaxNode> DescendantNodes(bool descendIntoTrivia = false)
        {
            return DescendantNodesAndSelf(descendIntoTrivia).Skip(1);
        }

        public IEnumerable<SyntaxNode> DescendantNodesAndSelf(bool descendIntoTrivia = false)
        {
            return from n in DescendantNodesAndTokensAndSelf(descendIntoTrivia)
                   where n.IsNode
                   select n.AsNode();
        }

        public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokens(bool descendIntoTrivia = false)
        {
            return DescendantNodesAndTokensAndSelf(descendIntoTrivia).Skip(1);
        }

        public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensAndSelf(bool descendIntoTrivia = false)
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
                {
                    if (child.IsToken && descendIntoTrivia)
                    {
                        var token = child.AsToken();
                        var structures = token.TrailingTrivia.Select(t => t.Structure).Where(s => s is not null);
                        foreach (var structure in structures)
                            stack.Push(structure);
                    }

                    stack.Push(child);

                    if (child.IsToken && descendIntoTrivia)
                    {
                        var token = child.AsToken();
                        var structures = token.LeadingTrivia.Select(t => t.Structure).Where(s => s is not null);
                        foreach (var structure in structures)
                            stack.Push(structure);
                    }
                }
            }
        }

        public IEnumerable<SyntaxToken> DescendantTokens(bool descendIntoTrivia = false)
        {
            return from n in DescendantNodesAndTokens(descendIntoTrivia)
                   where n.IsToken
                   select n.AsToken();
        }

        public SyntaxToken FirstToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetFirstToken(this, includeZeroLength, includeSkippedTokens);
        }

        public SyntaxToken LastToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetLastToken(this, includeZeroLength, includeSkippedTokens);
        }

        public SyntaxToken FindToken(int position, bool descendIntoTrivia = false)
        {
            if (FullSpan.End == position)
            {
                if (this is CompilationUnitSyntax compilationUnit)
                    return compilationUnit.EndOfFileToken;
            }

            if (!FullSpan.Contains(position))
                throw new ArgumentOutOfRangeException(nameof(position));

            var child = (from nodeOrToken in ChildNodesAndTokens()
                         where nodeOrToken.FullSpan.Contains(position)
                         select nodeOrToken).First();

            if (child.IsNode)
                return child.AsNode().FindToken(position, descendIntoTrivia);

            var token = child.AsToken();

            if (descendIntoTrivia)
            {
                var triviaStructure = (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                                       where t.Structure is not null && t.Structure.FullSpan.Contains(position)
                                       select t.Structure).FirstOrDefault();

                if (triviaStructure is not null)
                    return triviaStructure.FindToken(position, true);
            }

            return token;
        }

        public bool IsEquivalentTo(SyntaxNode other)
        {
            ArgumentNullException.ThrowIfNull(other);

            return SyntaxTreeEquivalence.AreEquivalent(this, other);
        }

        public SyntaxTree SyntaxTree { get; }

        public SyntaxNode Parent
        {
            get { return SyntaxTree.GetParentNode(this); }
        }

        public abstract SyntaxKind Kind { get; }

        public TextSpan Span
        {
            get
            {
                _span ??= ComputeSpan();
                return _span.Value;
            }
        }

        public TextSpan FullSpan
        {
            get
            {
                _fullSpan ??= ComputeFullSpan();
                return _fullSpan.Value;
            }
        }

        public bool IsMissing
        {
            get
            {
                _isMissing ??= ComputeIsMissing();
                return _isMissing.Value;
            }
        }

        public void WriteTo(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

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
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}