using System;

using NQuery.Text;

namespace NQuery.Authoring.Rearrangement
{
    public interface IArrangementAction
    {
        string Description { get; }
        bool TryMoveBefore(out SyntaxTree syntaxTree, out TextSpan resultSpan);
        bool TryMoveAfter(out SyntaxTree syntaxTree, out TextSpan resultSpan);
    }
}