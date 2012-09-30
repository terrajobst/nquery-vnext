namespace NQuery.Language.VSEditor.Completion
{
    public sealed class CompletionItem
    {
        private readonly string _displayText;
        private readonly string _insertionText;
        private readonly string _description;
        private readonly NQueryGlyph? _glyph;

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph)
        {
            _displayText = displayText;
            _insertionText = insertionText;
            _description = description;
            _glyph = glyph;
        }

        public string DisplayText
        {
            get { return _displayText; }
        }

        public string InsertionText
        {
            get { return _insertionText; }
        }

        public string Description
        {
            get { return _description; }
        }

        public NQueryGlyph? Glyph
        {
            get { return _glyph; }
        }
    }
}