using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraProjectNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _output;

        public AlgebraProjectNode(AlgebraRelation input, IList<ValueSlot> output)
        {
            _input = input;
            _output = new ReadOnlyCollection<ValueSlot>(output);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Project; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<ValueSlot> Output
        {
            get { return _output; }
        }
    }
}