using System.Collections.Immutable;
using System.Text;

using NQuery.Text;

namespace NQuery
{
    public partial class AnnotatedText
    {
        private static readonly IComparer<TextSpan> SpanComparer = Comparer<TextSpan>.Create((x, y) => x.Start.CompareTo(y.Start));
        private static readonly IComparer<TextChange> ChangeComparer = Comparer<TextChange>.Create((x, y) => x.Span.Start.CompareTo(y.Span.Start));

        private sealed class Parser
        {
            private readonly string _text;
            private int _position;
            private readonly StringBuilder _textBuilder = new StringBuilder();
            private readonly ImmutableArray<TextSpan>.Builder _spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            private readonly ImmutableArray<TextChange>.Builder _changeBuilder = ImmutableArray.CreateBuilder<TextChange>();

            public Parser(string text)
            {
                _text = text;
            }

            private char Char
            {
                get { return _position < _text.Length ? _text[_position] : '\0'; }
            }

            public AnnotatedText Parse()
            {
                ParseRoot();

                var text = _textBuilder.ToString();

                _spanBuilder.Sort(SpanComparer);
                var spans = _spanBuilder.ToImmutable();

                _changeBuilder.Sort(ChangeComparer);
                var changes = _changeBuilder.ToImmutable();

                return new AnnotatedText(text, spans, changes);
            }

            private void ParseRoot()
            {
                while (_position < _text.Length)
                {
                    switch (Char)
                    {
                        case '{':
                            ParseSpanOrChange();
                            break;
                        case '|':
                            ParsePosition();
                            break;
                        case ':':
                            throw ColonUnexpected();
                        default:
                            ParseText();
                            break;
                    }
                }
            }

            private void ParseSpanOrChange()
            {
                // Skip initial brace
                _position++;

                var spanStart = _textBuilder.Length;

                while (true)
                {
                    if (Char == '}')
                    {
                        // Skip close brace
                        _position++;
                        var spanEnd = _textBuilder.Length;
                        var span = TextSpan.FromBounds(spanStart, spanEnd);
                        _spanBuilder.Add(span);
                        break;
                    }
                    else if (Char == '{')
                    {
                        ParseSpanOrChange();
                    }
                    else if (Char == '|')
                    {
                        ParsePosition();
                    }
                    else if (Char == ':')
                    {
                        var spanEnd = _textBuilder.Length;

                        // Skip ':'
                        _position++;

                        var newTextStart = _textBuilder.Length;

                        ParseText();

                        var newTextEnd = _textBuilder.Length;
                        var newTextLength = newTextEnd - newTextStart;
                        var newText = _textBuilder.ToString(newTextStart, newTextLength);
                        _textBuilder.Length = newTextStart;

                        if (Char != '}')
                            throw MissingClosingBrace();

                        // Skip closing brace
                        _position++;

                        var span = TextSpan.FromBounds(spanStart, spanEnd);
                        var change = new TextChange(span, newText);
                        _changeBuilder.Add(change);
                        break;
                    }
                    else if (Char == '\0')
                    {
                        throw MissingClosingBrace();
                    }
                    else
                    {
                        ParseText();
                    }
                }
            }

            private FormatException ColonUnexpected()
            {
                var message = $"Character ':' is unexpected at position {_position}.";
                return new FormatException(message);
            }

            private FormatException MissingClosingBrace()
            {
                var message = $"Missing '}}' at position {_position}.";
                return new FormatException(message);
            }

            private void ParsePosition()
            {
                var span = new TextSpan(_textBuilder.Length, 0);
                _position++;
                _spanBuilder.Add(span);
            }

            private void ParseText()
            {
                var start = _position;
                while (_position < _text.Length &&
                       !IsSpecialCharacter(Char))
                {
                    _position++;
                }

                var end = _position;
                var length = end - start;
                var text = _text.Substring(start, length);
                _textBuilder.Append(text);
            }

            private static bool IsSpecialCharacter(char c)
            {
                return c == '{' ||
                       c == ':' ||
                       c == '}' ||
                       c == '|';
            }
        }
    }
}