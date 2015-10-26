using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class BoundTableSpoolPusher : BoundRelation
    {
        private readonly BoundRelation _input;

        public BoundTableSpoolPusher(BoundRelation input)
        {
            _input = input;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableSpoolPusher; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public BoundTableSpoolPusher Update(BoundRelation input)
        {
            if (input == _input)
                return this;

            return new BoundTableSpoolPusher(input);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues();
        }
    }
}