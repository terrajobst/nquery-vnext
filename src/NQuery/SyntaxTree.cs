using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxTree
    {
        private Dictionary<object, object> _parentFromChild;

        private SyntaxTree(SourceText text, Func<Parser, CompilationUnitSyntax> parseMethod)
        {
            var parser = new Parser(text, this);
            Text = text;
            Root = parseMethod(parser);
        }

        public static SyntaxTree ParseQuery(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseQuery(sourceText);
        }

        public static SyntaxTree ParseQuery(SourceText text)
        {
            return new SyntaxTree(text, p => p.ParseRootQuery());
        }

        public static SyntaxTree ParseExpression(string source)
        {
            var textBuffer = SourceText.From(source);
            return ParseExpression(textBuffer);
        }

        public static SyntaxTree ParseExpression(SourceText sourceText)
        {
            return new SyntaxTree(sourceText, p => p.ParseRootExpression());
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return from token in Root.DescendantTokens(descendIntoTrivia: true)
                   let leadingDiagnostics = token.LeadingTrivia.SelectMany(t => t.Diagnostics)
                   let tokenDiagnostics = token.Diagnostics
                   let trailingDianostics = token.TrailingTrivia.SelectMany(t => t.Diagnostics)
                   from d in leadingDiagnostics.Concat(tokenDiagnostics).Concat(trailingDianostics)
                   select d;
        }

        private T GetParent<T>(object child)
             where T: class
        {
            if (_parentFromChild == null)
                Interlocked.CompareExchange(ref _parentFromChild, GetParents(Root), null);

            object parent;
            _parentFromChild.TryGetValue(child, out parent);
            return parent as T;
        }

        internal SyntaxNode GetParentNode(SyntaxNode node)
        {
            return GetParent<SyntaxNode>(node);
        }

        internal SyntaxNode GetParentNode(SyntaxToken token)
        {
            return GetParent<SyntaxNode>(token);
        }

        internal SyntaxToken GetParentToken(SyntaxTrivia trivia)
        {
            return GetParent<SyntaxToken>(trivia);
        }

        internal SyntaxTrivia GetParentTrivia(StructuredTriviaSyntax structuredTrivia)
        {
            return GetParent<SyntaxTrivia>(structuredTrivia);
        }

        private static Dictionary<object,object> GetParents(SyntaxNode compilationUnit)
        {
            var result = new Dictionary<object, object>();
            GetParents(result, compilationUnit);
            return result;
        }

        private static void GetParents(IDictionary<object, object> parents, SyntaxNode parent)
        {
            foreach (var nodeOrToken in parent.ChildNodesAndTokens())
            {
                if (nodeOrToken.IsNode)
                {
                    var node = nodeOrToken.AsNode();
                    parents.Add(node, parent);
                    GetParents(parents, node);
                }
                else
                {
                    var token = nodeOrToken.AsToken();
                    parents.Add(token, parent);

                    foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
                    {
                        parents.Add(trivia, token);
                        var structure = trivia.Structure;
                        if (structure != null)
                        {
                            parents.Add(structure, trivia);
                            GetParents(parents, structure);
                        }
                    }
                }
            }
        }

        public SyntaxTree WithChanges(params TextChange[] textChanges)
        {
            if (textChanges == null)
                return this;

            return WithChanges(textChanges.AsEnumerable());
        }

        public SyntaxTree WithChanges(IEnumerable<TextChange> textChanges)
        {
            var newText = Text.WithChanges(textChanges);
            if (newText == Text)
                return this;

            var isQuery = Root.Root is QuerySyntax;
            return isQuery
                ? ParseQuery(newText)
                : ParseExpression(newText);
        }

        public static SyntaxTree Empty = ParseQuery(string.Empty);

        public CompilationUnitSyntax Root { get; }

        public SourceText Text { get; }
    }
}