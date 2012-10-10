using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NQuery.Language
{
    public abstract class SyntaxNode
    {
        private readonly SyntaxTree _syntaxTree;
        private bool? _isMissing;
        private TextSpan? _span;
        private TextSpan? _fullSpan;

        protected SyntaxNode(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
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
            while (node != null && (ascendOutOfTrivia || !(node is StructuredTriviaSyntax)))
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
                        var structures = token.LeadingTrivia.Select(t => t.Structure).Where(s => s != null);
                        foreach (var structure in structures)
                            stack.Push(structure);
                    }

                    stack.Push(child);

                    if (child.IsToken && descendIntoTrivia)
                    {
                        var token = child.AsToken();
                        var structures = token.TrailingTrivia.Select(t => t.Structure).Where(s => s != null);
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
                var compilationUnit = this as CompilationUnitSyntax;
                if (compilationUnit != null)
                    return compilationUnit.EndOfFileToken;
            }

            if (!FullSpan.Contains(position))
                throw new ArgumentOutOfRangeException("position");

            var child = (from nodeOrToken in ChildNodesAndTokens()
                         where nodeOrToken.FullSpan.Contains(position)
                         select nodeOrToken).First();

            if (child.IsNode)
                return child.AsNode().FindToken(position);

            var token = child.AsToken();

            if (descendIntoTrivia)
            {
                var triviaStructure = (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                                       where t.Structure != null && t.Structure.FullSpan.Contains(position)
                                       select t.Structure).FirstOrDefault();

                if (triviaStructure != null)
                    return triviaStructure.FindToken(position);
            }

            return token;
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public SyntaxNode Parent
        {
            get { return _syntaxTree.GetParentNode(this); }
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

        public bool IsMissing
        {
            get
            {
                if (_isMissing == null)
                    _isMissing = ComputeIsMissing();

                return _isMissing.Value;
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