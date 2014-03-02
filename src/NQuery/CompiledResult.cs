using System;
using System.Linq;

using NQuery.Binding;
using NQuery.Plan;

namespace NQuery
{
    public sealed class CompiledResult
    {
        private readonly BoundQuery _boundQuery;

        internal CompiledResult(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
        }

        public QueryReader CreateQueryReader(bool schemaOnly)
        {
            var columnNamesAndTypes = _boundQuery.OutputColumns.Select(c => Tuple.Create(c.Name, c.Type.ToOutputType())).ToArray();
            var iterator = PlanBuilder.Build(_boundQuery.Relation);
            return new QueryReader(iterator, columnNamesAndTypes, schemaOnly);
        }

        public ShowPlanNode GetShowPlan()
        {
            return ShowPlanBuilder.Build(_boundQuery);
        }
    }
}