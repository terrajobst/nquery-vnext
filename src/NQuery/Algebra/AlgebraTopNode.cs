using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraTopNode : AlgebraNode
    {
        private readonly AlgebraNode _input;
        private readonly int _top;
        private readonly bool _withTies;

        public AlgebraTopNode(AlgebraNode input, int top, bool withTies)
        {
            _input = input;
            _top = top;
            _withTies = withTies;
        }

        public AlgebraNode Input
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

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Top; }
        }
    }
}