using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Authoring.QuickInfo
{
    public static class QuickInfoExtensions
    {
        public static IEnumerable<IQuickInfoModelProvider> GetStandardQuickInfoModelProviders()
        {
            return new IQuickInfoModelProvider[]
                   {
                       new CastExpressionQuickInfoModelProvider(),
                       new CoalesceExpressionQuickInfoModelProvider(),
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
                   };
        }

        public static QuickInfoModel GetQuickInfoModel(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardQuickInfoModelProviders();
            return semanticModel.GetQuickInfoModel(position, providers);
        }

        public static QuickInfoModel GetQuickInfoModel(this SemanticModel semanticModel, int position, IEnumerable<IQuickInfoModelProvider> providers)
        {
            var model = providers
                .Select(p => p.GetModel(semanticModel, position))
                .FirstOrDefault(m => m != null);
            return model;
        }
    }
}