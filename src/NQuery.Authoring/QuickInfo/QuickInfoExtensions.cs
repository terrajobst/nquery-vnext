using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Authoring.QuickInfo.Providers;

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
                   };
        }

        public static QuickInfoModel GetQuickInfoModel(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardQuickInfoModelProviders();
            return semanticModel.GetQuickInfoModel(position, providers);
        }

        public static QuickInfoModel GetQuickInfoModel(this SemanticModel semanticModel, int position, IEnumerable<IQuickInfoModelProvider> providers)
        {
            return (from p in providers
                    let m = p.GetModel(semanticModel, position)
                    where m != null
                    select m).FirstOrDefault();
        }
    }
}