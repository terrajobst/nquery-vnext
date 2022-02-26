namespace NQuery.Binding
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        public BoundExistsSubselect(BoundRelation relation)
        {
            Relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundRelation Relation { get; }

        public override Type Type
        {
            get { return typeof(bool); }
        }

        public BoundExistsSubselect Update(BoundRelation relation)
        {
            if (relation == Relation)
                return this;

            return new BoundExistsSubselect(relation);
        }

        public override string ToString()
        {
            return $"EXISTS ({Relation})";
        }
    }
}