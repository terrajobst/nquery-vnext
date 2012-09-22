using System;
using System.Collections.ObjectModel;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConcatNode : AlgebraNode
    {
        private readonly ReadOnlyCollection<AlgebraNode> _inputs;

        public AlgebraConcatNode(params AlgebraNode[] inputs)
        {
            _inputs = new ReadOnlyCollection<AlgebraNode>(inputs);
        }

        public ReadOnlyCollection<AlgebraNode> Inputs
        {
            get { return _inputs; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Concat; }
        }
    }
}