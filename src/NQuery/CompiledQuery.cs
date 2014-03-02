using System;
using System.Linq;

using NQuery.Binding;
using NQuery.Plan;

namespace NQuery
{
    public sealed class CompiledQuery
    {
        private readonly BoundQuery _query;

        internal CompiledQuery(BoundQuery query)
        {
            _query = query;
        }

        public QueryReader CreateReader()
        {
            return CreateReader(false);
        }

        public QueryReader CreateSchemaReader()
        {
            return CreateReader(true);
        }

        private QueryReader CreateReader(bool schemaOnly)
        {
            var columnNamesAndTypes = _query.OutputColumns.Select(c => Tuple.Create(c.Name, c.Type.ToOutputType())).ToArray();
            var iterator = PlanBuilder.Build(_query.Relation);
            return new QueryReader(iterator, columnNamesAndTypes, schemaOnly);
        }

        public ShowPlanNode GetShowPlan()
        {
            return ShowPlanBuilder.Build(_query);
        }
    }
}