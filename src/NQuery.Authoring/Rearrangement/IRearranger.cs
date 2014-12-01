using System;

namespace NQuery.Authoring.Rearrangement
{
    public interface IRearranger
    {
        Arrangement GetArrangement(SyntaxTree syntaxTree, int position);
    }
}