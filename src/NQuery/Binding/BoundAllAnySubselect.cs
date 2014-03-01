using System;

namespace NQuery.Binding
{
    internal sealed class BoundAllAnySubselect : BoundExpression
    {
        private readonly BoundExpression _left;
        private readonly BoundRelation _relation;
        private readonly OverloadResolutionResult<BinaryOperatorSignature> _result;

        public BoundAllAnySubselect(BoundExpression left, BoundRelation relation, OverloadResolutionResult<BinaryOperatorSignature> result)
        {
            _left = left;
            _relation = relation;
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

        public BoundRelation Relation
        {
            get { return _relation; }
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