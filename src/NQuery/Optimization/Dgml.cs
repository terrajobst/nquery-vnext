using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NQuery.Optimization
{
    internal static class Dgml
    {
        public static XElement Create<TNode, TEdge>(
            ICollection<TNode> nodes,
            ICollection<TEdge> edges,
            Func<TEdge, TNode> edgeSourceSelector,
            Func<TEdge, TNode> edgeTargetSelector,
            Func<TNode, string> nodeLabelSelector = null,
            Func<TEdge, string> edgeLabelSelector = null)
        {
            var current = 0;
            var nodeId = new Dictionary<TNode, string>();
            foreach (var node in nodes)
            {
                var name = current.ToString();
                current++;
                nodeId.Add(node, name);
            }

            const string nsp = "http://schemas.microsoft.com/vs/2009/dgml";
            var doc =
                new XElement(XName.Get("DirectedGraph", nsp),
                    new XElement(XName.Get("Nodes", nsp),
                        from n in nodes
                        select new XElement(XName.Get("Node", nsp),
                            new XAttribute("Id", nodeId[n]),
                            new XAttribute("Label", nodeLabelSelector?.Invoke(n) ?? n.ToString()))
                        ),
                    new XElement(XName.Get("Links", nsp),
                        from e in edges
                        select new XElement(XName.Get("Link", nsp),
                            new XAttribute("Source", nodeId[edgeSourceSelector(e)]),
                            new XAttribute("Target", nodeId[edgeTargetSelector(e)]),
                            new XAttribute("Label", edgeLabelSelector?.Invoke(e) ?? e.ToString()))
                        )
                    );

            return doc;
        }
    }
}