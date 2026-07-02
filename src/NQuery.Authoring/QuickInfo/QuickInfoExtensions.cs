using System.Collections.Immutable;

using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.QuickInfo;

public static class QuickInfoExtensions
{
    public static ImmutableArray<IQuickInfoModelProvider> StandardQuickInfoModelProviders { get; } =
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

    extension(SemanticModel semanticModel)
    {
        public QuickInfoModel? GetQuickInfoModel(int position)
        {
            return semanticModel.GetQuickInfoModel(position, StandardQuickInfoModelProviders);
        }

        public QuickInfoModel? GetQuickInfoModel(int position, IEnumerable<IQuickInfoModelProvider> providers)
        {
            return (from p in providers
                    let m = p.GetModel(semanticModel, position)
                    where m is not null
                    select m).FirstOrDefault();
        }
    }
}
