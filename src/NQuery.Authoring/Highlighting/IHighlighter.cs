using System;
using System.Collections.Generic;

namespace NQuery.Authoring.Highlighting
{
    public interface IHighlighter
    {
        IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position);
    }
}