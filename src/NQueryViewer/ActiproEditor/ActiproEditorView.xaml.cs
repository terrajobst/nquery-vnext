using System;
using System.Windows;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
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
        private readonly NQueryDocument _document;

        public ActiproEditorView()
        {
            InitializeComponent();

            var language = new NQueryLanguage();
            var classificationTypes = language.GetService<INQueryClassificationTypes>();
            classificationTypes.RegisterAll();

            AmbientHighlightingStyleRegistry.Instance.Register(ClassificationTypes.CompilerError, new HighlightingStyle(Brushes.Blue));

            _document = new NQueryDocument();
            _document.ParseDataChanged += DocumentOnParseDataChanged;
            _document.SemanticDataChanged += DocumentOnSemanticDataChanged;

            _syntaxEditor = new SyntaxEditor();
            _syntaxEditor.IsIndicatorMarginVisible = true;
            _syntaxEditor.IsSelectionMarginVisible = false;
            _syntaxEditor.BorderThickness = new Thickness(0);
            _syntaxEditor.Document = _document;
            _syntaxEditor.ViewSelectionChanged += SyntaxEditorOnViewSelectionChanged;
            _syntaxEditor.RegisterCodeActionCommands();
            _syntaxEditor.RegisterSelectionCommands();
            _syntaxEditor.ViewMarginFactories.Add(new NQueryEditorViewMarginFactory());

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
            var parseData = await _document.GetParseDataAsync();
            var snapshot = parseData.SyntaxTree.GetTextSnapshot();
            var textBuffer = parseData.SyntaxTree.TextBuffer;

            var snapshotRange = _syntaxEditor.ActiveView.Selection.SnapshotRange;
            var translatedRange = snapshotRange.TranslateTo(snapshot, TextRangeTrackingModes.Default);
            var span = translatedRange.ToTextSpan(textBuffer);

            CaretPosition = span.Start;
            Selection = span;
        }

        private void DocumentOnParseDataChanged(object sender, ParseDataPropertyChangedEventArgs e)
        {
            SyntaxTree = _document.ParseData.SyntaxTree;
        }

        private void DocumentOnSemanticDataChanged(object sender, EventArgs eventArgs)
        {
            SemanticModel = _document.SemanticData.SemanticModel;
        }

        protected override async void OnCaretPositionChanged()
        {
            var parseData = await _document.GetParseDataAsync();
            var snapshot = parseData.SyntaxTree.GetTextSnapshot();
            var textBuffer = parseData.SyntaxTree.TextBuffer;
            var snapshotOffset = textBuffer.ToSnapshotOffset(snapshot, CaretPosition);
            _syntaxEditor.Caret.Position = snapshotOffset.Position;
            base.OnCaretPositionChanged();
        }

        protected override async void OnSelectionChanged()
        {
            var parseData = await _document.GetParseDataAsync();
            var snapshot = parseData.SyntaxTree.GetTextSnapshot();
            var textBuffer = parseData.SyntaxTree.TextBuffer;
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
