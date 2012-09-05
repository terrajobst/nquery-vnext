using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NQuery.Language
{
    public sealed class SyntaxTree
    {
        private readonly CompilationUnitSyntax _root;
        private readonly TextBuffer _textBuffer;

        private Dictionary<SyntaxNode, SyntaxNode> _parentNodes;

        private SyntaxTree(string source, Func<Parser, CompilationUnitSyntax> parseMethod)
        {
            var parser = new Parser(source, this);
            _root = parseMethod(parser);
            _textBuffer = new TextBuffer(source);;
        }

        public static SyntaxTree ParseQuery(string source)
        {
            return new SyntaxTree(source, p => p.ParseRootQuery());
        }

        public static SyntaxTree ParseExpression(string source)
        {
            return new SyntaxTree(source, p => p.ParseRootExpression());
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return from token in Root.DescendantTokens()
                   let leadingDiagnostics = token.LeadingTrivia.SelectMany(t => t.Diagnostics)
                   let tokenDiagnostics = token.Diagnostics
                   let trailingDianostics = token.TrailingTrivia.SelectMany(t => t.Diagnostics)
                   from d in leadingDiagnostics.Concat(tokenDiagnostics).Concat(trailingDianostics)
                   select d;
        }

        internal SyntaxNode GetParent(SyntaxNode syntaxNode)
        {
            if (_parentNodes == null)
                Interlocked.CompareExchange(ref _parentNodes, GetParents(_root), null);

            SyntaxNode parent;
            _parentNodes.TryGetValue(syntaxNode, out parent);
            return parent;
        }

        private static Dictionary<SyntaxNode,SyntaxNode> GetParents(SyntaxNode compilationUnit)
        {
            var result = new Dictionary<SyntaxNode, SyntaxNode>();
            result.Add(compilationUnit, null);
            GetParents(result, compilationUnit);
            return result;
        }

        private static void GetParents(IDictionary<SyntaxNode, SyntaxNode> parents, SyntaxNode parent)
        {
            foreach (var childNode in parent.ChildNodes())
            {
                parents.Add(childNode, parent);
                GetParents(parents, childNode);
            }
        }

        public static SyntaxTree Empty = ParseQuery(string.Empty);

        public CompilationUnitSyntax Root
        {
            get { return _root; }
        }

        public TextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }
    }
}