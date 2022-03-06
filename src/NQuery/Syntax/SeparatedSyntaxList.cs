using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Syntax
{
    public sealed class SeparatedSyntaxList<TNode> : IList<TNode>, IReadOnlyList<TNode>
        where TNode : SyntaxNode
    {
        private readonly ImmutableArray<Entry> _entries;

        public static readonly SeparatedSyntaxList<TNode> Empty = new(new SyntaxNodeOrToken[0]);

        public SeparatedSyntaxList(IReadOnlyCollection<SyntaxNodeOrToken> nodeOrTokens)
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

                var requiresValidNode = (last is null ||
                                         !last.Value.IsNode ||
                                         nodeOrToken.IsNode);
                if (requiresValidNode)
                {
                    var isValidNode = nodeOrToken.IsNode && nodeOrToken.AsNode() is TNode;
                    if (!isValidNode)
                        throw new ArgumentException(Resources.SeparatedSyntaxListInvalidSequence, nameof(nodeOrTokens));
                }

                last = nodeOrToken;
            }
        }

        private static ImmutableArray<Entry> ReadEntries(IReadOnlyCollection<SyntaxNodeOrToken> nodeOrTokens)
        {
            var entryCount = (nodeOrTokens.Count + 1) / 2;
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
                    var node = (TNode)nodeOrToken.AsNode();
                    entries[entryIndex] = new Entry(node, null);
                    entryIndex++;
                }
            }
            return entries.ToImmutableArray();
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
                if (entry.Separator is not null)
                    yield return entry.Separator;
            }
        }

        public IEnumerable<SyntaxToken> GetSeparators()
        {
            return from e in _entries
                   where e.Separator is not null
                   select e.Separator;
        }

        public TNode this[int index]
        {
            get { return _entries[index].Node; }
        }

        public bool Contains(TNode item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Contains(SyntaxToken separator)
        {
            return IndexOf(separator) >= 0;
        }

        public bool Contains(SyntaxNodeOrToken itemOrSeparator)
        {
            return IndexOf(itemOrSeparator) >= 0;
        }

        void ICollection<TNode>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<TNode>.Add(TNode item)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(TNode item)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Node == item)
                    return i;
            }

            return -1;
        }

        public int IndexOf(SyntaxToken separator)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Separator == separator)
                    return i;
            }

            return -1;
        }

        public int IndexOf(SyntaxNodeOrToken itemOrSeparator)
        {
            if (itemOrSeparator.IsNode)
            {
                if (itemOrSeparator.AsNode() is not TNode node)
                    return -1;

                return IndexOf(node);
            }

            return IndexOf(itemOrSeparator.AsToken());
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
            public Entry(TNode node, SyntaxToken separator)
            {
                Node = node;
                Separator = separator;
            }

            public TNode Node { get; }

            public SyntaxToken Separator { get; }
        }
    }
}