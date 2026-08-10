using System.Collections.Immutable;

using NQuery.Authoring.QuickInfo.Providers;

namespace NQuery.Authoring.QuickInfo;

public static class QuickInfoExtensions
{
    private static ImmutableArray<IQuickInfoModelProvider> StandardQuickInfoModelProviders { get; } =
    [
        new CastExpressionQuickInfoModelProvider(),
        new CoalesceExpressionQuickInfoModelProvider(),
        new CommonTableExpressionColumnNameQuickInfoModelProvider(),
        new CommonTableExpressionQuickInfoModelProvider(),
        new CountAllExpressionQuickInfoModelProvider(),
        new DerivedTableReferenceQuickInfoModelProvider(),
        new ExpressionSelectColumnQuickInfoModelProvider(),
        new FunctionInvocationExpressionQuickInfoModelProvider(),
        new MethodInvocationExpressionQuickInfoModelProvider(),
        new NamedTableReferenceQuickInfoModelProvider(),
        new NameExpressionQuickInfoModelProvider(),
        new NullIfQuickInfoModelProvider(),
        new PropertyAccessExpressionQuickInfoModelProvider(),
        new VariableExpressionQuickInfoModelProvider(),
        new WildcardSelectColumnQuickInfoModelProvider()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddQuickInfoService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new QuickInfoService(s.GetProviders<IQuickInfoModelProvider>()));
        }

        public AuthoringServicesBuilder AddQuickInfoModelProvider(IQuickInfoModelProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<IQuickInfoModelProvider>(provider);
        }

        public AuthoringServicesBuilder AddStandardQuickInfoModelProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardQuickInfoModelProviders);
        }
    }
}
