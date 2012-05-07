using System;
using System.Collections.Generic;

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

            var start = _charReader.Position;
            ReadToken();
            var end = _charReader.Position;
            var kind = _kind;
            var span = TextSpan.FromBounds(start, end);
            var text = GetText(span);

            // TODO: Get errors

            _trailingTrivia.Clear();
            ReadTrivia(_trailingTrivia, isLeading: true);
            var trailingTrivia = _trailingTrivia.ToArray();

            return new SyntaxToken(kind, _contextualKind, false, span, text, leadingTrivia, trailingTrivia);
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
            var trivia = new SyntaxTrivia(kind, text, span);
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

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                        // TODO
                        // _errorReporter.UnterminatedString(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
                        return;

                    case '\'':
                        _charReader.NextChar();

                        if (_charReader.Current != '\'')
                            return;
                        
                        _charReader.NextChar();
                        break;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadDate()
        {
            _kind = SyntaxKind.DateLiteralToken;

            // Skip initial #
            _charReader.NextChar();

            // Just read everything that looks like it could be a date
            // date are verified by the parser

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        // TODO
                        //_errorReporter.UnterminatedDate(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
                        return;

                    case '#':
                        _charReader.NextChar();
                        return;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadNumber()
        {
            _kind = SyntaxKind.NumericLiteralToken;

            // Just read everything that looks like it could be a number
            // numbers are verified by the parser

            while (true)
            {
                switch (_charReader.Current)
                {
                        // dot
                    case '.':
                        break;

                        // special handling for e, it could be the exponent indicator 
                        // followed by an optional sign

                    case 'E':
                    case 'e':
                        if (_charReader.Peek() == '-' || _charReader.Peek() == '+')
                            _charReader.NextChar();
                        break;

                    default:
                        if (!Char.IsLetterOrDigit(_charReader.Current))
                            return;
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadIdentifierOrKeyword()
        {
            // TODO: We shouldn't extract the text multiple times.

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
        }

        private void ReadQuotedIdentifier()
        {
            _kind = SyntaxKind.IdentifierToken;
            
            // Skip initial quote
            _charReader.NextChar();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        // TODO
                        // _errorReporter.UnterminatedQuotedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
                        return;

                    case '"':
                        _charReader.NextChar();

                        if (_charReader.Current != '"')
                            return;

                        _charReader.NextChar();
                        break;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private void ReadParenthesizedIdentifier()
        {
            _kind = SyntaxKind.IdentifierToken;

            // Skip initial [
            _charReader.NextChar();

            while (true)
            {
                switch (_charReader.Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        // TODO:
                        //_errorReporter.UnterminatedParenthesizedIdentifier(_tokenRange.StartLocation, _source.Substring(_tokenStart + 1, _reader.Pos - _tokenStart));
                        return;

                    case ']':
                        _charReader.NextChar();

                        if (_charReader.Current != ']')
                            return;
                        
                        _charReader.NextChar();
                        break;

                    default:
                        _charReader.NextChar();
                        break;
                }
            }
        }

        private string GetText(TextSpan span)
        {
            return _source.Substring(span.Start, span.Length);
        }
    }
}