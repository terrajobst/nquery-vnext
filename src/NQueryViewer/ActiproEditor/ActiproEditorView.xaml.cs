using System;
using System.Windows;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;

using NQuery.Authoring;
using NQuery.Authoring.ActiproWpf;
using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Authoring.ActiproWpf.CodeActions;
using NQuery.Authoring.ActiproWpf.Margins;
using NQuery.Authoring.ActiproWpf.Selection;
using NQuery.Authoring.ActiproWpf.Text;

namespace NQueryViewer.ActiproEditor
{
    internal sealed partial class ActiproEditorView : IActiproEditorView
    {
        private readonly SyntaxEditor _syntaxEditor;
        private readonly Workspace _workspace;

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
            _syntaxEditor.Document.AutoConvertTabsToSpaces = true;
            _syntaxEditor.ViewSelectionChanged += SyntaxEditorOnViewSelectionChanged;
            _syntaxEditor.RegisterCodeActionCommands();
            _syntaxEditor.RegisterSelectionCommands();
            _syntaxEditor.ViewMarginFactories.Add(new NQueryEditorViewMarginFactory());

            _workspace = _syntaxEditor.Document.GetWorkspace();

            EditorHost.Content = _syntaxEditor;
            UpdateCaretAndSelection();
        }

        private void SyntaxEditorOnViewSelectionChanged(object sender, EditorViewSelectionEventArgs e)
        {
            UpdateCaretAndSelection();
        }

        private void UpdateCaretAndSelection()
        {
            var document = _workspace.CurrentDocument;
            var snapshot = document.Text.ToTextSnapshot();

            var snapshotRange = _syntaxEditor.ActiveView.Selection.SnapshotRange;
            var translatedRange = snapshotRange.TranslateTo(snapshot, TextRangeTrackingModes.Default);
            var span = translatedRange.ToTextSpan();

            CaretPosition = span.Start;
            Selection = span;
        }

        public override Workspace Workspace
        {
            get { return _workspace; }
        }

        protected override async void OnCaretPositionChanged()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var textBuffer = syntaxTree.Text;
            var snapshotOffset = textBuffer.ToSnapshotOffset(CaretPosition);
            _syntaxEditor.Caret.Position = snapshotOffset.Position;
            base.OnCaretPositionChanged();
        }

        protected override async void OnSelectionChanged()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var textBuffer = syntaxTree.Text;
            var snapshotRange = textBuffer.ToSnapshotRange(Selection);
            _syntaxEditor.ActiveView.Selection.SelectRange(snapshotRange);
            base.OnSelectionChanged();
        }

        public override void Focus()
        {
            _syntaxEditor.ActiveView.Focus();
        }
    }
}
