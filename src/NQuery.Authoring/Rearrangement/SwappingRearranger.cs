using System;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.Rearrangement
{
    public abstract class SwappingRearranger<T> : Rearranger<T>
        where T : SyntaxNode
    {
        protected abstract string Description { get; }

        protected abstract bool IsHorizontal { get; }

        protected override Arrangement GetArrangement(SyntaxTree syntaxTree, T node, int position)
        {
            TextSpan currentSpan;
            TextSpan? beforeSpan;
            TextSpan? afterSpan;
            if (!TryGetBeforeAfter(syntaxTree, node, position, out currentSpan, out beforeSpan, out afterSpan))
                return null;

            if (beforeSpan == null && afterSpan == null)
                return null;

            var action = new ArrangementAction(syntaxTree, Description, currentSpan, beforeSpan, afterSpan);
            var opration = new ArrangementOperation(currentSpan, action);
            return IsHorizontal
                ? Arrangement.CreateHorizontal(opration)
                : Arrangement.CreateVertical(opration);
        }

        protected abstract bool TryGetBeforeAfter(SyntaxTree syntaxTree, T node, int position, out TextSpan currentSpan, out TextSpan? beforeSpan, out TextSpan? afterSpan);

        private sealed class ArrangementAction : IArrangementAction
        {
            private readonly SyntaxTree _syntaxTree;
            private readonly string _description;
            private readonly TextSpan _currentSpan;
            private readonly TextSpan? _beforeSpan;
            private readonly TextSpan? _afterSpan;

            public ArrangementAction(SyntaxTree syntaxTree, string description, TextSpan currentSpan, TextSpan? beforeSpan, TextSpan? afterSpan)
            {
                _syntaxTree = syntaxTree;
                _description = description;
                _currentSpan = currentSpan;
                _beforeSpan = beforeSpan;
                _afterSpan = afterSpan;
            }

            public string Description
            {
                get { return _description; }
            }

            public bool TryMoveBefore(out SyntaxTree syntaxTree, out TextSpan resultSpan)
            {
                if (_beforeSpan == null)
                {
                    resultSpan = default(TextSpan);
                    syntaxTree = null;
                    return false;
                }

                resultSpan = new TextSpan(_beforeSpan.Value.Start, _currentSpan.Length);
                syntaxTree = Swap(_beforeSpan.Value, _currentSpan);
                return true;
            }

            public bool TryMoveAfter(out SyntaxTree syntaxTree, out TextSpan resultSpan)
            {
                if (_afterSpan == null)
                {
                    resultSpan = default(TextSpan);
                    syntaxTree = null;
                    return false;
                }

                var delta = _afterSpan.Value.Length - _currentSpan.Length;
                resultSpan = new TextSpan(_afterSpan.Value.Start + delta, _currentSpan.Length);
                syntaxTree = Swap(_currentSpan, _afterSpan.Value);
                return true;
            }

            private SyntaxTree Swap(TextSpan span1, TextSpan span2)
            {
                // Ensure span 1 is before span 2.
                if (span1.Start > span2.Start)
                {
                    var temp = span1;
                    span1 = span2;
                    span2 = temp;
                }

                var span1Text = _syntaxTree.Text.GetText(span1);
                var span2Text = _syntaxTree.Text.GetText(span2);

                return _syntaxTree.WithChanges(
                	new TextChange(span1, span2Text),
                	new TextChange(span2, span1Text)
                );
            }
        }
    }
}