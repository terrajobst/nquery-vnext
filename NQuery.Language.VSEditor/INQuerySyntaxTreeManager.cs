using System;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySyntaxTreeManager
    {
        SyntaxTree SyntaxTree { get; }

        event EventHandler<EventArgs> SyntaxTreeChanged;
    }
}