using System.Collections.Immutable;
using System.Globalization;
using System.Text;

using NQuery.Text;

namespace NQuery.Syntax
{
    internal sealed class Lexer
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly SourceText _text;
        private readonly CharReader _charReader;
        private readonly List<SyntaxTrivia> _leadingTrivia = new List<SyntaxTrivia>();
        private readonly List<SyntaxTrivia> _trailingTrivia = new List<SyntaxTrivia>();
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        private SyntaxKind _kind;
        private SyntaxKind _contextualKind;
        private object _value;
        private int _start;

        public Lexer(SyntaxTree syntaxTree, SourceText text)
        {
            _syntaxTree = syntaxTree;
            _text = text;
            _charReader = new CharReader(text);
        }

        public SyntaxToken Lex()
        {
            _leadingTrivia.Clear();
            _diagnostics.Clear();
            _start = _charReader.Position;
            ReadTrivia(_leadingTrivia, isTrailing: false);
            var leadingTrivia = _leadingTrivia.ToImmutableArray();

            _kind = SyntaxKind.BadToken;
            _contextualKind = SyntaxKind.BadToken;
            _value = null;
            _diagnostics.Clear();
            _start = _charReader.Position;
            ReadToken();
            var end = _charReader.Position;
            var kind = _kind;
            var span = TextSpan.FromBounds(_start, end);
            var text = _text.GetText(span);
            var diagnostics = _diagnostics.ToImmutableArray();

            _trailingTrivia.Clear();
            _diagnostics.Clear();
            _start = _charReader.Position;
            ReadTrivia(_trailingTrivia, isTrailing: true);
            var trailingTrivia = _trailingTrivia.ToImmutableArray();

            return new SyntaxToken(_syntaxTree, kind, _contextualKind, false, span, text, _value, leadingTrivia, trailingTrivia, diagnostics);
        }

        private TextSpan CurrentSpan
        {
            get { return TextSpan.FromBounds(_start, _charReader.Position); }
        }

        private TextSpan CurrentSpanStart
        {
            get { return TextSpan.FromBounds(_start, Math.Min(_start + 2, _text.Length)); }
        }

        private void ReadTrivia(List<SyntaxTrivia> target, bool isTrailing)
        {
            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\n':
                    case '\r':
                        {
                            ReadEndOfLine();
                            AddTrivia(target, SyntaxKind.EndOfLineTrivia);
                            if (isTrailing)
                                return;
                        }
                        break;
                    case '-':
                        if (_charReader.Peek() == '-')
                        {
                            ReadSinglelineComment();
                            AddTrivia(target, SyntaxKind.SingleLineCommentTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case '/':
                        if (_charReader.Peek() == '/')
                        {
                            ReadSinglelineComment();
                            AddTrivia(target, SyntaxKind.SingleLineCommentTrivia);
                        }
                        else if (_charReader.Peek() == '*')
                        {
                            ReadMultilineComment();
                            AddTrivia(target, SyntaxKind.MultiLineCommentTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        if (char.IsWhiteSpace(_charReader.Current))
                        {
                            ReadWhitespace();
                            AddTrivia(target, SyntaxKind.WhitespaceTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
            }
        }

        private void ReadEndOfLine()
        {
            if (_charReader.Current == '\r')
            {
                _charReader.NextChar();

                if (_charReader.Current == '\n')
                    _charReader.NextChar();
            }
            else
            {
                _charReader.NextChar();
            }
        }

        private void ReadSinglelineComment()
        {
            _kind = SyntaxKind.SingleLineCommentTrivia;
            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        return;

                    case '\r':
                    case '\n':
                        return;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadMultilineComment()
        {
            _charReader.NextChar(); // Skip /
            _charReader.NextChar(); // Skip *

            _kind = SyntaxKind.MultiLineCommentTrivia;

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        _diagnostics.ReportUnterminatedComment(CurrentSpanStart);
                        return;

                    case '*':
                        _charReader.NextChar();
                        if (_charReader.Current == '/')
                        {
                            _charReader.NextChar();
                            return;
                        }
                        break;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(_charReader.Current) &&
                   _charReader.Current != '\r' &&
                   _charReader.Current != '\n')
            {
                _charReader.NextChar();
            }
        }

        private void AddTrivia(List<SyntaxTrivia> target, SyntaxKind kind)
        {
            var start = _start;
            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = _text.GetText(span);
            var diagnostics = _diagnostics.ToImmutableArray();
            var trivia = new SyntaxTrivia(_syntaxTree, kind, text, span, null, diagnostics);
            target.Add(trivia);

            _diagnostics.Clear();
            _start = _charReader.Position;
        }

        private void ReadToken()
        {
            switch (_charReader.Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;

                case '~':
                    _kind = SyntaxKind.BitwiseNotToken;
                    _charReader.NextChar();
                    break;

                case '&':
                    _kind = SyntaxKind.AmpersandToken;
                    _charReader.NextChar();
                    break;

                case '|':
                    _kind = SyntaxKind.BarToken;
                    _charReader.NextChar();
                    break;

                case '^':
                    _kind = SyntaxKind.CaretToken;
                    _charReader.NextChar();
                    break;

                case '(':
                    _kind = SyntaxKind.LeftParenthesisToken;
                    _charReader.NextChar();
                    break;

                case ')':
                    _kind = SyntaxKind.RightParenthesisToken;
                    _charReader.NextChar();
                    break;

                case '.':
                    if (char.IsDigit(_charReader.Peek()))
                        ReadNumber();
                    else
                    {
                        _kind = SyntaxKind.DotToken;
                        _charReader.NextChar();
                    }
                    break;

                case '@':
                    _kind = SyntaxKind.AtToken;
                    _charReader.NextChar();
                    break;

                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _charReader.NextChar();
                    break;

                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _charReader.NextChar();
                    break;

                case '*':
                    _charReader.NextChar();
                    if (_charReader.Current != '*')
                    {
                        _kind = SyntaxKind.AsteriskToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.AsteriskAsteriskToken;
                        _charReader.NextChar();
                    }
                    break;

                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _charReader.NextChar();
                    break;

                case '%':
                    _kind = SyntaxKind.PercentToken;
                    _charReader.NextChar();
                    break;

                case ',':
                    _kind = SyntaxKind.CommaToken;
                    _charReader.NextChar();
                    break;

                case '=':
                    _kind = SyntaxKind.EqualsToken;
                    _charReader.NextChar();
                    break;

                case '!':
                    if (_charReader.Peek() == '=')
                    {
                        _kind = SyntaxKind.ExclamationEqualsToken;
                        _charReader.NextChar();
                        _charReader.NextChar();
                    }
                    else if (_charReader.Peek()== '>')
                    {
                        _kind = SyntaxKind.ExclamationGreaterToken;
                        _charReader.NextChar();
                        _charReader.NextChar();
                    }
                    else if (_charReader.Peek() == '<')
                    {
                        _kind = SyntaxKind.ExclamationLessToken;
                        _charReader.NextChar();
                        _charReader.NextChar();
                    }
                    else
                    {
                        ReadInvalidCharacter();
                    }
                    break;

                case '<':
                    _charReader.NextChar();
                    if (_charReader.Current == '<')
                    {
                        _kind = SyntaxKind.LessLessToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '>')
                    {
                        _kind = SyntaxKind.LessGreaterToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.LessEqualToken;
                        _charReader.NextChar();
                    }
                    else
                        _kind = SyntaxKind.LessToken;
                    break;

                case '>':
                    _charReader.NextChar();
                    if (_charReader.Current == '>')
                    {
                        _kind = SyntaxKind.GreaterGreaterToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.GreaterEqualToken;
                        _charReader.NextChar();
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterToken;
                    }
                    break;

                case '\'':
                    ReadString();
                    break;

                case '"':
                    ReadQuotedIdentifier();
                    break;

                case '[':
                    ReadParenthesizedIdentifier();
                    break;

                case '#':
                    ReadDate();
                    break;

                default:
                    if (char.IsLetter(_charReader.Current) || _charReader.Current == '_')
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsDigit(_charReader.Current))
                    {
                        ReadNumber();
                    }
                    else
                    {
                        ReadInvalidCharacter();
                    }

                    break;
            }
        }

        private void ReadInvalidCharacter()
        {
            var c = _charReader.Current;
            _charReader.NextChar();
            _diagnostics.ReportIllegalInputCharacter(CurrentSpan, c);
        }

        private void ReadString()
        {
            _kind = SyntaxKind.StringLiteralToken;

            // Skip first single quote
            _charReader.NextChar();

            var sb = new StringBuilder();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        _diagnostics.ReportUnterminatedString(CurrentSpanStart);
                        goto ExitLoop;

                    case '\'':
                        _charReader.NextChar();

                        if (_charReader.Current != '\'')
                            goto ExitLoop;

                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;

                    default:
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;
                }
            }

        ExitLoop:
            _value = sb.ToString();
        }

        private void ReadDate()
        {
            _kind = SyntaxKind.DateLiteralToken;

            // Skip initial #
            _charReader.NextChar();

            var sb = new StringBuilder();

            // Just read everything that looks like it could be a date -- we will
            // verify it afterwards by proper DateTime parsing.

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        _diagnostics.ReportUnterminatedDate(CurrentSpanStart);
                        goto ExitLoop;

                    case '#':
                        _charReader.NextChar();
                        goto ExitLoop;

                    default:
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;
                }
            }

        ExitLoop:
            var text = sb.ToString();
            DateTime result;
            if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                _diagnostics.ReportInvalidDate(CurrentSpan, text);

            _value = result;
        }

        private void ReadNumber()
        {
            _kind = SyntaxKind.NumericLiteralToken;

            // Just read everything that looks like it could be a number -- we will
            // verify it afterwards by proper number parsing.

            var sb = new StringBuilder();
            var hasExponentialModifier = false;
            var hasDotModifier = false;

            while (true)
            {
                switch (_charReader.Current)
                {
                    // dot
                    case '.':

                        // "10.Equals" should not be recognized as a number.

                        var peek1 = _charReader.Peek(1);
                        var peek2 = _charReader.Peek(2);
                        var startsFloatingPoint = char.IsDigit(peek1) ||
                                                  ((peek1 == 'e' || peek1 == 'E') && (peek2 == '+' || peek2 == '-' || char.IsDigit(peek2)));
                        if (!startsFloatingPoint)
                            goto ExitLoop;

                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        hasDotModifier = true;
                        break;

                    // special handling for e, it could be the exponent indicator
                    // followed by an optional sign

                    case 'E':
                    case 'e':
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        hasExponentialModifier = true;
                        if (_charReader.Current == '-' || _charReader.Current == '+')
                        {
                            sb.Append(_charReader.Current);
                            _charReader.NextChar();
                        }
                        break;

                    default:
                        if (!char.IsLetterOrDigit(_charReader.Current))
                            goto ExitLoop;
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;
                }
            }

            ExitLoop:

            var text = sb.ToString();
            _value = hasDotModifier || hasExponentialModifier
                         ? ReadDouble(text)
                         : ReadInt32OrInt64(text);
        }

        private double ReadDouble(string text)
        {
            try
            {
                return double.Parse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                _diagnostics.ReportNumberTooLarge(CurrentSpan, text);
            }
            catch (FormatException)
            {
                _diagnostics.ReportInvalidReal(CurrentSpan, text);
            }
            return 0.0;
        }

        private object ReadInt32OrInt64(string text)
        {
            var int64 = ReadInt64(text);

            // If the integer can be represented as Int32 we return
            // an Int32 literal. Otherwise we return an Int64.

            try
            {
                checked
                {
                    return (int)int64;
                }
            }
            catch (OverflowException)
            {
                return int64;
            }
        }

        private long ReadInt64(string text)
        {
            // Get indicator

            var indicator = text[text.Length - 1];

            // Remove trailing indicator (h, b, or o)

            var textWithoutIndicator = text.Substring(0, text.Length - 1);

            switch (indicator)
            {
                case 'H':
                case 'h':
                    try
                    {
                        return long.Parse(textWithoutIndicator, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException)
                    {
                        _diagnostics.ReportNumberTooLarge(CurrentSpan, textWithoutIndicator);
                    }
                    catch (FormatException)
                    {
                        _diagnostics.ReportInvalidHex(CurrentSpan, textWithoutIndicator);
                    }

                    return 0;

                case 'B':
                case 'b':
                    try
                    {
                        return ReadBinaryValue(textWithoutIndicator);
                    }
                    catch (OverflowException)
                    {
                        _diagnostics.ReportNumberTooLarge(CurrentSpan, textWithoutIndicator);
                    }

                    return 0;

                case 'O':
                case 'o':
                    try
                    {
                        return ReadOctalValue(textWithoutIndicator);
                    }
                    catch (OverflowException)
                    {
                        _diagnostics.ReportNumberTooLarge(CurrentSpan, textWithoutIndicator);
                    }

                    return 0;

                default:
                    try
                    {
                        return long.Parse(text, CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException)
                    {
                        _diagnostics.ReportNumberTooLarge(CurrentSpan, text);
                    }
                    catch (FormatException)
                    {
                        _diagnostics.ReportInvalidInteger(CurrentSpan, text);
                    }

                    return 0;
            }
        }

        private long ReadBinaryValue(string binary)
        {
            long val = 0;

            for (int i = binary.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (binary[i] == '0')
                {
                    // Nothing to add
                }
                else if (binary[i] == '1')
                {
                    checked
                    {
                        // Don't use >> because this implicitly casts the operator to Int32.
                        // Also this operation will never detect an overflow.
                        val += (long)Math.Pow(2, j);
                    }
                }
                else
                {
                    _diagnostics.ReportInvalidBinary(CurrentSpan, binary);
                    return 0;
                }
            }

            return val;
        }

        private long ReadOctalValue(string octal)
        {
            long val = 0;

            for (int i = octal.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int c;

                try
                {
                    c = int.Parse(new string(octal[i], 1), CultureInfo.InvariantCulture);

                    if (c > 7)
                    {
                        _diagnostics.ReportInvalidOctal(CurrentSpan, octal);
                        return 0;
                    }
                }
                catch (FormatException)
                {
                    _diagnostics.ReportInvalidOctal(CurrentSpan, octal);
                    return 0;
                }

                checked
                {
                    val += (long)(c * Math.Pow(8, j));
                }
            }

            return val;
        }

        private void ReadIdentifierOrKeyword()
        {
            var start = _charReader.Position;

            // Skip first letter
            _charReader.NextChar();

            // The following characters can be letters, digits the underscore and the dollar sign.

            while (char.IsLetterOrDigit(_charReader.Current) ||
                   _charReader.Current == '_' ||
                   _charReader.Current == '$')
            {
                _charReader.NextChar();
            }

            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = _text.GetText(span);

            _kind = SyntaxFacts.GetKeywordKind(text);
            _contextualKind = SyntaxFacts.GetContextualKeywordKind(text);
            _value = text;
        }

        private void ReadQuotedIdentifier()
        {
            _kind = SyntaxKind.IdentifierToken;

            // Skip initial quote
            _charReader.NextChar();

            var sb = new StringBuilder();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        _diagnostics.ReportUnterminatedQuotedIdentifier(CurrentSpanStart);
                        goto ExitLoop;

                    case '"':
                        if (_charReader.Peek() != '"')
                        {
                            _charReader.NextChar();
                            goto ExitLoop;
                        }
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        _charReader.NextChar();
                        break;

                    default:
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;
                }
            }

        ExitLoop:
            var text = sb.ToString();
            _value = text;
        }

        private void ReadParenthesizedIdentifier()
        {
            _kind = SyntaxKind.IdentifierToken;

            // Skip initial [
            _charReader.NextChar();

            var sb = new StringBuilder();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        _diagnostics.ReportUnterminatedParenthesizedIdentifier(CurrentSpanStart);
                        goto ExitLoop;

                    case ']':
                        if (_charReader.Peek() != ']')
                        {
                            _charReader.NextChar();
                            goto ExitLoop;
                        }
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        _charReader.NextChar();
                        break;

                    default:
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();
                        break;
                }
            }

        ExitLoop:
            var text = sb.ToString();
            _value = text;
        }
    }
}