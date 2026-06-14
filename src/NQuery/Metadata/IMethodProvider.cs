namespace NQuery.Metadata;

public interface IMethodProvider
{
    IEnumerable<MethodDefinition> GetMethods(Type type);
}
