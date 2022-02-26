using System.Dynamic;

namespace NQuery.Dynamic
{
    internal sealed class DynamicRow : DynamicObject
    {
        private readonly IDictionary<string, object> _values;

        public DynamicRow(IDictionary<string, object> values)
        {
            _values = values;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _values.TryGetValue(binder.Name, out result);
        }
    }
}
