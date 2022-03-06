﻿using System.Collections.Immutable;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NQuery.Authoring.Wpf
{
    internal static class TreeViewExtensions
    {
        private sealed class Expander
        {
            private readonly ImmutableArray<object> _items;
            private readonly bool _expandLastItem;

            private ItemContainerGenerator _parentGenerator;
            private int _next;
            private TreeViewItem _lastItem;

            public Expander(TreeView treeView, IEnumerable<object> items, bool expandLastItem)
            {
                _items = items.ToImmutableArray();
                _expandLastItem = expandLastItem;
                _parentGenerator = treeView.ItemContainerGenerator;
            }

            public void ExpandNext()
            {
            continueWithNextItem:

                var item = _items[_next];
                _next++;

                if (_parentGenerator.ContainerFromItem(item) is TreeViewItem childNode)
                {
                    var generator = childNode.ItemContainerGenerator;
                    _parentGenerator = generator;
                    _lastItem = childNode;

                    if (_next >= _items.Length)
                    {
                        _lastItem.IsSelected = true;
                        _lastItem.BringIntoView();

                        if (_expandLastItem)
                            _lastItem.IsExpanded = true;

                        return;
                    }

                    if (generator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        childNode.IsExpanded = true;
                        goto continueWithNextItem;
                    }

                    generator.StatusChanged += ItemContainerGeneratorStatusChanged;
                    childNode.IsExpanded = true;
                }
            }

            private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
            {
                var generator = (ItemContainerGenerator)sender;
                if (generator.Status == GeneratorStatus.ContainersGenerated)
                {
                    generator.StatusChanged -= ItemContainerGeneratorStatusChanged;
                    ExpandNext();
                }
            }
        }

        private static void ExpandNodes(this TreeView treeView, IEnumerable<object> items, bool expandNode)
        {
            var expander = new Expander(treeView, items, expandNode);
            expander.ExpandNext();
        }

        public static void SelectNode<T>(this TreeView treeView, T node, Func<T, T> getParentNode, bool expandNode = false)
            where T : class
        {
            if (node is null)
                return;

            var nodeStack = new Stack<object>();
            while (node is not null)
            {
                nodeStack.Push(node);
                node = getParentNode(node);
            }

            var nodesToExpand = nodeStack.ToImmutableArray();
            treeView.ExpandNodes(nodesToExpand, expandNode);
        }
    }
}