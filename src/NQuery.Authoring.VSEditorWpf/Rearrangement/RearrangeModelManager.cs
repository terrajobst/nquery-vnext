using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Rearrangement;
using NQuery.Authoring.VSEditorWpf.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    internal sealed class RearrangeModelManager : IRearrangeModelManager
    {
        private readonly Workspace _workspace;
        private readonly ITextView _textView;
        private readonly ISyntaxTreeApplier _syntaxTreeApplier;
        private readonly ImmutableArray<IRearranger> _rearrangers;

        private bool _isVisible;
        private RearrangeModel _model;

        public RearrangeModelManager(Workspace workspace, ITextView textView, ISyntaxTreeApplier syntaxTreeApplier, ImmutableArray<IRearranger> rearrangers)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textView = textView;
            _syntaxTreeApplier = syntaxTreeApplier;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _rearrangers = rearrangers;
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateModel();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateModel();
        }

        private async void UpdateModel()
        {
            var documentView = _textView.GetDocumentView();
            var syntaxTree = await documentView.Document.GetSyntaxTreeAsync();
            var position = documentView.Position;
            var model = await ComputeModelAsync(syntaxTree, position, _rearrangers);
            var newDocumentView = _textView.GetDocumentView();

            if (documentView.Document == newDocumentView.Document &&
                documentView.Position == newDocumentView.Position)
            {
                Model = model;
            }
        }

        private static Task<RearrangeModel> ComputeModelAsync(SyntaxTree syntaxTree, int position, ImmutableArray<IRearranger> rearrangers)
        {
            return Task.Run(() =>
            {
                var arrangement = syntaxTree.GetRearrangement(position, rearrangers);
                if (arrangement == null)
                    return null;

                return new RearrangeModel(syntaxTree, arrangement);
            });
        }

        public void MoveUp()
        {
            if (Model != null && Model.Arrangement.VerticalOperation != null)
                ExecuteMoveBefore(Model.Arrangement.VerticalOperation, "Move up");
        }

        public void MoveDown()
        {
            if (Model != null && Model.Arrangement.VerticalOperation != null)
                ExecuteMoveAfter(Model.Arrangement.VerticalOperation, "Move down");
        }

        public void MoveLeft()
        {
            if (Model != null && Model.Arrangement.HorizontalOperation != null)
                ExecuteMoveBefore(Model.Arrangement.HorizontalOperation, "Move left");
        }

        public void MoveRight()
        {
            if (Model != null && Model.Arrangement.HorizontalOperation != null)
                ExecuteMoveAfter(Model.Arrangement.HorizontalOperation, "Move right");
        }

        private void ExecuteMoveBefore(ArrangementOperation operation, string description)
        {
            if (operation.Action != null)
            {
                SyntaxTree syntaxTree;
                TextSpan highlightSpan;
                if (operation.Action.TryMoveBefore(out syntaxTree, out highlightSpan))
                    Apply(syntaxTree, highlightSpan, description);
            }
        }

        private void ExecuteMoveAfter(ArrangementOperation operation, string description)
        {
            if (operation.Action != null)
            {
                SyntaxTree syntaxTree;
                TextSpan highlightSpan;
                if (operation.Action.TryMoveAfter(out syntaxTree, out highlightSpan))
                    Apply(syntaxTree, highlightSpan, description);
            }
        }

        private void Apply(SyntaxTree moveBefore, TextSpan highlightSpan, string description)
        {
            _syntaxTreeApplier.Apply(moveBefore, description);
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var start = highlightSpan.Start;
            var length = highlightSpan.Length;
            var snapshotSpan = new SnapshotSpan(snapshot, start, length);
            _textView.Caret.MoveTo(snapshotSpan.Start);
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnIsVisibleChanged();
                }
            }
        }

        public RearrangeModel Model
        {
            get { return _model; }
            private set
            {
                if (_model != value)
                {
                    _model = value;
                    if (_model == null)
                        IsVisible = false;

                    OnModelChanged();
                }
            }
        }

        private void OnIsVisibleChanged()
        {
            var handler = IsVisibleChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnModelChanged()
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> IsVisibleChanged;

        public event EventHandler<EventArgs> ModelChanged;
    }
}