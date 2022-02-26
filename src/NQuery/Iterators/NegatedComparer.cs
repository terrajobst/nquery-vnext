using System.Collections;

namespace NQuery.Iterators
{
    internal sealed class NegatedComparer : IComparer
    {
        private readonly IComparer _comparer;

        public NegatedComparer(IComparer comparer)
        {
            _comparer = comparer;
        }

        public int Compare(object x, object y)
        {
            return -_comparer.Compare(x, y);
        }
    }
}