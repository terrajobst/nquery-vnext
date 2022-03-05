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
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var sourceText = SourceText.From(text);
            return ParseQuery(sourceText);
        }

        public static SyntaxTree ParseQuery(SourceText text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            return new SyntaxTree(text, p => p.ParseRootQuery());
        }

        public static SyntaxTree ParseExpression(string source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var textBuffer = SourceText.From(source);
            return ParseExpression(textBuffer);
        }

        public static SyntaxTree ParseExpression(SourceText sourceText)
        {
            if (sourceText is null)
                throw new ArgumentNullException(nameof(sourceText));

            return new SyntaxTree(sourceText, p => p.ParseRootExpression());
        }

        public static SyntaxTree ParseTokens(string source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var textBuffer = SourceText.From(source);
            return ParseTokens(textBuffer);
        }

        public static SyntaxTree ParseTokens(SourceText sourceText)
        {
            if (sourceText is null)
                throw new ArgumentNullException(nameof(sourceText));

            return new SyntaxTree(sourceText, p => p.ParseRootTokens());
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return from token in Root.DescendantTokens(descendIntoTrivia: true)
                   let leadingDiagnostics = token.LeadingTrivia.SelectMany(t => t.Diagnostics)
                   let tokenDiagnostics = token.Diagnostics
                   let trailingDiagnostics = token.TrailingTrivia.SelectMany(t => t.Diagnostics)
                   from d in leadingDiagnostics.Concat(tokenDiagnostics).Concat(trailingDiagnostics)
                   select d;
        }

        private T GetParent<T>(object child)
             where T : class
        {
            if (_parentFromChild is null)
                Interlocked.CompareExchange(ref _parentFromChild, GetParents(Root), null);

            _parentFromChild.TryGetValue(child, out object parent);
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

        private static Dictionary<object, object> GetParents(SyntaxNode compilationUnit)
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
                        if (structure is not null)
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
            if (textChanges is null)
                return this;

            return WithChanges(textChanges.AsEnumerable());
        }

        public SyntaxTree WithChanges(IEnumerable<TextChange> textChanges)
        {
            if (textChanges is null)
                throw new ArgumentNullException(nameof(textChanges));

            var newText = Text.WithChanges(textChanges);
            if (newText == Text)
                return this;

            var isExpression = Root.Root is ExpressionSyntax;
            return isExpression
                ? ParseExpression(newText)
                : ParseQuery(newText);
        }

        public static readonly SyntaxTree Empty = ParseQuery(string.Empty);

        public CompilationUnitSyntax Root { get; }

        public SourceText Text { get; }
    }
}