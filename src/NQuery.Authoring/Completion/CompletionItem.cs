using System;

namespace NQuery.Authoring.Completion
{
    public sealed class CompletionItem
    {
        private readonly string _displayText;
        private readonly string _insertionText;
        private readonly string _description;
        private readonly NQueryGlyph? _glyph;
        private readonly Symbol _symbol;
        private readonly bool _isBuilder;

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph)
            : this(displayText, insertionText, description, glyph, null)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, bool isBuilder)
            : this(displayText, insertionText, description, null, null, isBuilder)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph, Symbol symbol)
            : this(displayText, insertionText, description, glyph, symbol, false)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph, Symbol symbol, bool isBuilder)
        {
            _displayText = displayText;
            _insertionText = insertionText;
            _description = description;
            _glyph = glyph;
            _symbol = symbol;
            _isBuilder = isBuilder;
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

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public bool IsBuilder
        {
            get { return _isBuilder; }
        }
    }
}