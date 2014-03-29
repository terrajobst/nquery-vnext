using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Syntax
{
    internal sealed class Parser
    {
        private readonly TextBuffer _textBuffer;
        private readonly SyntaxTree _syntaxTree;
        private readonly List<SyntaxToken> _tokens = new List<SyntaxToken>();
        private int _tokenIndex;

        public Parser(TextBuffer textBuffer, SyntaxTree syntaxTree)
        {
            _textBuffer = textBuffer;
            _syntaxTree = syntaxTree;
            LexAllTokens(textBuffer);
        }

        private SyntaxToken Current
        {
            get { return Peek(0); }
        }

        private SyntaxToken Lookahead
        {
            get { return Peek(1); }
        }

        private void LexAllTokens(TextBuffer textBuffer)
        {
            var lexer = new Lexer(_syntaxTree, textBuffer);
            var badTokens = new List<SyntaxToken>();

            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                // Skip any bad tokens.

                badTokens.Clear();
                while (token.Kind == SyntaxKind.BadToken)
                {
                    badTokens.Add(token);
                    token = lexer.Lex();
                }

                if (badTokens.Count > 0)
                {
                    var trivia = ImmutableArray.Create(CreateSkippedTokensTrivia(badTokens))
                                 .Concat(token.LeadingTrivia).ToImmutableArray();
                    token = token.WithLeadingTrivia(trivia);
                }

                _tokens.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);
        }

        private SyntaxToken Peek(int offset)
        {
            var i = Math.Min(_tokenIndex + offset, _tokens.Count - 1);
            return _tokens[i];
        }

        private SyntaxToken GetPreviousToken()
        {
            var previousIndex = _tokenIndex - 1;
            return previousIndex < 0
                ? null
                : _tokens[previousIndex];
        }

        private SyntaxToken NextToken()
        {
            var result = Current;
            _tokenIndex = Math.Min(_tokenIndex + 1, _tokens.Count - 1);
            return result;
        }

        private SyntaxToken NextTokenAsKeyword()
        {
            var result = NextToken();
            return result.Kind.IsKeyword()
                       ? result
                       : result.WithKind(result.ContextualKind);
        }

        private SyntaxToken NextTokenIf(SyntaxKind kind)
        {
            return Current.Kind == kind
                       ? NextToken()
                       : null;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            // Normally, our behavior is to insert missing tokens if the current token doesn't
            // match our expected token. The root of the parser will make sure that after we've
            // completed parsing the main element (i.e. query or expression) the only remaining
            // token is EOF.
            //
            // If there are any tokens left, we report an error saying "we expected EOF but
            // found <token>". That EOF token will contain all remaining tokens as skipped
            // token trivia.
            //
            // However, there are cases where we expect an identifier but the currently typed
            // identifier is a keyword.
            //
            // For example, consider this case:
            //
            //     SELECT  e.FirstName + ' ' + e.LastName AS Full|
            //     FROM    Employees e
            //
            // The developer is typing "FullName" but as soon as he completes the second
            // "l" the syntax is completely messed up.
            //
            // Here is another example:
            //
            //     SELECT   *
            //     FROM     Or|
            //
            // In this case, the develope wants to type "Orders" but as soon as the "r" is
            // typed the completion will stop working because the context of the "Or"
            // keyword is no longer part of the table reference but part of the skipped
            // token trivia contained in the EOF token.
            //
            // Both cases can be fixed by skipping the current token and inserting a missing
            // token that contains the skipped token as trivia.
            //
            // You may wonder why we shouldn't do this for all cases where an identifier is
            // expeted but a keyword is found.
            //
            // Consider this case:
            //
            //     SELECT   o.|
            //     FROM     Orders
            //
            // If we were always eating the curent token, we would skip the FROM token which
            // would cause the completion provider to think we've typed "o.FROM|" which is not
            // what we want. So in essence we want to disambiguate this case:
            //
            //     SELECT   o.|
            //     FROM     Orders
            //
            // from:
            //
            //     SELECT   o.From|
            //
            // The easiest is to use the line information -- if the current token is on the
            // same line as the previous, then we skip (second case). Otherwise we insert
            // a new token (frist case).

            if (kind == SyntaxKind.IdentifierToken && Current.Kind.IsKeyword() && IsPreviousTokenOnSameLine())
                return SkipAndInsertMissingToken(SyntaxKind.IdentifierToken);

            return InsertMissingToken(kind);
        }

        private bool IsPreviousTokenOnSameLine()
        {
            var previous = GetPreviousToken();
            if (previous == null)
                return true;

            var previousLine = _textBuffer.GetLineNumberFromPosition(previous.Span.Start);
            var currentLine = _textBuffer.GetLineNumberFromPosition(Current.Span.Start);
            return previousLine == currentLine;
        }

        private void SkipTokens(Func<SyntaxToken, bool> stopPredicate)
        {
            if (stopPredicate(Current))
                return;

            var tokens = new List<SyntaxToken>();
            do
            {
                tokens.Add(NextToken());
            } while (!stopPredicate(Current));

            var current = _tokens[_tokenIndex];
            var skippedTokensTrivia = CreateSkippedTokensTrivia(tokens);

            var leadingTrivia = new List<SyntaxTrivia>(current.LeadingTrivia.Length + 1);
            leadingTrivia.Add(skippedTokensTrivia);
            leadingTrivia.AddRange(current.LeadingTrivia);

            _tokens[_tokenIndex] = current.WithLeadingTrivia(leadingTrivia);
        }

        private SyntaxToken InsertMissingToken(SyntaxKind kind)
        {
            var missingTokenSpan = new TextSpan(Current.FullSpan.Start, 0);
            var diagnosticSpan = GetDiagnosticSpanForMissingToken();
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(diagnosticSpan, Current, kind);
            return new SyntaxToken(_syntaxTree, kind, SyntaxKind.BadToken, true, missingTokenSpan, string.Empty, null, new SyntaxTrivia[0], new SyntaxTrivia[0], diagnostics);
        }

        private SyntaxToken SkipAndInsertMissingToken(SyntaxKind kind)
        {
            var skippedToken = Current;
            var skippedTokensTrivia = new[] { CreateSkippedTokensTrivia(new[] { skippedToken }) };
            NextToken();

            var missingTokenSpan = new TextSpan(Current.FullSpan.Start, 0);
            var diagnosticSpan = GetDiagnosticSpanForMissingToken();
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(diagnosticSpan, skippedToken, kind);
            return new SyntaxToken(_syntaxTree, kind, SyntaxKind.BadToken, true, missingTokenSpan, string.Empty, null, skippedTokensTrivia, new SyntaxTrivia[0], diagnostics);
        }

        private TextSpan GetDiagnosticSpanForMissingToken()
        {
            if (_tokenIndex > 0)
            {
                var previousToken = _tokens[_tokenIndex - 1];
                var previousTokenLine = _textBuffer.GetLineFromPosition(previousToken.Span.End);
                var currentTokenLine = _textBuffer.GetLineFromPosition(Current.Span.Start);
                if (currentTokenLine != previousTokenLine)
                    return new TextSpan(previousToken.Span.End, 2);
            }

            return Current.Span;
        }

        private SyntaxTrivia CreateSkippedTokensTrivia(IReadOnlyCollection<SyntaxToken> tokens)
        {
            var start = tokens.First().FullSpan.Start;
            var end = tokens.Last().FullSpan.End;
            var span = TextSpan.FromBounds(start, end);
            var structure = new SkippedTokensTriviaSyntax(_syntaxTree, tokens);
            return new SyntaxTrivia(_syntaxTree, SyntaxKind.SkippedTokensTrivia, null, span, structure, new Diagnostic[0]);
        }

        private bool CurrentIsStartingQuery()
        {
            // Current token indicates the start of a query if we see
            // a SELECT keywordword preceeded by any number of open
            // parentheses.

            var peek = 0;
            while (Peek(peek).Kind == SyntaxKind.LeftParenthesisToken)
            {
                peek++;
            }

            return Peek(peek).Kind == SyntaxKind.SelectKeyword;
        }

        public CompilationUnitSyntax ParseRootQuery()
        {
            var query = ParseQueryWithOptionalCte();
            var endOfFileToken = ParseEndOfFileToken();
            return new CompilationUnitSyntax(_syntaxTree, query, endOfFileToken);
        }

        public CompilationUnitSyntax ParseRootExpression()
        {
            var expression = ParseExpression();
            var endOfFileToken = ParseEndOfFileToken();
            return new CompilationUnitSyntax(_syntaxTree, expression, endOfFileToken);
        }

        private SyntaxToken ParseEndOfFileToken()
        {
            if (Current.Kind == SyntaxKind.EndOfFileToken)
                return NextToken();

            var firstUnexpecteToken = Current;
            SkipTokens(t => t.Kind == SyntaxKind.EndOfFileToken);

            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(firstUnexpecteToken.Span, firstUnexpecteToken, SyntaxKind.EndOfFileToken);
            return endOfFileToken.WithDiagnotics(diagnostics);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseSubExpression(null, 0);
        }

        private ExpressionSyntax ParseSubExpression(ExpressionSyntax left, int precedence)
        {
            if (left == null)
            {
                // No left operand, so we parse one and take care of leading unary operators

                var unaryExpression = SyntaxFacts.GetUnaryOperatorExpression(Current.Kind);
                left = unaryExpression == SyntaxKind.BadToken
                           ? ParseSimpleExpression()
                           : ParseUnaryExpression(unaryExpression);
            }

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                // Special handling for NOT BETWEEN, NOT IN, NOT LIKE, NOT SIMILAR TO, and NOT SOUND SLIKE.

                var notKeyword = Current.Kind == SyntaxKind.NotKeyword && Lookahead.Kind.CanHaveLeadingNot()
                                     ? NextToken()
                                     : null;

                // Special handling for the only ternary operator BETWEEN

                if (Current.Kind == SyntaxKind.BetweenKeyword)
                {
                    left = ParseBetweenExpression(left, notKeyword);
                }
                else
                {
                    // If there is no binary operator we are finished

                    var binaryExpression = SyntaxFacts.GetBinaryOperatorExpression(Current.Kind);
                    if (binaryExpression == SyntaxKind.BadToken)
                        return left;

                    var operatorPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression);

                    // Precedence is lower or equal, parse it later

                    if (operatorPrecedence <= precedence)
                        return left;

                    // Precedence is higher

                    left = ParseBinaryExpression(left, notKeyword, binaryExpression, operatorPrecedence);
                }
            }

            return left;
        }

        private ExpressionSyntax ParseUnaryExpression(SyntaxKind unaryExpression)
        {
            var operatorToken = NextToken();
            var operatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryExpression);
            var expression = ParseSubExpression(null, operatorPrecedence);
            return new UnaryExpressionSyntax(_syntaxTree, operatorToken, expression);
        }

        private ExpressionSyntax ParseBinaryExpression(ExpressionSyntax left, SyntaxToken notKeyword, SyntaxKind binaryExpression, int operatorPrecedence)
        {
            var operatorToken = NextToken();

            switch (operatorToken.Kind)
            {
                case SyntaxKind.SimilarKeyword:
                    return ParseSimilarToExpression(left, notKeyword, operatorPrecedence, operatorToken);
                case SyntaxKind.SoundsKeyword:
                    return ParseSoundslikeExpression(left, notKeyword, operatorPrecedence, operatorToken);
                case SyntaxKind.LikeKeyword:
                    return ParseLikeExpression(left, notKeyword, operatorPrecedence, operatorToken);
                case SyntaxKind.InKeyword:
                    return ParseInExpression(left, notKeyword, operatorToken);
                default:
                    var isAllAny = Current.Kind == SyntaxKind.AllKeyword ||
                                   Current.Kind == SyntaxKind.AnyKeyword ||
                                   Current.Kind == SyntaxKind.SomeKeyword;

                    return isAllAny
                               ? ParseAllAnySubselect(left, binaryExpression, operatorToken)
                               : ParseBinaryExpression(left, operatorPrecedence, operatorToken);
            }
        }

        private ExpressionSyntax ParseBinaryExpression(ExpressionSyntax left, int operatorPrecedence, SyntaxToken operatorToken)
        {
            var expression = ParseSubExpression(null, operatorPrecedence);
            return new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, expression);
        }

        private ExpressionSyntax ParseBetweenExpression(ExpressionSyntax left, SyntaxToken notKeyword)
        {
            var betweenPrecedence = SyntaxFacts.GetTernaryOperatorPrecedence(SyntaxKind.BetweenExpression);
            var betweenKeyword = NextToken();
            var lowerBound = ParseSubExpression(null, betweenPrecedence);
            var andKeyword = Match(SyntaxKind.AndKeyword);
            var upperBound = ParseSubExpression(null, betweenPrecedence);
            return new BetweenExpressionSyntax(_syntaxTree, left, notKeyword, betweenKeyword, lowerBound, andKeyword, upperBound);
        }

        private ExpressionSyntax ParseSimilarToExpression(ExpressionSyntax left, SyntaxToken notKeyword, int operatorPrecedence, SyntaxToken operatorToken)
        {
            var toKeyword = Match(SyntaxKind.ToKeyword);
            var expression = ParseSubExpression(null, operatorPrecedence);
            return new SimilarToExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, toKeyword, expression);
        }

        private ExpressionSyntax ParseSoundslikeExpression(ExpressionSyntax left, SyntaxToken notKeyword, int operatorPrecedence, SyntaxToken soundsKeywordToken)
        {
            var likeKeyword = Match(SyntaxKind.LikeKeyword);
            var expression = ParseSubExpression(null, operatorPrecedence);
            return new SoundslikeExpressionSyntax(_syntaxTree, left, notKeyword, soundsKeywordToken, likeKeyword, expression);
        }

        private ExpressionSyntax ParseLikeExpression(ExpressionSyntax left, SyntaxToken notKeyword, int operatorPrecedence, SyntaxToken operatorToken)
        {
            var expression = ParseSubExpression(null, operatorPrecedence);
            return new LikeExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, expression);
        }

        private ExpressionSyntax ParseInExpression(ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken operatorToken)
        {
            var isQuery = CurrentIsStartingQuery();
            if (!isQuery)
            {
                var argumentList = ParseArgumentList(1);
                return new InExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, argumentList);
            }
            
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var querySyntax = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new InQueryExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, leftParenthesis, querySyntax, rightParenthesis);
        }

        private ExpressionSyntax ParseAllAnySubselect(ExpressionSyntax left, SyntaxKind binaryExpression, SyntaxToken operatorToken)
        {
            var allAnyOperatorToken = binaryExpression.IsValidAllAnyOperator()
                                          ? operatorToken
                                          : operatorToken.WithInvalidOperatorForAllAnyDiagnostics();
            var keyword = NextToken();
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var query = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new AllAnySubselectSyntax(_syntaxTree, left, allAnyOperatorToken, keyword, leftParenthesis, query, rightParenthesis);
        }

        private ExpressionSyntax ParseSimpleExpression()
        {
            return ParseIsNullExpression();
        }

        private ExpressionSyntax ParseIsNullExpression()
        {
            var expression = ParseMemberExpression();

            if (Current.Kind == SyntaxKind.IsKeyword)
            {
                var isToken = NextToken();
                var notToken = NextTokenIf(SyntaxKind.NotKeyword);
                var nullToken = Match(SyntaxKind.NullKeyword);

                return new IsNullExpressionSyntax(_syntaxTree, expression, isToken, notToken, nullToken);
            }

            return expression;
        }

        private ExpressionSyntax ParseMemberExpression()
        {
            var target = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.DotToken)
            {
                var dot = NextToken();

                var name = Match(SyntaxKind.IdentifierToken);

                if (Current.Kind != SyntaxKind.LeftParenthesisToken)
                {
                    target = new PropertyAccessExpressionSyntax(_syntaxTree, target, dot, name);
                }
                else
                {
                    var argumentList = ParseArgumentList();
                    target = new MethodInvocationExpressionSyntax(_syntaxTree, target, dot, name, argumentList);
                }
            }

            return target;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.NullKeyword:
                    return ParseNullLiteral();

                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();

                case SyntaxKind.DateLiteralToken:
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.StringLiteralToken:
                    return ParseLiteral();

                case SyntaxKind.ExistsKeyword:
                    return ParseExistsSubselect();

                case SyntaxKind.AtToken:
                    return ParseVariableExpression();

                case SyntaxKind.CastKeyword:
                    return ParseCastExpression();

                case SyntaxKind.CaseKeyword:
                    return ParseCaseExpression();

                case SyntaxKind.CoalesceKeyword:
                    return ParseCoalesceExpression();

                case SyntaxKind.NullIfKeyword:
                    return ParseNullIfExpression();

                case SyntaxKind.IdentifierToken:
                    return ParseNameOrFunctionInvocationExpression();

                case SyntaxKind.LeftParenthesisToken:
                    return ParseSubqueryOrParenthesizedExpression();

                default:
                    return ParseNameExpression();
            }
        }

        private ExpressionSyntax ParseNullLiteral()
        {
            return new LiteralExpressionSyntax(_syntaxTree, NextToken(), null);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var token = NextToken();
            var value = token.Kind == SyntaxKind.TrueKeyword;
            return new LiteralExpressionSyntax(_syntaxTree, token, value);
        }

        private ExpressionSyntax ParseLiteral()
        {
            var token = NextToken();
            return new LiteralExpressionSyntax(_syntaxTree, token, token.Value);
        }

        private ExpressionSyntax ParseExistsSubselect()
        {
            var existsKeyword = NextToken();
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var query = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new ExistsSubselectSyntax(_syntaxTree, existsKeyword, leftParenthesis, query, rightParenthesis);
        }

        private ExpressionSyntax ParseVariableExpression()
        {
            var atToken = NextToken();
            var name = Match(SyntaxKind.IdentifierToken);
            return new VariableExpressionSyntax(_syntaxTree, atToken, name);
        }

        private ExpressionSyntax ParseCastExpression()
        {
            var castKeyword = NextToken();
            var leftParenthesisToken = Match(SyntaxKind.LeftParenthesisToken);
            var expression = ParseExpression();
            var asKeyword = Match(SyntaxKind.AsKeyword);
            var typeName = Match(SyntaxKind.IdentifierToken);
            var rightParenthesisToken = Match(SyntaxKind.RightParenthesisToken);
            return new CastExpressionSyntax(_syntaxTree, castKeyword, leftParenthesisToken, expression, asKeyword, typeName, rightParenthesisToken);
        }

        private ExpressionSyntax ParseCaseExpression()
        {
            var caseKeyword = Match(SyntaxKind.CaseKeyword);
            var inputExpression = ParseOptionalCaseInputExpression();
            var caseLabels = ParseCaseLabels();
            var elseLabel = ParseOptionalCaseElseLabel();
            var endKeyword = Match(SyntaxKind.EndKeyword);
            return new CaseExpressionSyntax(_syntaxTree, caseKeyword, inputExpression, caseLabels, elseLabel, endKeyword);
        }

        private ExpressionSyntax ParseOptionalCaseInputExpression()
        {
            var hasInput = Current.Kind != SyntaxKind.WhenKeyword &&
                           Current.Kind != SyntaxKind.ElseKeyword &&
                           Current.Kind != SyntaxKind.EndKeyword;

            var inputExpression = hasInput ? ParseExpression() : null;
            return inputExpression;
        }

        private List<CaseLabelSyntax> ParseCaseLabels()
        {
            var caseLabels = new List<CaseLabelSyntax>();
            do
            {
                var caseLabel = ParseCaseLabel();
                caseLabels.Add(caseLabel);
            } while (Current.Kind == SyntaxKind.WhenKeyword);
            return caseLabels;
        }

        private CaseLabelSyntax ParseCaseLabel()
        {
            var whenKeyword = Match(SyntaxKind.WhenKeyword);
            var whenExpression = ParseExpression();
            var thenKeyword = Match(SyntaxKind.ThenKeyword);
            var thenExpression = ParseExpression();
            return new CaseLabelSyntax(_syntaxTree, whenKeyword, whenExpression, thenKeyword, thenExpression);
        }

        private CaseElseLabelSyntax ParseOptionalCaseElseLabel()
        {
            return Current.Kind != SyntaxKind.ElseKeyword
                       ? null
                       : ParseCaseElseLabel();
        }

        private CaseElseLabelSyntax ParseCaseElseLabel()
        {
            var elseKeyword = Match(SyntaxKind.ElseKeyword);
            var elseExpression = ParseExpression();
            return new CaseElseLabelSyntax(_syntaxTree, elseKeyword, elseExpression);
        }

        private ExpressionSyntax ParseCoalesceExpression()
        {
            var coalesceKeyword = NextToken();
            var arguments = ParseArgumentList(2);
            return new CoalesceExpressionSyntax(_syntaxTree, coalesceKeyword, arguments);
        }

        private ExpressionSyntax ParseNullIfExpression()
        {
            var nullIfKeyword = NextToken();
            var leftParenthesisToken = Match(SyntaxKind.LeftParenthesisToken);
            var leftExpression = ParseExpression();
            var commaToken = Match(SyntaxKind.CommaToken);
            var rightExpression = ParseExpression();
            var rightParenthesisToken = Match(SyntaxKind.RightParenthesisToken);
            return new NullIfExpressionSyntax(_syntaxTree, nullIfKeyword, leftParenthesisToken, leftExpression, commaToken, rightExpression, rightParenthesisToken);
        }

        private ExpressionSyntax ParseNameOrFunctionInvocationExpression()
        {
            var isFunctionInvocation = Current.Kind == SyntaxKind.IdentifierToken &&
                                       Lookahead.Kind == SyntaxKind.LeftParenthesisToken;

            return !isFunctionInvocation
                       ? ParseNameExpression()
                       : ParseFunctionInvocationExpression();
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var name = Match(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(_syntaxTree, name);
        }

        private ExpressionSyntax ParseFunctionInvocationExpression()
        {
            var name = Match(SyntaxKind.IdentifierToken);
            if (Lookahead.Kind == SyntaxKind.AsteriskToken && name.Matches("COUNT"))
            {
                var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
                var asteriskToken = Match(SyntaxKind.AsteriskToken);
                var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                return new CountAllExpressionSyntax(_syntaxTree, name, leftParenthesis, asteriskToken, rightParenthesis);
            }

            var arguments = ParseArgumentList();
            return new FunctionInvocationExpressionSyntax(_syntaxTree, name, arguments);
        }

        private ExpressionSyntax ParseSubqueryOrParenthesizedExpression()
        {
            return Lookahead.Kind == SyntaxKind.SelectKeyword
                       ? ParseSingleRowSubselect()
                       : ParseParenthesizedExpression();
        }

        private ExpressionSyntax ParseSingleRowSubselect()
        {
            var leftParenthesis = NextToken();
            var query = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new SingleRowSubselectSyntax(_syntaxTree, leftParenthesis, query, rightParenthesis);
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var leftParenthesis = NextToken();
            var expression = ParseExpression();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new ParenthesizedExpressionSyntax(_syntaxTree, leftParenthesis, expression, rightParenthesis);
        }

        private ArgumentListSyntax ParseArgumentList(int minimumNumberOfArguments = 0)
        {
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var expressionList = ParseArgumentExpressionList(minimumNumberOfArguments);
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new ArgumentListSyntax(_syntaxTree, leftParenthesis, expressionList, rightParenthesis);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArgumentExpressionList(int minimumNumberOfArguments)
        {
            if (Current.Kind == SyntaxKind.RightParenthesisToken && minimumNumberOfArguments == 0)
                return SeparatedSyntaxList<ExpressionSyntax>.Empty;

            var expressionsWithCommas = new List<SyntaxNodeOrToken>();
            var minimumNumberOfItems = minimumNumberOfArguments * 2 - 1;

            while (true)
            {
                var oldIndex = _tokenIndex;

                var argument = ParseExpression();
                expressionsWithCommas.Add(argument);

                if (_tokenIndex == oldIndex)
                    SkipTokens(t => SyntaxFacts.CanFollowArgument(t.Kind));

                var missingComma = Current.Kind != SyntaxKind.CommaToken;
                var enoughParameters = minimumNumberOfItems <= expressionsWithCommas.Count;
                var canStartExpression = SyntaxFacts.CanStartExpression(Current.Kind);

                if (missingComma && enoughParameters && !canStartExpression)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                expressionsWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(expressionsWithCommas);
        }

        private QuerySyntax ParseQueryWithOptionalCte()
        {
            if (Current.Kind != SyntaxKind.WithKeyword)
                return ParseQuery();

            var withKeyword = Match(SyntaxKind.WithKeyword);
            var commonTableExpressionList = ParseCommonTableExpressionList();
            var query = ParseQuery();
            return new CommonTableExpressionQuerySyntax(_syntaxTree, withKeyword, commonTableExpressionList, query);
        }

        private SeparatedSyntaxList<CommonTableExpressionSyntax> ParseCommonTableExpressionList()
        {
            var commonTableExpressionsWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var commonTableExpression = ParseCommonTableExpression();
                commonTableExpressionsWithCommas.Add(commonTableExpression);

                if (Current.Kind != SyntaxKind.CommaToken)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                commonTableExpressionsWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<CommonTableExpressionSyntax>(commonTableExpressionsWithCommas);
        }

        private CommonTableExpressionSyntax ParseCommonTableExpression()
        {
            var identifer = Match(SyntaxKind.IdentifierToken);
            var commonTableExpressionColumnNameList = ParseCommonTableExpressionColumnNameList();
            var asKeyword = Match(SyntaxKind.AsKeyword);
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var query = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new CommonTableExpressionSyntax(_syntaxTree, identifer, commonTableExpressionColumnNameList, asKeyword, leftParenthesis, query, rightParenthesis);
        }

        private CommonTableExpressionColumnNameListSyntax ParseCommonTableExpressionColumnNameList()
        {
            if (Current.Kind != SyntaxKind.LeftParenthesisToken)
                return null;

            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var columnNameList = ParseCommonTableExpressionColumnNames();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new CommonTableExpressionColumnNameListSyntax(_syntaxTree, leftParenthesis, columnNameList, rightParenthesis);
        }

        private SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> ParseCommonTableExpressionColumnNames()
        {
            var namesAndCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var identifier = Match(SyntaxKind.IdentifierToken);
                var columnName = new CommonTableExpressionColumnNameSyntax(_syntaxTree, identifier);
                namesAndCommas.Add(columnName);

                if (Current.Kind != SyntaxKind.CommaToken)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                namesAndCommas.Add(comma);
            }

            return new SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax>(namesAndCommas);
        }

        private QuerySyntax ParseQuery()
        {
            var query = ParseUnifiedOrExceptionalQuery();

            // ORDER BY

            if (Current.Kind != SyntaxKind.OrderKeyword)
                return query;

            var orderKeyword = Match(SyntaxKind.OrderKeyword);
            var byKeyword = Match(SyntaxKind.ByKeyword);
            var columnList = ParseOrderByColumnList();
            return new OrderedQuerySyntax(_syntaxTree, query, orderKeyword, byKeyword, columnList);
        }

        private SeparatedSyntaxList<OrderByColumnSyntax> ParseOrderByColumnList()
        {
            var columnsWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var column = ParseOrderByColumn();
                columnsWithCommas.Add(column);

                if (Current.Kind != SyntaxKind.CommaToken)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                columnsWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<OrderByColumnSyntax>(columnsWithCommas);
        }

        private OrderByColumnSyntax ParseOrderByColumn()
        {
            var expression = ParseExpression();
            SyntaxToken modifier;

            if (Current.Kind == SyntaxKind.AscKeyword)
            {
                modifier = NextToken();
            }
            else if (Current.Kind == SyntaxKind.DescKeyword)
            {
                modifier = NextToken();
            }
            else
            {
                modifier = null;
            }

            var column = new OrderByColumnSyntax(_syntaxTree, expression, modifier);
            return column;
        }

        private QuerySyntax ParseUnifiedOrExceptionalQuery()
        {
            var leftQuery = ParseIntersectionalQuery();

            if (leftQuery == null)
                return null;

            while (Current.Kind == SyntaxKind.UnionKeyword ||
                   Current.Kind == SyntaxKind.ExceptKeyword)
            {
                if (Current.Kind == SyntaxKind.UnionKeyword)
                {
                    var unionKeyword = Match(SyntaxKind.UnionKeyword);
                    var allKeyword = NextTokenIf(SyntaxKind.AllKeyword);
                    var rightQuery = ParseIntersectionalQuery();
                    leftQuery = new UnionQuerySyntax(_syntaxTree, leftQuery, unionKeyword, allKeyword, rightQuery);
                }
                else
                {
                    var exceptKeyword = Match(SyntaxKind.ExceptKeyword);
                    var rightQuery = ParseIntersectionalQuery();
                    leftQuery = new ExceptQuerySyntax(_syntaxTree, leftQuery, exceptKeyword, rightQuery);
                }
            }

            return leftQuery;
        }

        private QuerySyntax ParseIntersectionalQuery()
        {
            var leftQuery = ParseSelectQuery();

            if (leftQuery == null)
                return null;

            while (Current.Kind == SyntaxKind.IntersectKeyword)
            {
                var intersectKeyword = Match(SyntaxKind.IntersectKeyword);
                var rightQuery = ParseSelectQuery();

                if (rightQuery == null)
                    return null;

                leftQuery = new IntersectQuerySyntax(_syntaxTree, leftQuery, intersectKeyword, rightQuery);
            }

            return leftQuery;
        }

        private QuerySyntax ParseSelectQuery()
        {
            if (Current.Kind == SyntaxKind.LeftParenthesisToken)
            {
                var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
                var query = ParseQuery();
                var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                return new ParenthesizedQuerySyntax(_syntaxTree, leftParenthesis, query, rightParenthesis);
            }

            var selectClause = ParseSelectClause();

            var fromClause = Current.Kind == SyntaxKind.FromKeyword
                                 ? ParseFromClause()
                                 : null;

            var whereClause = Current.Kind == SyntaxKind.WhereKeyword
                                  ? ParseWhereClause()
                                  : null;

            var groupByClause = Current.Kind == SyntaxKind.GroupKeyword
                                    ? ParseGroupByClause()
                                    : null;

            var havingClause = Current.Kind == SyntaxKind.HavingKeyword
                                   ? ParseHavingClause()
                                   : null;

            return new SelectQuerySyntax(_syntaxTree, selectClause, fromClause, whereClause, groupByClause, havingClause);
        }

        private SelectClauseSyntax ParseSelectClause()
        {
            var selectKeyword = Match(SyntaxKind.SelectKeyword);

            var distinctAllKeyword = Current.Kind == SyntaxKind.DistinctKeyword ||
                                     Current.Kind == SyntaxKind.AllKeyword
                                         ? NextToken()
                                         : null;

            var topClause = Current.Kind == SyntaxKind.TopKeyword
                                ? ParseTopClause()
                                : null;

            var columns = ParseSelectColumnList();

            return new SelectClauseSyntax(_syntaxTree, selectKeyword, distinctAllKeyword, topClause, columns);
        }

        private TopClauseSyntax ParseTopClause()
        {
            var topKeyword = Match(SyntaxKind.TopKeyword);
            var value = Match(SyntaxKind.NumericLiteralToken);

            // Let's make sure that the int literal we got is actually a valid int.
            // Note: We check for IsMissing because we don't want to validate synthesized
            //       tokens -- we already added the "token missing" diagnostics to those.
            if (!value.IsMissing && !(value.Value is int))
            {
                var diagnostics = new List<Diagnostic>();
                diagnostics.ReportInvalidInteger(value.Span, value.ValueText);
                value = value.WithDiagnotics(diagnostics);
            }

            var expectedWithTies = Current.Kind == SyntaxKind.WithKeyword ||
                                   Current.Kind == SyntaxKind.TiesKeyword;
            if (!expectedWithTies)
                return new TopClauseSyntax(_syntaxTree, topKeyword, value, null, null);

            var withKeyword = Match(SyntaxKind.WithKeyword);
            var tiesKeyword = Match(SyntaxKind.TiesKeyword);
            return new TopClauseSyntax(_syntaxTree, topKeyword, value, withKeyword, tiesKeyword);
        }

        private SeparatedSyntaxList<SelectColumnSyntax> ParseSelectColumnList()
        {
            var columnsWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var oldIndex = _tokenIndex;

                var selectColumn = ParseSelectColumn();
                columnsWithCommas.Add(selectColumn);

                if (_tokenIndex == oldIndex)
                    SkipTokens(t => SyntaxFacts.CanFollowSelectColumn(t.Kind));

                if (Current.Kind != SyntaxKind.CommaToken && !SyntaxFacts.CanStartExpression(Current.Kind))
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                columnsWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<SelectColumnSyntax>(columnsWithCommas);
        }

        private SelectColumnSyntax ParseSelectColumn()
        {
            var isWildcard = Peek(0).Kind == SyntaxKind.AsteriskToken;
            var isQualifiedWildcard = Peek(0).Kind == SyntaxKind.IdentifierToken &&
                                      Peek(1).Kind == SyntaxKind.DotToken &&
                                      Peek(2).Kind == SyntaxKind.AsteriskToken;

            if (isWildcard)
            {
                var asteriskToken = Match(SyntaxKind.AsteriskToken);
                return new WildcardSelectColumnSyntax(_syntaxTree, null, null, asteriskToken);
            }

            if (isQualifiedWildcard)
            {
                var tableName = Match(SyntaxKind.IdentifierToken);
                var dotToken = Match(SyntaxKind.DotToken);
                var asteriskToken = Match(SyntaxKind.AsteriskToken);
                return new WildcardSelectColumnSyntax(_syntaxTree, tableName, dotToken, asteriskToken);
            }

            var expression = ParseExpression();
            var alias = ParseOptionalColumnAlias();
            return new ExpressionSelectColumnSyntax(_syntaxTree, expression, alias);
        }

        private AliasSyntax ParseOptionalColumnAlias()
        {
            var isAlias = Peek(0).Kind == SyntaxKind.AsKeyword ||
                          Peek(0).Kind == SyntaxKind.IdentifierToken && SyntaxFacts.CanFollowSelectColumn(Peek(1).Kind);

            return isAlias
                       ? ParseAlias()
                       : null;
        }

        private FromClauseSyntax ParseFromClause()
        {
            var fromKeyword = NextToken();
            var tableReferenceList = ParseTableReferenceList();
            return new FromClauseSyntax(_syntaxTree, fromKeyword, tableReferenceList);
        }

        private SeparatedSyntaxList<TableReferenceSyntax> ParseTableReferenceList()
        {
            var tableReferencesWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var tableReference = ParseTableReference();
                tableReferencesWithCommas.Add(tableReference);

                if (Current.Kind != SyntaxKind.CommaToken)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                tableReferencesWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<TableReferenceSyntax>(tableReferencesWithCommas);
        }

        private TableReferenceSyntax ParseTableReference()
        {
            TableReferenceSyntax left;

            if (Current.Kind == SyntaxKind.LeftParenthesisToken)
            {
                left = Lookahead.Kind == SyntaxKind.SelectKeyword
                           ? ParseDerivedTableReference()
                           : ParseParenthesizedTableReference();
            }
            else
            {
                left = ParseNamedTableReference();
            }

            while (true)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.CrossKeyword:
                        left = ParseCrossJoinTableReference(left);
                        break;
                    case SyntaxKind.InnerKeyword:
                    case SyntaxKind.JoinKeyword:
                        left = ParseInnerJoinTableReference(left);
                        break;
                    default:
                        if (Current.ContextualKind == SyntaxKind.LeftKeyword ||
                            Current.ContextualKind == SyntaxKind.RightKeyword ||
                            Current.Kind == SyntaxKind.FullKeyword)
                        {
                            left = ParseOuterJoinTableReference(left);
                        }
                        else
                        {
                            return left;
                        }
                        break;
                }
            }
        }

        private TableReferenceSyntax ParseParenthesizedTableReference()
        {
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var tableReference = ParseTableReference();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new ParenthesizedTableReferenceSyntax(_syntaxTree, leftParenthesis, tableReference, rightParenthesis);
        }

        private TableReferenceSyntax ParseNamedTableReference()
        {
            var tableName = Match(SyntaxKind.IdentifierToken);
            var alias = ParseOptionalAlias();
            return new NamedTableReferenceSyntax(_syntaxTree, tableName, alias);
        }

        private TableReferenceSyntax ParseCrossJoinTableReference(TableReferenceSyntax left)
        {
            var crossKeyword = Match(SyntaxKind.CrossKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            return new CrossJoinedTableReferenceSyntax(_syntaxTree, left, crossKeyword, joinKeyword, right);
        }

        private TableReferenceSyntax ParseInnerJoinTableReference(TableReferenceSyntax left)
        {
            var innerKeyword = NextTokenIf(SyntaxKind.InnerKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            var onKeyword = Match(SyntaxKind.OnKeyword);
            var condition = ParseExpression();
            return new InnerJoinedTableReferenceSyntax(_syntaxTree, left, innerKeyword, joinKeyword, right, onKeyword, condition);
        }

        private TableReferenceSyntax ParseOuterJoinTableReference(TableReferenceSyntax left)
        {
            var typeKeyword = NextTokenAsKeyword();
            var outerKeyword = NextTokenIf(SyntaxKind.OuterKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            var onKeyword = Match(SyntaxKind.OnKeyword);
            var condition = ParseExpression();
            return new OuterJoinedTableReferenceSyntax(_syntaxTree, left, typeKeyword, outerKeyword, joinKeyword, right, onKeyword, condition);
        }

        private DerivedTableReferenceSyntax ParseDerivedTableReference()
        {
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var query = ParseQuery();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            var asKeyword = NextTokenIf(SyntaxKind.AsKeyword);
            var name = Match(SyntaxKind.IdentifierToken);
            return new DerivedTableReferenceSyntax(_syntaxTree, leftParenthesis, query, rightParenthesis, asKeyword, name);
        }

        private WhereClauseSyntax ParseWhereClause()
        {
            var whereKeyword = NextToken();
            var predicate = ParseExpression();
            return new WhereClauseSyntax(_syntaxTree, whereKeyword, predicate);
        }

        private HavingClauseSyntax ParseHavingClause()
        {
            var havingKeyword = NextToken();
            var predicate = ParseExpression();
            return new HavingClauseSyntax(_syntaxTree, havingKeyword, predicate);
        }

        private GroupByClauseSyntax ParseGroupByClause()
        {
            var groupKeyword = Match(SyntaxKind.GroupKeyword);
            var byKeyword = Match(SyntaxKind.ByKeyword);
            var columnList = ParseGroupByColumnList();
            return new GroupByClauseSyntax(_syntaxTree, groupKeyword, byKeyword, columnList);
        }

        private SeparatedSyntaxList<GroupByColumnSyntax> ParseGroupByColumnList()
        {
            var columnsWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var expression = ParseExpression();
                var column = new GroupByColumnSyntax(_syntaxTree, expression);
                columnsWithCommas.Add(column);

                if (Current.Kind != SyntaxKind.CommaToken)
                    break;

                var comma = Match(SyntaxKind.CommaToken);
                columnsWithCommas.Add(comma);
            }

            return new SeparatedSyntaxList<GroupByColumnSyntax>(columnsWithCommas);
        }

        private AliasSyntax ParseOptionalAlias()
        {
            return Current.Kind == SyntaxKind.AsKeyword || Current.Kind == SyntaxKind.IdentifierToken
                       ? ParseAlias()
                       : null;
        }

        private AliasSyntax ParseAlias()
        {
            var asKeyword = NextTokenIf(SyntaxKind.AsKeyword);
            var identifier = Match(SyntaxKind.IdentifierToken);
            return new AliasSyntax(_syntaxTree, asKeyword, identifier);
        }
    }
 }