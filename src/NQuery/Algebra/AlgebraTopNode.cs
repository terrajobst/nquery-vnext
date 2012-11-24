using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraTopNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly int _top;
        private readonly bool _withTies;

        public AlgebraTopNode(AlgebraRelation input, int top, bool withTies)
        {
            _input = input;
            _top = top;
            _withTies = withTies;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Top; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public int Top
        {
            get { return _top; }
        }

        public bool WithTies
        {
            get { return _withTies; }
        }
    }
}