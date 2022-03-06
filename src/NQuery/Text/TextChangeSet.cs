using System.Collections;

namespace NQuery.Text
{
    public sealed class TextChangeSet : IEnumerable<TextChange>
    {
        private readonly List<TextChange> _changes = new();

        public void ReplaceText(TextSpan span, string newText)
        {
            ArgumentNullException.ThrowIfNull(newText);

            var change = TextChange.ForReplacement(span, newText);
            RegisterChange(change);
        }

        public void InsertText(int position, string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            var change = TextChange.ForInsertion(position, text);
            RegisterChange(change);
        }

        public void DeleteText(TextSpan span)
        {
            var change = TextChange.ForDeletion(span);
            RegisterChange(change);
        }

        private void RegisterChange(TextChange newChange)
        {
            var conflicts = _changes.Any(existingChange => existingChange.Span.IntersectsWith(newChange.Span));
            if (conflicts)
            {
                var message = string.Format(Resources.CannotRegisterOverlappingChange, newChange);
                throw new InvalidOperationException(message);
            }

            _changes.Add(newChange);
        }

        public IEnumerator<TextChange> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}