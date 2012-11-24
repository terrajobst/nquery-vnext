using System;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundAllAnySubselect : BoundExpression
    {
        private readonly BoundExpression _left;
        private readonly BoundQuery _boundQuery;
        private readonly OverloadResolutionResult<BinaryOperatorSignature> _result;

        public BoundAllAnySubselect(BoundExpression left, BoundQuery boundQuery, OverloadResolutionResult<BinaryOperatorSignature> result)
        {
            _left = left;
            _boundQuery = boundQuery;
            _result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AllAnySubselect; }
        }

        public BoundExpression Left
        {
            get { return _left; }
        }

        public BoundQuery BoundQuery
        {
            get { return _boundQuery; }
        }

        public OverloadResolutionResult<BinaryOperatorSignature> Result
        {
            get { return _result; }
        }

        public override Type Type
        {
            get
            {
                return _result.Selected == null
                           ? TypeFacts.Unknown
                           : _result.Selected.Signature.ReturnType;
            }
        }
    }
}