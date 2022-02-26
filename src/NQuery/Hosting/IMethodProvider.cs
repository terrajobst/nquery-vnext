using NQuery.Symbols;

namespace NQuery.Hosting
{
    public interface IMethodProvider
    {
        IEnumerable<MethodSymbol> GetMethods(Type type);
    }
}