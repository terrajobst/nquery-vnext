using System;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NQuery.Language.Symbols;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    internal sealed class QuickInfoManager : IQuickInfoManager
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly IQuickInfoBroker _quickInfoBroker;

        private QuickInfoModel _model;
        private IQuickInfoSession _session;

        public QuickInfoManager(ITextView textView, INQueryDocument document, IQuickInfoBroker quickInfoBroker)
        {
            _textView = textView;
            _document = document;
            _quickInfoBroker = quickInfoBroker;
        }

        public async void TriggerQuickInfo(int offset)
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var model = FindNodeWithSymbol(syntaxTree.Root, offset, semanticModel);

            Model = model;
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, e);
        }

        public QuickInfoModel Model
        {
            get { return _model; }
            private set
            {
                if (_model != value)
                {
                    _model = value;
                    OnModelChanged(EventArgs.Empty);

                    var hasData = _model != null && _model.Symbol != null;
                    var showSession = _session == null && hasData;
                    var hideSession = _session != null && !hasData;

                    if (hideSession)
                    {
                        _session.Dismiss();
                    }
                    else if (showSession)
                    {
                        var syntaxTree = _model.NodeOrToken.SyntaxTree;
                        var snapshot = _document.GetTextSnapshot(syntaxTree);
                        var triggerPosition = _model.NodeOrToken.Span.Start;
                        var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                        _session = _quickInfoBroker.CreateQuickInfoSession(_textView, triggerPoint, true);
                        _session.Properties.AddProperty(typeof(IQuickInfoManager), this);
                        _session.Dismissed += SessionOnDismissed;
                        _session.Start();
                    }
                }
            }
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        public event EventHandler<EventArgs> ModelChanged;

        private static QuickInfoModel FindNodeWithSymbol(SyntaxNode root, int position, SemanticModel semanticModel)
        {
            if (root == null || !root.Span.Contains(position))
                return null;

            var nodes = root.ChildNodesAndTokens()
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

        private static QuickInfoModel GetSymbol(int position, SemanticModel semanticModel, SyntaxNode syntaxNode)
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

        private static QuickInfoModel GetSymbol(int position, SemanticModel semanticModel, PropertyAccessExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            if (symbol != null)
            {
                if (node.Name.Span.Contains(position))
                    return new QuickInfoModel(node.Name, symbol);
            }

            return null;
        }

        private static QuickInfoModel GetSymbol(SemanticModel semanticModel, ExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            if (symbol != null)
                return new QuickInfoModel(node, symbol);

            return null;
        }

        private static QuickInfoModel GetSymbol(int position, SemanticModel semanticModel, DerivedTableReferenceSyntax node)
        {
            if (node.Name.Span.Contains(position))
            {
                var symbol = semanticModel.GetDeclaredSymbol(node);
                if (symbol != null)
                    return new QuickInfoModel(node.Name, symbol);
            }

            return null;
        }

        private static QuickInfoModel GetSymbol(int position, SemanticModel semanticModel, NamedTableReferenceSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
            {
                if (node.TableName.Span.Contains(position))
                    return new QuickInfoModel(node.TableName, symbol.Table);

                if (node.Alias != null && node.Alias.Span.Contains(position))
                    return new QuickInfoModel(node.Alias, symbol);
            }

            return null;
        }
    }
}