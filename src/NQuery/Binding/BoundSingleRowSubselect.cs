using System;

namespace NQuery.Binding
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        private readonly ValueSlot _value;
        private readonly BoundRelation _relation;

        public BoundSingleRowSubselect(ValueSlot value, BoundRelation relation)
        {
            _value = value;
            _relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SingleRowSubselect; }
        }

        public override Type Type
        {
            get { return _value.Type; }
        }

        public ValueSlot Value
        {
            get { return _value; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }

        public BoundSingleRowSubselect Update(ValueSlot value, BoundRelation relation)
        {
            if (value == _value && relation == _relation)
                return this;

            return new BoundSingleRowSubselect(value, relation);
        }

        public override string ToString()
        {
            return string.Format("({0})", _relation);
        }
    }
}