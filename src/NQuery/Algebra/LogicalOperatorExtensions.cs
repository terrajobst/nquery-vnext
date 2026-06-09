#nullable enable

namespace NQuery.Algebra
{
    internal static class LogicalOperatorExtensions
    {
        public static IEnumerable<LogicalOperator> DescendantsAndSelf(this LogicalOperator node)
        {
            yield return node;
            foreach (var input in node.GetInputs())
            {
                foreach (var descendant in input.DescendantsAndSelf())
                    yield return descendant;
            }
        }

        public static IEnumerable<LogicalOperator> GetInputs(this LogicalOperator node)
        {
            return node switch
            {
                LogicalFilter n => new[] { n.Input },
                LogicalCompute n => new[] { n.Input },
                LogicalProject n => new[] { n.Input },
                LogicalSort n => new[] { n.Input },
                LogicalTop n => new[] { n.Input },
                LogicalAssert n => new[] { n.Input },
                LogicalAggregate n => new[] { n.Input },
                LogicalJoin n => new[] { n.Left, n.Right },
                LogicalApply n => new[] { n.Left, n.Right },
                LogicalIntersectOrExcept n => new[] { n.Left, n.Right },
                LogicalUnion n => n.Inputs.AsEnumerable(),
                _ => Enumerable.Empty<LogicalOperator>()
            };
        }
    }
}
