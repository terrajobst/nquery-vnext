using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NQuery.Language
{
    internal sealed class Lexer
    {
        private readonly string _source;
        private readonly CharReader _charReader;
        private readonly List<SyntaxTrivia> _leadingTrivia = new List<SyntaxTrivia>();
        private readonly List<SyntaxTrivia> _trailingTrivia = new List<SyntaxTrivia>();
        
        private SyntaxKind _kind;
        private SyntaxKind _contextualKind;
        private object _value;

        public Lexer(string source)
        {
            _source = source;
            _charReader = new CharReader(source);
        }

        public SyntaxToken Lex()
        {
            _leadingTrivia.Clear();
            ReadTrivia(_leadingTrivia, isLeading: false);
            var leadingTrivia = _leadingTrivia.ToArray();

            _kind = SyntaxKind.BadToken;
            _contextualKind = SyntaxKind.BadToken;
            _value = null;

            var start = _charReader.Position;
            ReadToken();
            var end = _charReader.Position;
            var kind = _kind;
            var span = TextSpan.FromBounds(start, end);
            var text = string.IsNullOrEmpty(SyntaxFacts.GetText(kind))
                           ? GetText(span)
                           : null;

            // TODO: Get errors

            _trailingTrivia.Clear();
            ReadTrivia(_trailingTrivia, isLeading: true);
            var trailingTrivia = _trailingTrivia.ToArray();

            return new SyntaxToken(kind, _contextualKind, false, span, text, _value, leadingTrivia, trailingTrivia);
        }

        private void ReadTrivia(List<SyntaxTrivia> target, bool isLeading)
        {
            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\n':
                    case '\r':
                        {
                            var start = _charReader.Position;
                            ReadEndOfLine();
                            AddTrivia(target, start, SyntaxKind.EndOfLineTrivia);
                            if (isLeading)
                                return;
                        }
                        break;
                    case '-':
                        if (_charReader.Peek() == '-')
                        {
                            var start = _charReader.Position;
                            ReadSinglelineComment();
                            AddTrivia(target, start, SyntaxKind.SingleLineCommentTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case '/':
                        if (_charReader.Peek() == '/')
                        {
                            var start = _charReader.Position;
                            ReadSinglelineComment();
                            AddTrivia(target, start, SyntaxKind.SingleLineCommentTrivia);
                        }
                        else if (_charReader.Peek() == '*')
                        {
                            var start = _charReader.Position;
                            ReadMultilineComment();
                            AddTrivia(target, start, SyntaxKind.MultiLineCommentTrivia);
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        if (char.IsWhiteSpace(_charReader.Current))
                        {
                            var start = _charReader.Position;
                            ReadWhitespace();
                            AddTrivia(target, start, SyntaxKind.WhitespaceTrivia);
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
                        // TODO:
                        //_errorReporter.UnterminatedComment(_tokenRange.StartLocation, _source.Substring(_tokenStart, _reader.Pos - _tokenStart));
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

        private void AddTrivia(List<SyntaxTrivia> target, int start, SyntaxKind kind)
        {
            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = GetText(span);
            var trivia = new SyntaxTrivia(kind, text, span, null);
            target.Add(trivia);
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
                    _kind = SyntaxKind.BitwiseAndToken;
                    _charReader.NextChar();
                    break;

                case '|':
                    _kind = SyntaxKind.BitwiseOrToken;
                    _charReader.NextChar();
                    break;

                case '^':
                    _kind = SyntaxKind.BitwiseXorToken;
                    _charReader.NextChar();
                    break;

                case '(':
                    _kind = SyntaxKind.LeftParenthesesToken;
                    _charReader.NextChar();
                    break;

                case ')':
                    _kind = SyntaxKind.RightParenthesesToken;
                    _charReader.NextChar();
                    break;

                case '.':
                    if (Char.IsDigit(_charReader.Peek()))
                        ReadNumber();
                    else
                    {
                        _kind = SyntaxKind.DotToken;
                        _charReader.NextChar();
                    }
                    break;

                case '@':
                    _kind = SyntaxKind.ParameterMarkerToken;
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
                        _kind = SyntaxKind.MultiplyToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.PowerToken;
                        _charReader.NextChar();
                    }
                    break;

                case '/':
                    _kind = SyntaxKind.DivideToken;
                    _charReader.NextChar();
                    break;

                case '%':
                    _kind = SyntaxKind.ModulusToken;
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
                    _charReader.NextChar();
                    if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.UnequalsToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '>')
                    {
                        _kind = SyntaxKind.NotGreaterToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '<')
                    {
                        _kind = SyntaxKind.NotLessToken;
                        _charReader.NextChar();
                    }
                    else
                    {
                        // TODO: Report error
                        //_errorReporter.IllegalInputCharacter(_charReader.Location, _charReader.Current);
                    }
                    break;

                case '<':
                    _charReader.NextChar();
                    if (_charReader.Current == '<')
                    {
                        _kind = SyntaxKind.LeftShiftToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '>')
                    {
                        _kind = SyntaxKind.UnequalsToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.LessOrEqualToken;
                        _charReader.NextChar();
                    }
                    else
                        _kind = SyntaxKind.LessToken;
                    break;

                case '>':
                    _charReader.NextChar();
                    if (_charReader.Current == '>')
                    {
                        _kind = SyntaxKind.RightShiftToken;
                        _charReader.NextChar();
                    }
                    else if (_charReader.Current == '=')
                    {
                        _kind = SyntaxKind.GreaterOrEqualToken;
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
                    if (Char.IsLetter(_charReader.Current) || _charReader.Current == '_')
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (Char.IsDigit(_charReader.Current))
                    {
                        ReadNumber();
                    }
                    else
                    {
                        // TODO: Report error
                        //_errorReporter.IllegalInputCharacter(_charReader.Location, _charReader.Current);
                        _charReader.NextChar();
                    }

                    break;
            }
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
                        // TODO
                        // _errorReporter.UnterminatedString(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
                        goto ExitLoop;

                    case '\'':
                        sb.Append(_charReader.Current);
                        _charReader.NextChar();

                        if (_charReader.Current != '\'')
                            goto ExitLoop;
                        
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
                        // TODO
                        //_errorReporter.UnterminatedDate(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
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
            {
                // TODO: Report error
                //_errorReporter.InvalidDate(tokenRange, textWithoutDelimiters)
            }

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
                        if (!Char.IsLetterOrDigit(_charReader.Current))
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

        private static double ReadDouble(string text)
        {
            try
            {
                return double.Parse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                // TODO:
                //_errorReporter.NumberTooLarge(tokenRange, number);
            }
            catch (FormatException)
            {
                // TODO:
                //_errorReporter.InvalidReal(tokenRange, number);
            }
            return 0.0;
        }

        private static object ReadInt32OrInt64(string text)
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

        private static long ReadInt64(string text)
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
                        //TODO:
                        //_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
                    }
                    catch (FormatException)
                    {
                        //TODO:
                        //_errorReporter.InvalidHex(tokenRange, textWithoutIndicator);
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
                        // TODO:
                        //_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
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
                        // TODO:
                        //_errorReporter.NumberTooLarge(tokenRange, textWithoutIndicator);
                    }

                    return 0;

                default:
                    try
                    {
                        return Int64.Parse(text, CultureInfo.InvariantCulture);
                    }
                    catch (OverflowException)
                    {
                        // TODO:
                        //_errorReporter.NumberTooLarge(tokenRange, text);
                    }
                    catch (FormatException)
                    {
                        // TODO:
                        //_errorReporter.InvalidInteger(tokenRange, text);
                    }

                    return 0;
            }
        }

        private static long ReadBinaryValue(string binary)
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
                    // TODO
                    //_errorReporter.InvalidBinary(tokenRange, binary);
                    return 0;
                }
            }

            return val;
        }

        private static long ReadOctalValue(string octal)
        {
            long val = 0;

            for (int i = octal.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int c;

                try
                {
                    c = Int32.Parse(new string(octal[i], 1), CultureInfo.InvariantCulture);

                    if (c > 7)
                    {
                        // TODO:
                        //_errorReporter.InvalidOctal(tokenRange, octal);
                        return 0;
                    }
                }
                catch (FormatException)
                {
                    // TODO:
                    //_errorReporter.InvalidOctal(tokenRange, octal);
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

            while (Char.IsLetterOrDigit(_charReader.Current) ||
                   _charReader.Current == '_' ||
                   _charReader.Current == '$')
            {
                _charReader.NextChar();
            }

            var end = _charReader.Position;
            var span = TextSpan.FromBounds(start, end);
            var text = GetText(span);

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
                        // TODO
                        // _errorReporter.UnterminatedQuotedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
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
                        // TODO:
                        //_errorReporter.UnterminatedParenthesizedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
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

        private string GetText(TextSpan span)
        {
            return _source.Substring(span.Start, span.Length);
        }
    }
}