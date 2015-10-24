using System;
using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Iterators
{
    internal sealed class HashMatchIterator : Iterator
    {
        private readonly BoundHashMatchOperator _logicalOperator;
        private readonly Iterator _build;
        private readonly Iterator _probe;
        private readonly int _buildIndex;
        private readonly int _probeIndex;
        private readonly IteratorPredicate _remainder;
        private readonly HashMatchRowBuffer _rowBuffer;

        private Dictionary<object,Entry> _hashTable;
        private Entry _entry;
        private IEnumerator<Entry> _entryEnumerator;
        private Phase _currentPhase;
        private bool _probeMatched;

        private static readonly object NullKey = new object();

        public HashMatchIterator(BoundHashMatchOperator logicalOperator, Iterator build, Iterator probe, int buildIndex, int probeIndex, IteratorPredicate remainder)
        {
            _logicalOperator = logicalOperator;
            _build = build;
            _probe = probe;
            _buildIndex = buildIndex;
            _probeIndex = probeIndex;
            _remainder = remainder;
            _rowBuffer= new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _build.Open();
            _probe.Open();
            BuildHashtable();

            _entry = null;
            _entryEnumerator = null;
            _currentPhase = Phase.ProduceMatch;
            _probeMatched = true;
        }

        private void BuildHashtable()
        {
            _hashTable = new Dictionary<object, Entry>();

            while (_build.Read())
            {
                var keyValue = _build.RowBuffer[_buildIndex] ?? NullKey;
                var rowValues = new object[_build.RowBuffer.Count];
                _build.RowBuffer.CopyTo(rowValues, 0);
                AddToHashtable(keyValue, rowValues);
            }
        }

        private void AddToHashtable(object keyValue, object[] values)
        {
            Entry entry;
            _hashTable.TryGetValue(keyValue, out entry);

            if (entry == null)
            {
                entry = new Entry();
            }
            else
            {
                var newEntry = new Entry {Next = entry};
                entry = newEntry;
            }

            entry.RowValues = values;
            _hashTable[keyValue] = entry;
        }

        public override bool Read()
        {
            switch (_currentPhase)
            {
                case Phase.ProduceMatch:
                {
                    var matchFound = false;
                    _rowBuffer.SetProbe(_probe.RowBuffer);

                    while (!matchFound)
                    {
                        if (_entry != null)
                            _entry = _entry.Next;

                        if (_entry == null)
                        {
                            // All rows having the same key value are exhausted.

                            if (!_probeMatched && (_logicalOperator == BoundHashMatchOperator.FullOuter ||
                                                   _logicalOperator == BoundHashMatchOperator.RightOuter))
                            {
                                _probeMatched = true;
                                _rowBuffer.SetBuild(null);
                                return true;
                            }

                            // Read next row from probe input.

                            if (!_probe.Read())
                            {
                                // The probe input is exhausted. If we have a full outer or left outer
                                // join we are not finished. We have to return all rows from the build
                                // input that have not been matched with the probe input.

                                if (_logicalOperator == BoundHashMatchOperator.FullOuter ||
                                    _logicalOperator == BoundHashMatchOperator.LeftOuter)
                                {
                                    _currentPhase = Phase.ReturnUnmatchedRowsFromBuildInput;
                                    _entry = null;
                                    goto case Phase.ReturnUnmatchedRowsFromBuildInput;
                                }

                                return false;
                            }

                            // Get probe value

                            _probeMatched = false;
                            var probeValue = _probe.RowBuffer[_probeIndex];

                            // Seek first occurence of probe value

                            if (probeValue != null)
                                _hashTable.TryGetValue(probeValue, out _entry);
                        }

                        if (_entry != null)
                        {
                            _rowBuffer.SetBuild(_entry);

                            if (_remainder())
                            {
                                _entry.Matched = true;
                                matchFound = true;
                                _probeMatched = true;
                            }
                        }
                    }

                    return true;
                }

                case Phase.ReturnUnmatchedRowsFromBuildInput:
                {
                    var unmatchedFound = false;
                    _rowBuffer.SetProbe(null);

                    while (!unmatchedFound)
                    {
                        if (_entry != null)
                            _entry = _entry.Next;

                        if (_entry == null)
                        {
                            if (_entryEnumerator == null)
                                _entryEnumerator = _hashTable.Values.GetEnumerator();

                            // All rows having the same key value are exhausted.
                            // Read next key from build input.

                            if (!_entryEnumerator.MoveNext())
                            {
                                // We have read all keys. So we are finished.
                                return false;
                            }

                            _entry = _entryEnumerator.Current;
                        }

                        unmatchedFound = !_entry.Matched;
                    }

                    _rowBuffer.SetBuild(_entry);
                    return true;
                }

                default:
                    throw new NotImplementedException();
            }
        }

        private sealed class EntryRowBuffer : RowBuffer
        {
            public Entry Entry { get; set; }

            public override int Count
            {
                get { return Entry.RowValues.Length; }
            }

            public override object this[int index]
            {
                get { return Entry.RowValues[index]; }
            }

            public override void CopyTo(object[] array, int destinationIndex)
            {
                var source = Entry.RowValues;
                Array.Copy(source, 0, array, destinationIndex, source.Length);
            }
        }

        private sealed class HashMatchRowBuffer : RowBuffer
        {
            private readonly IndirectedRowBuffer _build;
            private readonly EntryRowBuffer _buildEntry;
            private readonly NullRowBuffer _buildNull;

            private readonly IndirectedRowBuffer _probe;
            private readonly NullRowBuffer _probeNull;

            public HashMatchRowBuffer(int buildCount, int probeCount)
            {
                _build = new IndirectedRowBuffer(buildCount);
                _buildEntry = new EntryRowBuffer();
                _buildNull = new NullRowBuffer(buildCount);

                _probe = new IndirectedRowBuffer(probeCount);
                _probeNull = new NullRowBuffer(probeCount);
            }

            public void SetBuild(Entry entry)
            {
                _buildEntry.Entry = entry;
                _build.ActiveRowBuffer = entry == null ? (RowBuffer) _buildNull : _buildEntry;
            }

            public void SetProbe(RowBuffer rowBuffer)
            {
                _probe.ActiveRowBuffer = rowBuffer ?? _probeNull;
            }

            public override int Count
            {
                get { return _buildNull.Count + _probeNull.Count; }
            }

            public override object this[int index]
            {
                get
                {
                    return index < _build.Count
                               ? _build[index]
                               : _probe[index - _build.Count];
                }
            }

            public override void CopyTo(object[] array, int destinationIndex)
            {
                _build.CopyTo(array, destinationIndex);
                _probe.CopyTo(array, _build.Count + destinationIndex);
            }
        }

        private sealed class Entry
        {
            public object[] RowValues;
            public Entry Next;
            public bool Matched;
        }

        private enum Phase
        {
            ProduceMatch,
            ReturnUnmatchedRowsFromBuildInput
        }
    }
}