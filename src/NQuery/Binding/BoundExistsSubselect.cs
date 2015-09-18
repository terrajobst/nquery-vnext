using System;

namespace NQuery.Binding
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        private readonly BoundRelation _relation;

        public BoundExistsSubselect(BoundRelation relation)
        {
            _relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }

        public BoundExistsSubselect Update(BoundRelation relation)
        {
            if (relation == _relation)
                return this;

            return new BoundExistsSubselect(relation);
        }

        public override string ToString()
        {
            return $"EXISTS ({_relation})";
        }
    }
}