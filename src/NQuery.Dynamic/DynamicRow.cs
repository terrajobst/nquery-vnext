using System.Collections.Frozen;
using System.Dynamic;

namespace NQuery.Dynamic;

internal sealed class DynamicRow : DynamicObject
{
    private readonly FrozenDictionary<string, object> _values;

    public DynamicRow(FrozenDictionary<string, object> values)
    {
        _values = values;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        return _values.TryGetValue(binder.Name, out result);
    }
}
