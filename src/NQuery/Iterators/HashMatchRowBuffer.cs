namespace NQuery.Iterators
{
    internal sealed class HashMatchRowBuffer : RowBuffer
    {
        private readonly IndirectedRowBuffer _build;
        private readonly HashMatchEntryRowBuffer _buildEntry;
        private readonly NullRowBuffer _buildNull;

        private readonly IndirectedRowBuffer _probe;
        private readonly NullRowBuffer _probeNull;

        public HashMatchRowBuffer(int buildCount, int probeCount)
        {
            _build = new IndirectedRowBuffer(buildCount);
            _buildEntry = new HashMatchEntryRowBuffer();
            _buildNull = new NullRowBuffer(buildCount);

            _probe = new IndirectedRowBuffer(probeCount);
            _probeNull = new NullRowBuffer(probeCount);
        }

        public void SetBuild(HashMatchEntry entry)
        {
            _buildEntry.Entry = entry;
            _build.ActiveRowBuffer = entry is null ? _buildNull : _buildEntry;
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
}