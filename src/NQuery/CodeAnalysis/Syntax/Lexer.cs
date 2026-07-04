using System.Collections.Immutable;
using System.Globalization;
using System.Text;

using NQuery.CodeAnalysis.Text;

namespace NQuery.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly SyntaxTree _syntaxTree;
    private readonly SourceText _text;
    private readonly CharReader _charReader;
    private readonly List<SyntaxTrivia> _leadingTrivia = new();
    private readonly List<SyntaxTrivia> _trailingTrivia = new();
    private readonly List<Diagnostic> _diagnostics = new();

    private SyntaxKind _kind;
    private SyntaxKind _contextualKind;
    private object? _value;
    private int _start;

    public Lexer(SyntaxTree syntaxTree, SourceText text)
    {
        ThrowIfNull(syntaxTree);
        ThrowIfNull(text);

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
        var diagnostics = _diagnostics.ToImmutableArray();

        _trailingTrivia.Clear();
        _diagnostics.Clear();
        _start = _charReader.Position;
        ReadTrivia(_trailingTrivia, isTrailing: true);
        var trailingTrivia = _trailingTrivia.ToImmutableArray();

        return new SyntaxToken(_syntaxTree, kind, _contextualKind, false, span, _value, leadingTrivia, trailingTrivia, diagnostics);
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
                    AddTrivia(target);
                    if (isTrailing)
                        return;
                }
                break;
                case '-':
                    if (_charReader.Peek() == '-')
                    {
                        ReadSinglelineComment();
                        AddTrivia(target);
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
                        AddTrivia(target);
                    }
                    else if (_charReader.Peek() == '*')
                    {
                        ReadMultilineComment();
                        AddTrivia(target);
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
                        AddTrivia(target);
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
        _kind = SyntaxKind.EndOfLineTrivia;

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
        _kind = SyntaxKind.WhitespaceTrivia;

        while (char.IsWhiteSpace(_charReader.Current) &&
               _charReader.Current != '\r' &&
               _charReader.Current != '\n')
        {
            _charReader.NextChar();
        }
    }

    private void AddTrivia(List<SyntaxTrivia> target)
    {
        var start = _start;
        var end = _charReader.Position;
        var span = TextSpan.FromBounds(start, end);
        var diagnostics = _diagnostics.ToImmutableArray();
        var trivia = new SyntaxTrivia(_syntaxTree, _kind, span, null, diagnostics);
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
                if (char.IsAsciiDigit(_charReader.Peek()))
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
                else if (_charReader.Peek() == '>')
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
                else if (char.IsAsciiDigit(_charReader.Current))
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
        if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
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
                    var startsFloatingPoint = char.IsAsciiDigit(peek1) ||
                                              ((peek1 == 'e' || peek1 == 'E') && (peek2 == '+' || peek2 == '-' || char.IsAsciiDigit(peek2)));
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
                    if (!char.IsAsciiDigit(_charReader.Current))
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
        // .NET Core parses out-of-range magnitudes to +/-Infinity rather than
        // failing the parse (unlike .NET Framework). Treat both a parse failure
        // and an overflow as an invalid floating-point literal.
        if (!double.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var result) ||
            double.IsInfinity(result))
        {
            _diagnostics.ReportInvalidReal(CurrentSpan, text);
            return 0.0;
        }

        return result;
    }

    private object ReadInt32OrInt64(string text)
    {
        var int64 = ReadInt64(text);

        // If the integer can be represented as Int32 we return an Int32
        // literal. Otherwise we return an Int64. Note the separate return
        // statements matter: a ternary would unify both branches to Int64
        // and box the value as Int64 even when it fits in an Int32.

        if (int64 is >= int.MinValue and <= int.MaxValue)
            return (int)int64;

        return int64;
    }

    private long ReadInt64(string text)
    {
        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        // ReadNumber only routes ASCII-digit text here (char.IsAsciiDigit), so a
        // parse failure can only mean the value doesn't fit in an Int64. Using
        // char.IsAsciiDigit rather than char.IsDigit matters: the latter also
        // accepts Unicode decimal digits (e.g. Arabic-Indic) that InvariantCulture
        // parsing rejects, which would surface here as a misleading overflow.
        _diagnostics.ReportNumberTooLarge(CurrentSpan, text);
        return 0;
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
                        if (sb.Length == 0)
                            _diagnostics.ReportEmptyQuotedIdentifier(CurrentSpan);
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
                        if (sb.Length == 0)
                            _diagnostics.ReportEmptyParenthesizedIdentifier(CurrentSpan);
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
