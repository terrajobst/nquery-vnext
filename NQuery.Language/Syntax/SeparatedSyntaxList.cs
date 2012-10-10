using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    public sealed class SeparatedSyntaxList<TNode> : IList<TNode>
        where TNode: SyntaxNode
    {
        private readonly Entry[] _entries;

        public static readonly SeparatedSyntaxList<TNode> Empty = new SeparatedSyntaxList<TNode>(new SyntaxNodeOrToken[0]);

        public SeparatedSyntaxList(ICollection<SyntaxNodeOrToken> nodeOrTokens)
        {
            ValidateEntries(nodeOrTokens);
            _entries = ReadEntries(nodeOrTokens);
        }

        private static void ValidateEntries(IEnumerable<SyntaxNodeOrToken> nodeOrTokens)
        {
            SyntaxNodeOrToken? last = null;

            foreach (var nodeOrToken in nodeOrTokens)
            {
                // We expect a node if either of the following is true
                //
                //    * It's the first element
                //    * The last element was a separator
                //    * The current element is a node

                var requiresValidNode = (last == null ||
                                         !last.Value.IsNode ||
                                         nodeOrToken.IsNode);
                if (requiresValidNode)
                {
                    var isValidNode = nodeOrToken.IsNode && nodeOrToken.AsNode() is TNode;
                    if (!isValidNode)
                        throw new ArgumentException("nodeOrTokens");
                }

                last = nodeOrToken;
            }
        }

        private static Entry[] ReadEntries(ICollection<SyntaxNodeOrToken> nodeOrTokens)
        {
            var entryCount = (nodeOrTokens.Count + 1)/2;
            var entries = new Entry[entryCount];

            var entryIndex = 0;

            foreach (var nodeOrToken in nodeOrTokens)
            {
                if (nodeOrToken.IsToken)
                {
                    var itemIndex = entryIndex - 1;
                    var lastEntry = entries[itemIndex];
                    var separator = nodeOrToken.AsToken();
                    entries[itemIndex] = new Entry(lastEntry.Node, separator);
                }
                else
                {
                    var node = (TNode) nodeOrToken.AsNode();
                    entries[entryIndex] = new Entry(node, null);
                    entryIndex++;
                }
            }
            return entries;
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            return _entries.Select(e => e.Node).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SyntaxToken GetSeparator(int index)
        {
            return _entries[index].Separator;
        }

        public IEnumerable<SyntaxNodeOrToken> GetWithSeparators()
        {
            foreach (var entry in _entries)
            {
                yield return entry.Node;
                if (entry.Separator != null)
                    yield return entry.Separator;
            }            
        }

        public IEnumerable<SyntaxToken> GetSeparators()
        {
            return from e in _entries
                   where e.Separator != null
                   select e.Separator;
        }

        public TNode this[int index]
        {
            get { return _entries[index].Node; }
        }

        public bool Contains(TNode item)
        {
            return _entries.Any(e => e.Node == item);
        }

        void ICollection<TNode>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<TNode>.Add(TNode item)
        {
            throw new NotSupportedException();
        }

        int IList<TNode>.IndexOf(TNode item)
        {
            throw new NotSupportedException();
        }

        void IList<TNode>.Insert(int index, TNode item)
        {
            throw new NotSupportedException();
        }

        void IList<TNode>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        bool ICollection<TNode>.Remove(TNode item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<TNode>.IsReadOnly { get { return true; } }

        void ICollection<TNode>.CopyTo(TNode[] array, int arrayIndex)
        {
            for (var i = 0; i < _entries.Length; i++)
                array[arrayIndex + i] = _entries[i].Node;
        }

        TNode IList<TNode>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public int Count
        {
            get { return _entries.Length; }
        }

        private struct Entry
        {
            private readonly TNode _node;
            private readonly SyntaxToken _separator;

            public Entry(TNode node, SyntaxToken separator)
            {
                _node = node;
                _separator = separator;
            }

            public TNode Node
            {
                get { return _node; }
            }

            public SyntaxToken Separator
            {
                get { return _separator; }
            }
        }
    }
}