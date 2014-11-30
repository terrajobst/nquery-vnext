using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Linq;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class ExplicitJoinCodeRefactoringProvider : CodeRefactoringProvider<WhereClauseSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, WhereClauseSyntax node)
        {
            var selectQuery = node.Ancestors().OfType<SelectQuerySyntax>().First();

            var links = GetTableLinks(semanticModel, selectQuery);
            if (!links.Any())
                return Enumerable.Empty<ICodeAction>();

            return new[] {new ExplicitJoinCode(selectQuery, links)};
        }

        private static ImmutableArray<TableLink> GetTableLinks(SemanticModel semanticModel, SelectQuerySyntax selectQuery)
        {
            var tableIndices = GetDeclaredTables(semanticModel, selectQuery).Select((t, i) => Tuple.Create(t, i)).ToDictionary(t => t.Item1, t => t.Item2);
            var conjunctions = SplitConjunctions(selectQuery.WhereClause.Predicate);

            var tableLinks = new List<TableLink>();

            foreach (var conjunction in conjunctions)
            {
                var dependencies = GetColumnDependencies(semanticModel, conjunction).GroupBy(c => c.TableInstance).ToImmutableArray();
                if (dependencies.Length == 2)
                {
                    var table1 = dependencies[0].Key;
                    var table2 = dependencies[1].Key;

                    int index1;
                    int index2;

                    if (!tableIndices.TryGetValue(table1, out index1))
                        continue;

                    if (!tableIndices.TryGetValue(table2, out index2))
                        continue;

                    var firstTable = index1 < index2 ? table1 : table2;
                    var secondTable = firstTable != table1 ? table1 : table2;
                    var tableLink = new TableLink(firstTable, secondTable, conjunction);
                    tableLinks.Add(tableLink);
                }
            }

            return tableLinks.ToImmutableArray();
        }

        private static ImmutableArray<TableChain> GetTableChains(ImmutableArray<TableLink> links)
        {
            var graph = GetTableGraph(links);
            PrintGraph(graph, "Step1_Raw");
            TopologicalSort(graph);
            PrintGraph(graph, "Step2_Sort");
            RemoveShortcuts(graph);
            PrintGraph(graph, "Step3_ShortcutsRemoved");

            var startNodes = graph.Where(n => !n.Incoming.Any());
            return startNodes.Select(GetTableChain).ToImmutableArray();
        }

        private static void PrintGraph(ImmutableArray<TableNode> graph, string step)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = Path.Combine(desktop, step + ".dgml");

            var current = 'a';
            var nodeNames = new Dictionary<TableNode, string>();
            foreach (var node in graph)
            {
                var name = current.ToString();
                current++;
                nodeNames.Add(node, name);
            }

            var edges = graph.SelectMany(n => n.Incoming.Concat(n.Outgoing)).Distinct();

            const string nsp = "http://schemas.microsoft.com/vs/2009/dgml";
            var doc =
                new XElement(XName.Get("DirectedGraph", nsp),
                    new XElement(XName.Get("Nodes", nsp),
                        from n in graph
                        select new XElement(XName.Get("Node", nsp),
                            new XAttribute("Id", nodeNames[n]),
                            new XAttribute("Label", n.Order == 0 ? n.Table.Name : n.Table.Name + " : " + n.Order))
                        ),
                    new XElement(XName.Get("Links", nsp),
                        from e in edges
                        select new XElement(XName.Get("Link", nsp),
                            new XAttribute("Source", nodeNames[e.From]),
                            new XAttribute("Target", nodeNames[e.To]),
                            new XAttribute("Label", e.Predicate.ToString().Trim()))
                        )
                    );

            doc.Save(path);
        }

        private static TableChain GetTableChain(TableNode node)
        {
            var links = new List<TableLink>();
            GetTableLinks(links, node);
            return new TableChain(links);
        }

        private static void GetTableLinks(List<TableLink> receiver, TableNode node)
        {
            foreach (var edge in node.Outgoing)
            {
                var link = new TableLink(node.Table, edge.To.Table, edge.Predicate);
                receiver.Add(link);
                GetTableLinks(receiver, edge.To);
            }
        }

        private static ImmutableArray<TableNode> GetTableGraph(IEnumerable<TableLink> links)
        {
            var nodes = new Dictionary<TableInstanceSymbol, TableNode>();
            foreach (var tableLink in links)
            {
                var from = GetOrCreateTableNode(nodes, tableLink.From);
                var to = GetOrCreateTableNode(nodes, tableLink.To);
                var edge = new TableEdge(from, to, tableLink.Predicate);
                from.Outgoing.Add(edge);
                to.Incoming.Add(edge);
            }

            return nodes.Values.ToImmutableArray();
        }

        private static void RemoveShortcuts(IEnumerable<TableNode> nodes)
        {
            foreach (var node in nodes)
            {
                for (var i = node.Incoming.Count - 1; i >= 0; i--)
                {
                    var edge = node.Incoming[i];
                    var delta = edge.From.Order - node.Order;
                    if (delta > 1)
                    {
                        edge.From.Outgoing.Remove(edge);
                        edge.To.Incoming.Remove(edge);
                    }
                }
            }
        }

        private static void TopologicalSort(IEnumerable<TableNode> nodes)
        {
            var startNodes = nodes.Where(n => !n.Incoming.Any());
            var seenNodes = new HashSet<TableNode>();

            var current = 0;
            foreach (var node in startNodes)
                TopologicalSort(ref current, seenNodes, node);
        }

        private static void TopologicalSort(ref int current, ISet<TableNode> seenNodes, TableNode node)
        {
            if (seenNodes.Contains(node))
                return;

            foreach (var link in node.Outgoing)
                TopologicalSort(ref current, seenNodes, link.To);

            current++;
            node.Order = current;
        }

        private static TableNode GetOrCreateTableNode(Dictionary<TableInstanceSymbol, TableNode> tableNodes, TableInstanceSymbol symbol)
        {
            TableNode result;
            if (!tableNodes.TryGetValue(symbol, out result))
            {
                result = new TableNode(symbol);
                tableNodes.Add(symbol, result);
            }

            return result;
        }

        private sealed class TableLink
        {
            private readonly TableInstanceSymbol _from;
            private readonly TableInstanceSymbol _to;
            private readonly ExpressionSyntax _predicate;

            public TableLink(TableInstanceSymbol from, TableInstanceSymbol to, ExpressionSyntax predicate)
            {
                _from = from;
                _to = to;
                _predicate = predicate;
            }

            public TableInstanceSymbol From
            {
                get { return _from; }
            }

            public TableInstanceSymbol To
            {
                get { return _to; }
            }

            public ExpressionSyntax Predicate
            {
                get { return _predicate; }
            }

            public override string ToString()
            {
                return string.Format("{0} --> {1}",_from.Name, _to.Name);
            }
        }

        private sealed class TableChain
        {
            private readonly ImmutableArray<TableLink> _links;

            public TableChain(IEnumerable<TableLink> links)
            {
                _links = links.ToImmutableArray();
            }

            public ImmutableArray<TableLink> Links
            {
                get { return _links; }
            }

            public override string ToString()
            {
                return string.Join(", ", _links);
            }
        }

        private sealed class TableNode
        {
            private readonly TableInstanceSymbol _table;
            private readonly List<TableEdge> _incoming = new List<TableEdge>();
            private readonly List<TableEdge> _outgoing = new List<TableEdge>();

            public TableNode(TableInstanceSymbol table)
            {
                _table = table;
            }

            public int Order { get; set; }

            public TableInstanceSymbol Table
            {
                get { return _table; }
            }

            public List<TableEdge> Incoming
            {
                get { return _incoming; }
            }

            public List<TableEdge> Outgoing
            {
                get { return _outgoing; }
            }
        }

        private sealed class TableEdge
        {
            private readonly TableNode _from;
            private readonly TableNode _to;
            private readonly ExpressionSyntax _predicate;

            public TableEdge(TableNode from, TableNode to, ExpressionSyntax predicate)
            {
                _from = from;
                _to = to;
                _predicate = predicate;
            }

            public TableNode From
            {
                get { return _from; }
            }

            public TableNode To
            {
                get { return _to; }
            }

            public ExpressionSyntax Predicate
            {
                get { return _predicate; }
            }
        }

        private sealed class UnionFind<T>
        {
            private readonly T _value;

            private UnionFind<T> _parent;
            private int _rank;

            public UnionFind(T value)
            {
                _value = value;
                _parent = this;
            }

            public UnionFind<T> Find()
            {
                if (_parent != this)
                    _parent = _parent.Find();

                return _parent;
            }

            public void Union(UnionFind<T> other)
            {
                var xRoot = Find();
                var yRoot = other.Find();

                if (xRoot == yRoot)
                    return;

                if (xRoot._rank < yRoot._rank)
                    xRoot._parent = yRoot;
                else if (xRoot._rank > yRoot._rank)
                    yRoot._parent = xRoot;
                else
                {
                    yRoot._parent = xRoot;
                    xRoot._rank++;
                }
            }

            public T Value
            {
                get { return _value; }
            }
        }

        private static ImmutableArray<TableChain> GetTableChains2(ImmutableArray<TableLink> links)
        {
            var graph = GetJoinGraph(links);
            var roots = MakeSpanningTree(graph);
            return roots.Select(GetTableChain).ToImmutableArray();
        }

        private static ImmutableArray<JoinNode> GetJoinGraph(ImmutableArray<TableLink> links)
        {
            var nodes = links.Select(l => l.From)
                             .Concat(links.Select(l => l.To))
                             .Distinct()
                             .Select(t => new JoinNode(t))
                             .ToDictionary(n => n.Table);

            foreach (var link in links)
            {
                var node1 = nodes[link.From];
                var node2 = nodes[link.To];
                var edge = new JoinEdge(node1, node2, link.Predicate);
                node1.Edges.Add(edge);
                node2.Edges.Add(edge);
            }

            return nodes.Values.ToImmutableArray();
        }

        private static ImmutableArray<JoinNode> MakeSpanningTree(ImmutableArray<JoinNode> graph)
        {
            var edges = graph.SelectMany(n => n.Edges).Distinct().ToImmutableArray();
            var unionFindNodes = graph.Select(n => new UnionFind<JoinNode>(n)).ToDictionary(u => u.Value);

            foreach (var edge in edges)
            {
                var tree1 = unionFindNodes[edge.Node1];
                var tree2 = unionFindNodes[edge.Node2];
                var causesCycle = tree1.Find() == tree2.Find();

                if (causesCycle)
                {
                    edge.Node1.Edges.Remove(edge);
                    edge.Node2.Edges.Remove(edge);
                }
                else
                {
                    tree1.Union(tree2);
                }
            }

            var roots = unionFindNodes.Values.Select(n => n.Find().Value).Distinct().ToImmutableArray();
            return roots;
        }

        private static TableChain GetTableChain(JoinNode root)
        {
            var result = new List<TableLink>();
            var seenNodes = new HashSet<JoinNode>();
            GetTableLinks(result, seenNodes, root);
            return new TableChain(result);
        }

        private static void GetTableLinks(List<TableLink> receiver, HashSet<JoinNode> seenNodes, JoinNode node)
        {
            foreach (var edge in node.Edges)
            {
                var containsNode1 = !seenNodes.Add(edge.Node1);
                var containsNode2 = !seenNodes.Add(edge.Node2);
                if (containsNode1 && containsNode2)
                    continue;

                var other = containsNode2 ? edge.Node1 : edge.Node2;
                GetTableLinks(receiver, seenNodes, other);

                var link = new TableLink(other.Table, node.Table, edge.Predicate);
                receiver.Add(link);
            }
        }

        private sealed class JoinNode
        {
            private readonly TableInstanceSymbol _table;
            private readonly HashSet<JoinEdge> _edges = new HashSet<JoinEdge>();

            public JoinNode(TableInstanceSymbol table)
            {
                _table = table;
            }

            public TableInstanceSymbol Table
            {
                get { return _table; }
            }

            public ISet<JoinEdge> Edges
            {
                get { return _edges; }
            }

            public override string ToString()
            {
                return _table.Name;
            }
        }

        private sealed class JoinEdge
        {
            private readonly JoinNode _node1;
            private readonly JoinNode _node2;
            private readonly ExpressionSyntax _predicate;

            public JoinEdge(JoinNode node1, JoinNode node2, ExpressionSyntax predicate)
            {
                if (node1 == node2)
                    throw new ArgumentException("Invalid edge");

                _node1 = node1;
                _node2 = node2;
                _predicate = predicate;
            }

            public JoinNode Node1
            {
                get { return _node1; }
            }

            public JoinNode Node2
            {
                get { return _node2; }
            }

            public ExpressionSyntax Predicate
            {
                get { return _predicate; }
            }

            public override string ToString()
            {
                return string.Format("{0} <-> {1}", _node1.Table.Name, _node2.Table.Name);
            }
        }

        private static IEnumerable<TableInstanceSymbol> GetDeclaredTables(SemanticModel semanticModel, SelectQuerySyntax selectQuery)
        {
            // TODO: That's not correct because we must not visit condition (they could contain nested queries).

            return selectQuery.FromClause.DescendantNodes()
                                         .OfType<NamedTableReferenceSyntax>()
                                         .Select(semanticModel.GetDeclaredSymbol)
                                         .Where(s => s != null);
        }

        private static IEnumerable<ExpressionSyntax> SplitConjunctions(ExpressionSyntax expression)
        {
            var stack = new Stack<ExpressionSyntax>();
            stack.Push(expression);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var binary = current as BinaryExpressionSyntax;
                if (binary != null && binary.Kind == SyntaxKind.LogicalAndExpression)
                {
                    stack.Push(binary.Left);
                    stack.Push(binary.Right);
                }
                else
                {
                    yield return current;
                }
            }
        }

        private static IEnumerable<TableColumnInstanceSymbol> GetColumnDependencies(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            return expression.DescendantNodesAndSelf()
                             .OfType<ExpressionSyntax>()
                             .Select(semanticModel.GetSymbol)
                             .Where(s => s != null)
                             .OfType<TableColumnInstanceSymbol>();
        }

        private sealed class ExplicitJoinCode : CodeAction
        {
            private readonly SelectQuerySyntax _selectQuery;
            private readonly ImmutableArray<TableLink> _links;

            public ExplicitJoinCode(SelectQuerySyntax selectQuery, ImmutableArray<TableLink> links)
                : base(selectQuery.SyntaxTree)
            {
                _selectQuery = selectQuery;
                _links = links;
            }

            public override string Description
            {
                get { return string.Format("Replace with explicit JOIN clause"); }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var chains = GetTableChains2(_links);
                var insertLocation = _selectQuery.FromClause.Span.End;
                var text = GenerateFromClause(chains);
                changeSet.InsertText(insertLocation, text);
            }

            private static string GenerateFromClause(ImmutableArray<TableChain> chains)
            {
                var sb = new StringBuilder();
                foreach (var tableChain in chains)
                    GenerateFromClause(sb, tableChain);

                return sb.ToString();
            }

            private static void GenerateFromClause(StringBuilder sb, TableChain tableChain)
            {
                sb.AppendLine(",");

                var isFirst = true;

                foreach (var tableLink in tableChain.Links)
                {
                    if (isFirst)
                    {
                        GenerateNamedTableReference(sb, tableLink.From);
                        isFirst = false;
                    }

                    sb.AppendLine();
                    sb.Append(" INNER JOIN ");
                    GenerateNamedTableReference(sb, tableLink.To);
                    sb.Append(" ON ");
                    sb.Append(tableLink.Predicate);
                }
            }

            private static void GenerateNamedTableReference(StringBuilder sb, TableInstanceSymbol symbol)
            {
                sb.AppendFormat("{0} AS {1}", symbol.Table.Name, symbol.Name);
            }
        }
    }
}