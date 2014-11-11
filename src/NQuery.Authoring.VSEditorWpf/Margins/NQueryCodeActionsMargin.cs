using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.VSEditorWpf.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Margins
{
    internal sealed class NQueryCodeActionsMargin : Canvas, IWpfTextViewMargin, ICodeActionGlyphController
    {
        private readonly Workspace _workspace;
        private readonly IWpfTextViewHost _textViewHost;
        private readonly ImmutableArray<ICodeIssueProvider> _issueProviders;
        private readonly ImmutableArray<ICodeRefactoringProvider> _refactoringProviders;

        private readonly CodeActionGlyphPopup _glyphPopup = new CodeActionGlyphPopup();

        public NQueryCodeActionsMargin(Workspace workspace, IWpfTextViewHost textViewHost, ImmutableArray<ICodeIssueProvider> issueProviders, ImmutableArray<ICodeRefactoringProvider> refactoringProviders)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textViewHost = textViewHost;
            _textViewHost.TextView.Caret.PositionChanged += CaretOnPositionChanged;
            _textViewHost.TextView.LayoutChanged += TextViewOnLayoutChanged;
            _textViewHost.TextView.ZoomLevelChanged += TextViewOnZoomLevelChanged;
            _issueProviders = issueProviders;
            _refactoringProviders = refactoringProviders;

            Width = 16;
            SetLeft(_glyphPopup, 0);
            Children.Add(_glyphPopup);
        }

        private Task<ImmutableArray<CodeActionModel>> GetActionModelsAsync(SemanticModel semanticModel, int position, ITextBuffer textBuffer)
        {
            return Task.Run(() => GetActionModels(semanticModel, position, textBuffer));
        }

        private ImmutableArray<CodeActionModel> GetActionModels(SemanticModel semanticModel, int position, ITextBuffer textBuffer)
        {
            var issues = GetCodeIssues(semanticModel, position, textBuffer);
            var refactorings = GetRefactorings(semanticModel, position, textBuffer);
            return issues.Concat(refactorings).ToImmutableArray();
        }

        private IEnumerable<CodeActionModel> GetCodeIssues(SemanticModel semanticModel, int position, ITextBuffer textBuffer)
        {
            return semanticModel.GetIssues(_issueProviders)
                                .Where(i => i.Span.ContainsOrTouches(position))
                                .SelectMany(i => i.Actions)
                                .Select(a => new TextBufferCodeActionModel(CodeActionKind.IssueFix, a, textBuffer));
        }

        private IEnumerable<CodeActionModel> GetRefactorings(SemanticModel semanticModel, int position, ITextBuffer textBuffer)
        {
            return semanticModel.GetRefactorings(position, _refactoringProviders)
                                .Select(a => new TextBufferCodeActionModel(CodeActionKind.Refactoring, a, textBuffer));
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateGlyph();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateGlyph();
        }

        private void TextViewOnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            UpdateGlyph();
        }

        private void TextViewOnZoomLevelChanged(object sender, ZoomLevelChangedEventArgs e)
        {
            LayoutTransform = e.ZoomTransform;
        }

        private async void UpdateGlyph()
        {
            var textView = _textViewHost.TextView;
            var documentView = textView.GetDocumentView();
            var position = documentView.Position;
            var document = documentView.Document;
            var snapshot = document.GetTextSnapshot();
            var semanticModel = await document.GetSemanticModelAsync();
            var currentSnapshot = textView.TextBuffer.CurrentSnapshot;
            if (snapshot != currentSnapshot)
                return;

            var bufferPosition = new SnapshotPoint(snapshot, position);
            var textViewLine = textView.GetTextViewLineContainingBufferPosition(bufferPosition);
            var actionModels = await GetActionModelsAsync(semanticModel, position, snapshot.TextBuffer);

            if (actionModels.Length == 0)
            {
                _glyphPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                var viewportTop = textView.ViewportTop;
                var top = textViewLine.Top - viewportTop;
                SetTop(_glyphPopup, top);
                _glyphPopup.UpdateModels(actionModels);
                _glyphPopup.Visibility = Visibility.Visible;
            }
        }

        public void Dispose()
        {
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return null;
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

        public bool IsActive
        {
            get { return _glyphPopup.Visibility == Visibility.Visible; }
        }

        public bool IsExpanded
        {
            get { return _glyphPopup.IsExpanded; }
        }

        public double MarginSize
        {
            get { return ActualWidth; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public FrameworkElement VisualElement
        {
            get { return this; }
        }
    }
}