namespace NQuery.Metadata;

public interface IPropertyProvider
{
    IEnumerable<PropertyDefinition> GetProperties(Type type);
}
