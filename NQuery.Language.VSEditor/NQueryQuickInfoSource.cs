using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

using NQuery.Language.Semantic;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryQuickInfoSource : IQuickInfoSource
    {
        private readonly INQuerySemanticModelManager _semanticModelManager;

        public NQueryQuickInfoSource(INQuerySemanticModelManager semanticModelManager)
        {
            _semanticModelManager = semanticModelManager;
        }

        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            var semanticModel = _semanticModelManager.SemanticModel;
            if (semanticModel == null)
                return;

            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(currentSnapshot);
            if (triggerPoint == null)
                return;

            var syntaxTree = semanticModel.Compilation.SyntaxTree;

            var position = triggerPoint.Value.Position;
            var result = FindNodeWithSymbol(syntaxTree.Root, position, semanticModel);
            if (result == null)
                return;

            var textSpan = result.Context.Span;
            var symbol = result.Symbol;
            var span = new Span(textSpan.Start, textSpan.Length);

            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(symbol.Name + ": " + symbol.Kind.ToString().ToUpper());
        }

        private sealed class SyntaxResult
        {
            private readonly SyntaxNodeOrToken _context;
            private readonly Symbol _symbol;

            public SyntaxResult(SyntaxNodeOrToken context, Symbol symbol)
            {
                _context = context;
                _symbol = symbol;
            }

            public SyntaxNodeOrToken Context
            {
                get { return _context; }
            }

            public Symbol Symbol
            {
                get { return _symbol; }
            }
        }

        private static SyntaxResult FindNodeWithSymbol(SyntaxNode root, int position, SemanticModel semanticModel)
        {
            if (root == null || !root.Span.Contains(position))
                return null;

            var nodes = root.GetChildren()
                .SkipWhile(n => !n.Span.Contains(position))
                .TakeWhile(n => n.Span.Contains(position))
                .Where(n => n.IsNode)
                .Select(n => n.AsNode());

            foreach (var node in nodes)
            {
                var result = FindNodeWithSymbol(node, position, semanticModel) ?? GetSymbol(position, semanticModel, node);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static SyntaxResult GetSymbol(int position, SemanticModel semanticModel, SyntaxNode syntaxNode)
        {
            var propertyAccessSyntax = syntaxNode as PropertyAccessExpressionSyntax;
            if (propertyAccessSyntax != null)
                return GetSymbol(position, semanticModel, propertyAccessSyntax);

            var expressionNode = syntaxNode as ExpressionSyntax;
            if (expressionNode != null)
                return GetSymbol(semanticModel, expressionNode);

            var namedTableReference = syntaxNode as NamedTableReferenceSyntax;
            if (namedTableReference != null)
                return GetSymbol(position, semanticModel, namedTableReference);

            var derivedTableReference = syntaxNode as DerivedTableReferenceSyntax;
            if (derivedTableReference != null)
                return GetSymbol(position, semanticModel, derivedTableReference);

            return null;
        }

        private static SyntaxResult GetSymbol(int position, SemanticModel semanticModel, PropertyAccessExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            if (symbol != null)
            {
                if (node.Name.Span.Contains(position))
                    return new SyntaxResult(node.Name, symbol);
            }

            return null;
        }

        private static SyntaxResult GetSymbol(SemanticModel semanticModel, ExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            if (symbol != null)
                return new SyntaxResult(node, symbol);

            return null;
        }

        private static SyntaxResult GetSymbol(int position, SemanticModel semanticModel, DerivedTableReferenceSyntax node)
        {
            if (node.Name.Span.Contains(position))
            {
                var symbol = semanticModel.GetDeclaredSymbol(node);
                if (symbol != null)
                    return new SyntaxResult(node.Name, symbol);
            }

            return null;
        }

        private static SyntaxResult GetSymbol(int position, SemanticModel semanticModel, NamedTableReferenceSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
            {
                if (node.TableName.Span.Contains(position))
                    return new SyntaxResult(node.TableName, symbol.Table);

                if (node.Alias != null && node.Alias.Span.Contains(position))
                    return new SyntaxResult(node.Alias, symbol);
            }

            return null;
        }
    }
}