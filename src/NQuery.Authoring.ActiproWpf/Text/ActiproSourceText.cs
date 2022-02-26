using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    // I've considered implementing this text buffer directly against
    // the ITextSnapshot instead of parsing the text.
    //
    // The problem is that the ITextSnapshot doesn't counts line
    // breaks.
    //
    // This results in a lot of funky behavior as this means that
    // NQuery's offset doesn't align with the offset that the Actipro's
    // editor would accept.

    internal sealed class ActiproSourceText : SourceText
    {
        private readonly SourceText _sourceText;

        public ActiproSourceText(ActiproSourceTextContainer container, ITextSnapshot snapshot)
            : base(container)
        {
            Snapshot = snapshot;
            _sourceText = From(snapshot.Text);
        }

        public ITextSnapshot Snapshot { get; }

        public override int GetLineNumberFromPosition(int position)
        {
            return _sourceText.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return _sourceText.GetText(textSpan);
        }

        public override char this[int index]
        {
            get { return _sourceText[index]; }
        }

        public override int Length
        {
            get { return _sourceText.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return _sourceText.Lines; }
        }

        public override string ToString()
        {
            return $"Version={Snapshot.Version.Number}, Length={Snapshot.Length}";
        }
    }
}