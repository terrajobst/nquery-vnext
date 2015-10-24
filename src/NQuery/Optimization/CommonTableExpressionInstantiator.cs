using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal sealed class CommonTableExpressionInstantiator : BoundTreeRewriter
    {
        private int _cteCount;

        protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            var symbol = node.TableInstance.Table as CommonTableExpressionSymbol;
            if (symbol == null)
                return base.RewriteTableRelation(node);

            var prototype = symbol.Query.Relation;
            var instantiatedQuery = InstantiateCommonTableExpression(node.GetOutputValues(), prototype);

            // For regular SELECT queries the output node will be a projection.
            // We don't need this moving forward so it's safe to omit.
            //
            // NOTE: We need to keep this after we added the output slot mapping.
            //       Otherwise the slots will not align.

            var projection = instantiatedQuery as BoundProjectRelation;
            var result = projection == null ? instantiatedQuery : projection.Input;

            return result;
        }

        private BoundRelation InstantiateCommonTableExpression(IEnumerable<ValueSlot> outputValues, BoundRelation relation)
        {
            var mapping = outputValues.Zip(relation.GetOutputValues(), (v, k) => new KeyValuePair<ValueSlot, ValueSlot>(k, v));

            _cteCount++;
            return Instatiator.Instantiate(relation, CreateNewName, mapping);
        }

        private string CreateNewName(string name)
        {
            return $"{name}:CTE:{_cteCount}";
        }
    }
}