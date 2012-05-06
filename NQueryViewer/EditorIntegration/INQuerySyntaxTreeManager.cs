using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Language;

namespace NQueryViewer.EditorIntegration
{
    public interface INQuerySyntaxTreeManager
    {
        SyntaxTree SyntaxTree { get; }

        event EventHandler<EventArgs> SyntaxTreeChanged;
    }
}