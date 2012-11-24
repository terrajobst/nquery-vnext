using System;
using System.Collections.ObjectModel;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConcatNode : AlgebraRelation
    {
        private readonly ReadOnlyCollection<AlgebraRelation> _inputs;

        public AlgebraConcatNode(params AlgebraRelation[] inputs)
        {
            _inputs = new ReadOnlyCollection<AlgebraRelation>(inputs);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Concat; }
        }

        public ReadOnlyCollection<AlgebraRelation> Inputs
        {
            get { return _inputs; }
        }
    }
}