using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Margins;

using NQuery.Authoring.ActiproWpf.Margins;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    internal sealed class NQueryEditorViewCodeActionMargin : Canvas, IEditorViewMargin, ICodeActionGlyphController
    {
        private readonly IEditorView _view;
        private readonly CodeActionGlyphPopup _glyphPopup = new CodeActionGlyphPopup();

        public NQueryEditorViewCodeActionMargin(IEditorView view)
        {
            _view = view;
            _view.SelectionChanged += ViewOnSelectionChanged;
            _view.TextAreaLayout += ViewOnTextAreaLayout;
            var document = _view.SyntaxEditor.Document as NQueryDocument;
            if (document != null)
                document.SemanticDataChanged += DocumentOnSemanticDataChanged;
            _view.SyntaxEditor.Document.Language.RegisterService(typeof(ICodeActionGlyphController), this);

            Width = 19;
            Children.Add(_glyphPopup);

            _glyphPopup.UpdateModels(new List<CodeActionModel>
                                     {
                                         new TextDocumentCodeActionModel(CodeActionKind.IssueFix, new FakeAction(), view.SyntaxEditor.Document)
                                     });
        }

        private sealed class FakeAction : ICodeAction
        {
            public string Description
            {
                get { return "Convert to empty"; }
            }

            public SyntaxTree GetEdit()
            {
                return SyntaxTree.Empty;
            }
        }

        private static Task<IReadOnlyCollection<CodeActionModel>> GetActionModelsAsync(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            return Task.Run(() => GetActionModels(semanticModel, position, textDocument));
        }

        private static IReadOnlyCollection<CodeActionModel> GetActionModels(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            var issues = GetCodeIssues(semanticModel, position, textDocument);
            var refactorings = GetRefactorings(semanticModel, position, textDocument);
            return issues.Concat(refactorings).ToArray();
        }

        private static IEnumerable<CodeActionModel> GetCodeIssues(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            return semanticModel.GetIssues()
                                .Where(i => i.Span.ContainsOrTouches(position))
                                .SelectMany(i => i.Actions)
                                .Select(a => new TextDocumentCodeActionModel(CodeActionKind.IssueFix, a, textDocument));
        }

        private static IEnumerable<CodeActionModel> GetRefactorings(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            return semanticModel.GetRefactorings(position)
                                .Select(a => new TextDocumentCodeActionModel(CodeActionKind.Refactoring, a, textDocument));
        }

        private void DocumentOnSemanticDataChanged(object sender, EventArgs e)
        {
            UpdateGlyph();
        }

        private void ViewOnSelectionChanged(object sender, EditorViewSelectionEventArgs e)
        {
            UpdateGlyph();
        }

        private void ViewOnTextAreaLayout(object sender, TextViewTextAreaLayoutEventArgs e)
        {
            UpdateGlyph();
        }

        private async void UpdateGlyph()
        {
            var document = _view.SyntaxEditor.Document;
            var semanticData = await document.GetSemanticDataAsync();
            var semanticModel = semanticData.SemanticModel;
            var textBuffer = semanticModel.Compilation.SyntaxTree.TextBuffer;
            var position = _view.Selection.StartSnapshotOffset.ToOffset(textBuffer);
            var actionModels = await GetActionModelsAsync(semanticModel, position, document);

            if (actionModels.Count == 0)
            {
                _glyphPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                var top = _view.CurrentViewLine.Bounds.Top;
                SetTop(_glyphPopup, top);
                _glyphPopup.UpdateModels(actionModels);
                _glyphPopup.Visibility = Visibility.Visible;
            }
        }

        public void Expand()
        {
            if (IsActive)
                _glyphPopup.Expand();
        }

        public void Collapse()
        {
            if (IsActive)
                _glyphPopup.Collapse();
        }

        public string Key
        {
            get { return NQueryEditorViewMarginKeys.CodeActions; }
        }

        public IEnumerable<Ordering> Orderings
        {
            get { return new[] { new Ordering(EditorViewMarginKeys.Indicator, OrderPlacement.Before) }; }
        }

        public FrameworkElement VisualElement
        {
            get { return this; }
        }

        public EditorViewMarginPlacement Placement
        {
            get { return EditorViewMarginPlacement.ScrollableLeft; }
        }

        public bool IsActive
        {
            get { return _glyphPopup.Visibility == Visibility.Visible; }
        }

        public bool IsExpanded
        {
            get { return _glyphPopup.IsExpanded; }
        }
    }
}