using System;

using NQuery.Authoring.Rearrangement;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    public sealed class RearrangeModel
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly Arrangement _arrangement;

        public RearrangeModel(SyntaxTree syntaxTree, Arrangement arrangement)
        {
            _syntaxTree = syntaxTree;
            _arrangement = arrangement;
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public Arrangement Arrangement
        {
            get { return _arrangement; }
        }
    }
}