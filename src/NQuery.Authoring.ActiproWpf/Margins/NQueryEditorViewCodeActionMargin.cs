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

using NQuery.Authoring.ActiproWpf.CodeActions;
using NQuery.Authoring.ActiproWpf.Text;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.ActiproWpf.Margins
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
            var workspace = _view.SyntaxEditor.Document.GetWorkspace();
            if (workspace != null)
                workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _view.SyntaxEditor.Document.Language.RegisterService(typeof(ICodeActionGlyphController), this);

            Width = 19;
            Children.Add(_glyphPopup);
        }

        private static Task<ImmutableArray<CodeActionModel>> GetActionModelsAsync(SemanticModel semanticModel, int position)
        {
            return Task.Run(() => GetActionModels(semanticModel, position));
        }

        private static ImmutableArray<CodeActionModel> GetActionModels(SemanticModel semanticModel, int position)
        {
            var textDocument = semanticModel.SyntaxTree.Text.Container.ToTextDocument();

            var fixes = GetCodeFixes(semanticModel, position, textDocument);
            var issues = GetCodeIssues(semanticModel, position, textDocument);
            var refactorings = GetRefactorings(semanticModel, position, textDocument);
            return fixes.Concat(issues).Concat(refactorings).ToImmutableArray();
        }

        private static IEnumerable<CodeActionModel> GetCodeFixes(SemanticModel semanticModel, int position, ITextDocument textDocument)
        {
            return semanticModel.GetFixes(position)
                                .Select(a => new TextDocumentCodeActionModel(CodeActionKind.IssueFix, a, textDocument));
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

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
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
            var snapshot = _view.SyntaxEditor.GetDocumentView();
            var document = snapshot.Document;
            var position = snapshot.Position;
            var semanticModel = await document.GetSemanticModelAsync();
            var actionModels = await GetActionModelsAsync(semanticModel, position);

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