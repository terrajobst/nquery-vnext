using System.Collections;

namespace NQuery.Text
{
    public abstract class TextLineCollection : IReadOnlyList<TextLine>
    {
        public abstract IEnumerator<TextLine> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int Count { get; }

        public abstract TextLine this[int index] { get; }
    }
}