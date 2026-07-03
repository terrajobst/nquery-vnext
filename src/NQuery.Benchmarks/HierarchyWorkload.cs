namespace NQuery.Benchmarks;

// Recursive-CTE workload for the "deep vs. wide hierarchy" benchmark. Nothing here
// references NQuery types, so it lives on neither side of the extern-alias boundary.
//
// Both shapes emit the SAME total node count (Forests * NodesPerForest) and the same
// recursive-union output, but with opposite frontier profiles:
//
//   Deep  - each forest is a chain of NodesPerForest nodes. The frontier is one node
//           per forest and the recursion runs NodesPerForest-1 levels deep.
//
//   Wide  - each forest is a star: one root with NodesPerForest-1 direct children. The
//           frontier is (NodesPerForest-1) per forest and the recursion terminates
//           after a single level.
//
// With the recursive step's base hash built once and reused across rounds, the two shapes
// land close together and scale ~linearly in node count -- deep no longer pays a per-round
// base rebuild.
//
// NodesPerForest is capped below MAXRECURSION (fixed at 100) so the deep chain never
// trips the recursion limit.
public static class HierarchyWorkload
{
    // 99 nodes => a deep chain recurses 98 levels, safely under the fixed MAXRECURSION of 100.
    private const int NodesPerForest = 99;

    // Roots use a -1 parent sentinel rather than a nullable column: null semantics are beside
    // the point here, and a plain INT keeps the join key types identical on both sides.
    public const int RootParentId = -1;

    public sealed record Node(int Id, int ParentId);

    public enum Shape
    {
        Deep,
        Wide,
    }

    // The natural authoring of a hierarchy walk: the base relation is the join's LEFT input,
    // so today's structural planner (no cost model) builds the hash on the base every time the
    // recursive member opens. That is exactly the behavior this benchmark is here to size.
    public const string Sql =
        """
        WITH Tree AS (
            SELECT  n.Id, n.ParentId, 0 AS Depth
            FROM    Nodes n
            WHERE   n.ParentId = -1

            UNION ALL

            SELECT  n.Id, n.ParentId, t.Depth + 1
            FROM    Nodes n
                        INNER JOIN Tree t ON n.ParentId = t.Id
        )
        SELECT  Id, Depth
        FROM    Tree
        """;

    public static Node[] BuildRows(Shape shape, int forests)
    {
        var nodes = new Node[forests * NodesPerForest];
        var next = 0;

        for (var f = 0; f < forests; f++)
        {
            var root = f * NodesPerForest;
            nodes[next++] = new Node(root, RootParentId);

            for (var i = 1; i < NodesPerForest; i++)
            {
                var id = root + i;
                var parentId = shape == Shape.Deep ? id - 1 : root;
                nodes[next++] = new Node(id, parentId);
            }
        }

        return nodes;
    }
}
