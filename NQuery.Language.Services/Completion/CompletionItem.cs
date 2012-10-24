using System;

using NQuery.Language.Symbols;

namespace NQuery.Language.Services.Completion
{
    public sealed class CompletionItem
    {
        private readonly string _displayText;
        private readonly string _insertionText;
        private readonly string _description;
        private readonly NQueryGlyph? _glyph;
        private readonly Symbol _symbol;

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph)
            : this(displayText, insertionText, description, glyph, null)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, NQueryGlyph? glyph, Symbol symbol)
        {
            _displayText = displayText;
            _insertionText = insertionText;
            _description = description;
            _glyph = glyph;
            _symbol = symbol;
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
    }
}