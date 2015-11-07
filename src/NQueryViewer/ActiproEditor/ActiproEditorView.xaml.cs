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
using NQuery.Authoring.ActiproWpf.Commenting;
using NQuery.Authoring.ActiproWpf.Margins;
using NQuery.Authoring.ActiproWpf.Selection;
using NQuery.Authoring.ActiproWpf.Text;
using NQuery.Text;

namespace NQueryViewer.ActiproEditor
{
    internal sealed partial class ActiproEditorView : IActiproEditorView
    {
        private readonly SyntaxEditor _syntaxEditor;

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
            _syntaxEditor.RegisterCommentingCommands();
            _syntaxEditor.ViewMarginFactories.Add(new NQueryEditorViewMarginFactory());

            Workspace = _syntaxEditor.Document.GetWorkspace();

            EditorHost.Content = _syntaxEditor;
        }

        private void SyntaxEditorOnViewSelectionChanged(object sender, EditorViewSelectionEventArgs e)
        {
            OnCaretPositionChanged();
            OnSelectionChanged();
        }

        private int GetCaretPosition()
        {
            return _syntaxEditor.Caret.Offset;
        }

        private void SetCaretPosition(int caretPosition)
        {
            _syntaxEditor.Caret.Offset = caretPosition;
        }

        private TextSpan GetSelection()
        {
            return TextSpan.FromBounds(_syntaxEditor.ActiveView.Selection.StartOffset, _syntaxEditor.ActiveView.Selection.EndOffset);
        }

        private void SetSelection(TextSpan selection)
        {
            var sourceText = _syntaxEditor.Document.CurrentSnapshot.ToSourceText();
            var range = sourceText.ToSnapshotRange(selection);
            _syntaxEditor.ActiveView.Selection.SelectRange(range);
        }

        public override void Focus()
        {
            _syntaxEditor.ActiveView.Focus();
        }

        public override DocumentView GetDocumentView()
        {
            return _syntaxEditor.GetDocumentView();
        }

        public override Workspace Workspace { get; }

        public override int CaretPosition
        {
            get { return GetCaretPosition(); }
            set {  SetCaretPosition(value); }
        }

        public override TextSpan Selection
        {
            get { return GetSelection(); }
            set { SetSelection(value); }
        }
    }
}
