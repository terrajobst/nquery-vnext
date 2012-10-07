namespace NQuery.Language.VSEditor
{
    public sealed class RawSyntaxTag
    {
        private readonly TextSpan _textSpan;
        private readonly RawSyntaxTagKind _kind;

        public RawSyntaxTag(TextSpan textSpan, RawSyntaxTagKind kind)
        {
            _textSpan = textSpan;
            _kind = kind;
        }

        public TextSpan TextSpan
        {
            get { return _textSpan; }
        }

        public RawSyntaxTagKind Kind
        {
            get { return _kind; }
        }
    }
}