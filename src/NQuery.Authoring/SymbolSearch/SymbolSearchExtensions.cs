namespace NQuery.Authoring.SymbolSearch;

public static class SymbolSearchExtensions
{
    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddSymbolSearchService()
        {
            ThrowIfNull(builder);

            return builder.AddService(_ => new SymbolSearchService());
        }
    }
}
