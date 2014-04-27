using System;
using System.Windows;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;

using NQuery.Authoring.ActiproWpf;
using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Authoring.ActiproWpf.CodeActions;
using NQuery.Authoring.ActiproWpf.Margins;
using NQuery.Authoring.ActiproWpf.Selection;

namespace NQueryViewer.ActiproEditor
{
    internal sealed partial class ActiproEditorView : IActiproEditorView
    {
        private readonly SyntaxEditor _syntaxEditor;
        private readonly NQuery.Authoring.Document.NQueryDocument _document;

        public ActiproEditorView()
        {
            InitializeComponent();

            var language = new NQueryLanguage();
            var classificationTypes = language.GetService<INQueryClassificationTypes>();
            classificationTypes.RegisterAll();

            AmbientHighlightingStyleRegistry.Instance.Register(ClassificationTypes.CompilerError, new HighlightingStyle(Brushes.Blue));

            _syntaxEditor = new SyntaxEditor();
            _syntaxEditor.IsIndicatorMarginVisible = true;
            _syntaxEditor.IsSelectionMarginVisible = false;
            _syntaxEditor.BorderThickness = new Thickness(0);
            _syntaxEditor.Document.Language = language;
            _syntaxEditor.ViewSelectionChanged += SyntaxEditorOnViewSelectionChanged;
            _syntaxEditor.RegisterCodeActionCommands();
            _syntaxEditor.RegisterSelectionCommands();
            _syntaxEditor.ViewMarginFactories.Add(new NQueryEditorViewMarginFactory());

            _document = _syntaxEditor.Document.GetNQueryDocument();
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;

            EditorHost.Content = _syntaxEditor;
            DocumentType = _document.DocumentType;
            DataContext = _document.DataContext;
            UpdateCaretAndSelection();
        }

        private void SyntaxEditorOnViewSelectionChanged(object sender, EditorViewSelectionEventArgs editorViewSelectionEventArgs)
        {
            UpdateCaretAndSelection();
        }

        private async void UpdateCaretAndSelection()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;

            var snapshotRange = _syntaxEditor.ActiveView.Selection.SnapshotRange;
            var translatedRange = snapshotRange.TranslateTo(snapshot, TextRangeTrackingModes.Default);
            var span = translatedRange.ToTextSpan(textBuffer);

            CaretPosition = span.Start;
            Selection = span;
        }

        private async void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            SyntaxTree = await _document.GetSyntaxTreeAsync();
        }

        private async void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            SemanticModel = await _document.GetSemanticModelAsync();
        }

        protected override async void OnCaretPositionChanged()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;
            var snapshotOffset = textBuffer.ToSnapshotOffset(snapshot, CaretPosition);
            _syntaxEditor.Caret.Position = snapshotOffset.Position;
            base.OnCaretPositionChanged();
        }

        protected override async void OnSelectionChanged()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;
            var snapshotRange = textBuffer.ToSnapshotRange(snapshot, Selection);
            _syntaxEditor.ActiveView.Selection.SelectRange(snapshotRange);
            base.OnSelectionChanged();
        }

        protected override void OnDocumentTypeChanged()
        {
            _document.DocumentType = DocumentType;
            base.OnDocumentTypeChanged();
        }

        protected override void OnDataContextChanged()
        {
            _document.DataContext = DataContext;
            base.OnDataContextChanged();
        }

        public override void Focus()
        {
            _syntaxEditor.ActiveView.Focus();
        }
    }
}
