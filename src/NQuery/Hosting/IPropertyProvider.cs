using NQuery.Symbols;

namespace NQuery.Hosting
{
    public interface IPropertyProvider
    {
        IEnumerable<PropertySymbol> GetProperties(Type type);
    }
}