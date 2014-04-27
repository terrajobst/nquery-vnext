using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var document = _view.SyntaxEditor.Document.GetNQueryDocument();
            if (document != null)
                document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
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

        private static Task<ImmutableArray<CodeActionModel>> GetActionModelsAsync(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            return Task.Run(() => GetActionModels(semanticModel, position, textDocument));
        }

        private static ImmutableArray<CodeActionModel> GetActionModels(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            var issues = GetCodeIssues(semanticModel, position, textDocument);
            var refactorings = GetRefactorings(semanticModel, position, textDocument);
            return issues.Concat(refactorings).ToImmutableArray();
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

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
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
            var textDocument = _view.SyntaxEditor.Document;
            var semanticModel = await textDocument.GetSemanticModelAsync();
            var textBuffer = semanticModel.Compilation.SyntaxTree.TextBuffer;
            var position = _view.Selection.StartSnapshotOffset.ToOffset(textBuffer);
            var actionModels = await GetActionModelsAsync(semanticModel, position, textDocument);

            if (actionModels.Length == 0)
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