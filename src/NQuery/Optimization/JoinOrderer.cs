using System.Collections.Immutable;
using System.Diagnostics;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class JoinOrderer : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            // Extract relations and predicates

            List<BoundRelation> relations;
            List<BoundExpression> predicates;
            ExtractRelationsAndPredicates(node, out relations, out predicates);

            // Build a graph where the nodes are relations and edges are the predicates
            // connecting them.

            ICollection<JoinNode> nodes;
            ICollection<JoinEdge> edges;
            BuildGraph(relations, predicates, out nodes, out edges);

            // Given the graph, compute a join order that uses the predicates.

            var result = AssembleJoin(nodes, edges);

            return result;
        }

        private void ExtractRelationsAndPredicates(BoundRelation node, out List<BoundRelation> relations, out List<BoundExpression> predicates)
        {
            relations = new List<BoundRelation>();
            predicates = new List<BoundExpression>();

            var stack = new Stack<BoundRelation>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var join = current as BoundJoinRelation;
                if (join is null || join.JoinType != BoundJoinType.Inner)
                {
                    // NOTE: We generally want to rewrite joins that we can't extract
                    //       ourselves. However, we've to be careful not to rewrite the
                    //       node that we started from -- otherwise we stack overflow.
                    var rewrittenCurrent = current == node ? current : RewriteRelation(current);
                    relations.Add(rewrittenCurrent);
                }
                else
                {
                    if (join.Condition is not null)
                    {
                        var conjunctions = Expression.SplitConjunctions(join.Condition);
                        predicates.AddRange(conjunctions);
                    }

                    stack.Push(join.Right);
                    stack.Push(join.Left);
                }
            }
        }

        private static void BuildGraph(List<BoundRelation> relations, List<BoundExpression> predicates, out ICollection<JoinNode> nodes, out ICollection<JoinEdge> edges)
        {
            var nodeByRelation = relations.Select(r => new JoinNode(r)).ToDictionary(n => n.Relation);
            nodes = nodeByRelation.Values;
            edges = new List<JoinEdge>();
            var edgeByNodes = new Dictionary<(JoinNode, JoinNode), JoinEdge>();

            var relationByValueSlot = relations.SelectMany(r => r.GetDefinedValues(), ValueTuple.Create)
                                               .ToDictionary(t => t.Item2, t => t.Item1);

            var dependencyFinder = new ValueSlotDependencyFinder();

            foreach (var predicate in predicates)
            {
                dependencyFinder.ValueSlots.Clear();
                dependencyFinder.VisitExpression(predicate);

                var referencedSlots = dependencyFinder.ValueSlots;
                var referencedRelations = referencedSlots.Where(v => relationByValueSlot.ContainsKey(v))
                                                         .Select(v => relationByValueSlot[v])
                                                         .ToImmutableArray();

                if (referencedRelations.Length == 2)
                {
                    var left = nodeByRelation[referencedRelations[0]];
                    var right = nodeByRelation[referencedRelations[1]];
                    var leftRight = (left, right);
                    var rightLeft = (right, left);

                    JoinEdge edge;
                    if (!edgeByNodes.TryGetValue(leftRight, out edge))
                    {
                        if (!edgeByNodes.TryGetValue(rightLeft, out edge))
                        {
                            edge = new JoinEdge(left, right);

                            left.Edges.Add(edge);
                            right.Edges.Add(edge);

                            edges.Add(edge);
                            edgeByNodes.Add(leftRight, edge);
                        }
                    }

                    edge.Conditions.Add(predicate);
                }
            }
        }

        private static BoundRelation AssembleJoin(ICollection<JoinNode> nodes, ICollection<JoinEdge> edges)
        {
            var nodeComparer = Comparer<JoinNode>.Create(CompareNode);

            var remainingNodes = new HashSet<JoinNode>(nodes);
            var remainingEdges = new HashSet<JoinEdge>(edges);
            var candidateNodes = new HashSet<JoinNode>();

            BoundRelation result = null;

            while (remainingNodes.Count > 0)
            {
                var start = remainingNodes.OrderBy(n => n, nodeComparer).First();
                remainingNodes.Remove(start);

                var relation = start.Relation;

                candidateNodes.UnionWith(start.Edges.Select(e => e.Other(start)));

                while (candidateNodes.Count > 0)
                {
                    var usableEdges = candidateNodes.SelectMany(n => n.Edges)
                                                    .Where(e => remainingEdges.Contains(e))
                                                    .Where(n => !remainingNodes.Contains(n.Left) || !remainingNodes.Contains(n.Right))
                                                    .Where(e => e.Conditions.Any(IsRelation))
                                                    .ToImmutableArray();

                    if (!usableEdges.Any())
                    {
                        usableEdges = candidateNodes.SelectMany(n => n.Edges)
                                                    .Where(e => remainingEdges.Contains(e))
                                                    .Where(n => !remainingNodes.Contains(n.Left) || !remainingNodes.Contains(n.Right))
                                                    .ToImmutableArray();
                    }

                    var nextNode = usableEdges.SelectMany(e => new[] {e.Left, e.Right})
                                              .Where(candidateNodes.Contains)
                                              .OrderBy(n => n, nodeComparer)
                                              .FirstOrDefault();

                    if (nextNode is not null)
                    {
                        candidateNodes.Remove(nextNode);
                        candidateNodes.UnionWith(nextNode.Edges.Select(e => e.Other(nextNode)).Where(n => remainingNodes.Contains(n)));

                        var edge = usableEdges.First(e => !remainingNodes.Contains(e.Left) && e.Right == nextNode ||
                                                          !remainingNodes.Contains(e.Right) && e.Left == nextNode);

                        remainingNodes.Remove(nextNode);
                        remainingEdges.Remove(edge);

                        var left = relation;
                        var right = nextNode.Relation;
                        var condition = Expression.And(edge.Conditions);
                        relation = new BoundJoinRelation(BoundJoinType.Inner, left, right, condition, null, null);
                    }
                }

                result = result is null
                    ? relation
                    : new BoundJoinRelation(BoundJoinType.Inner, result, relation, null, null, null);
            }

            Debug.Assert(remainingNodes.Count == 0, @"Found remaining nodes");

            // Add filter for remaining predicates

            var remainingPredicates = remainingEdges.SelectMany(e => e.Conditions);
            var remainingCondition = Expression.And(remainingPredicates);
            if (remainingCondition is not null)
                result = new BoundFilterRelation(result, remainingCondition);

            return result;
        }

        private static int CompareNode(JoinNode x, JoinNode y)
        {
            var xEstimate = CardinalityEstimator.Estimate(x.Relation).Maximum;
            var yEstimate = CardinalityEstimator.Estimate(y.Relation).Maximum;

            if (xEstimate is null && yEstimate is null)
                return 0;

            if (xEstimate is null)
                return -1;

            if (yEstimate is null)
                return 1;

            return -xEstimate.Value.CompareTo(yEstimate.Value);
        }

        private static bool IsRelation(BoundExpression condition)
        {
            var binary = condition as BoundBinaryExpression;
            if (binary is null || binary.OperatorKind != BinaryOperatorKind.Equal)
                return false;

            var left = binary.Left as BoundValueSlotExpression;
            var right = binary.Right as BoundValueSlotExpression;

            if (left is null || right is null)
                return false;

            // TODO: We should somehow compute from the actual data context
            return left.ValueSlot.Name.Contains(@"ID:") &&
                   right.ValueSlot.Name.Contains(@"ID:");
        }

        private sealed class JoinNode
        {
            public JoinNode(BoundRelation relation)
            {
                Relation = relation;
            }

            public BoundRelation Relation { get; }
            public List<JoinEdge> Edges { get; } = new List<JoinEdge>();

            public override string ToString()
            {
                return Relation.ToString();
            }
        }

        private sealed class JoinEdge
        {
            public JoinEdge(JoinNode left, JoinNode right)
            {
                Left = left;
                Right = right;
            }

            public JoinNode Left { get; }
            public JoinNode Right { get; }
            public List<BoundExpression> Conditions { get; } = new List<BoundExpression>();

            public JoinNode Other(JoinNode node)
            {
                Debug.Assert(node == Left || node == Right);
                return node == Left ? Right : Left;
            }

            public override string ToString()
            {
                return Expression.And(Conditions).ToString();
            }
        }
    }
}