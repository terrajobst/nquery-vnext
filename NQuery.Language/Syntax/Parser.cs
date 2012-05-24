using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace NQuery.Language
{
    internal sealed class Parser
    {
        private List<SyntaxToken> _tokens = new List<SyntaxToken>();
        private int _tokenIndex;

        public Parser(string source)
        {
            LexAllTokens(source);
        }

        private SyntaxToken Current
        {
            get { return Peek(0); }
        }

        private SyntaxToken Lookahead
        {
            get { return Peek(1); }
        }

        private void LexAllTokens(string source)
        {
            var lexer = new Lexer(source);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                _tokens.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);
        }

        private SyntaxToken Peek(int offset)
        {
            var i = Math.Min(_tokenIndex + offset, _tokens.Count - 1);
            return _tokens[i];
        }

        private SyntaxToken NextToken()
        {
            var result = Current;
            _tokenIndex = Math.Min(_tokenIndex + 1, _tokens.Count - 1);
            return result;
        }

        private SyntaxToken? NextTokenIf(SyntaxKind kind)
        {
            return Current.Kind == kind
                       ? (SyntaxToken?)NextToken()
                       : null;
        }

        private SyntaxToken? NextTokenIfContextual(SyntaxKind kind)
        {
            return Current.ContextualKind == kind
                       ? (SyntaxToken?)NextToken()
                       : null;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            // TODO: Report token expected
            // _errorReporter.TokenExpected(_token.Range, _token.Text, tokenID);
            return CreateMissingToken(kind, SyntaxKind.BadToken);
        }

        private SyntaxToken MatchContextual(SyntaxKind kind)
        {
            if (Current.ContextualKind == kind)
            {
                var result = new SyntaxToken(Current.ContextualKind,
                                             Current.ContextualKind,
                                             Current.IsMissing,
                                             Current.Span,
                                             Current.Text,
                                             Current.Value,
                                             Current.LeadingTrivia,
                                             Current.TrailingTrivia);
                NextToken();
                return result;
            }

            // TODO: Report token expected
            // _errorReporter.TokenExpected(_token.Range, _token.Text, tokenID);
            return CreateMissingToken(kind, kind);
        }

        private SyntaxToken CreateMissingToken(SyntaxKind kind, SyntaxKind contextualKind)
        {
            var start = Current.FullSpan.Start;
            var span = new TextSpan(start, 0);
            return new SyntaxToken(kind, contextualKind, true, span, string.Empty, null, new List<SyntaxTrivia>(), new List<SyntaxTrivia>());
        }

        public CompilationUnitSyntax ParseRootQuery()
        {
            var query = ParseQueryWithOptionalCte();
            var endOfFileToken = ParseEndOfFileToken();
            return new CompilationUnitSyntax(query, endOfFileToken);
        }

        public CompilationUnitSyntax ParseRootExpression()
        {
            var expression = ParseExpression();
            var endOfFileToken = ParseEndOfFileToken();
            return new CompilationUnitSyntax(expression, endOfFileToken);
        }

        private SyntaxToken ParseEndOfFileToken()
        {
            if (Current.Kind == SyntaxKind.EndOfFileToken)
                return NextToken();

            var tokens = new List<SyntaxToken>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
                tokens.Add(NextToken());

            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            var start = tokens.First().Span.Start;
            var end = tokens.First().Span.End;
            var span = TextSpan.FromBounds(start, end);
            var skippedTokensNode = new SkippedTokensTriviaSyntax(tokens);
            var skippedTokensTrivia = new SyntaxTrivia(SyntaxKind.SkippedTokensTrivia, null, span, skippedTokensNode);

            var leadingTrivia = new List<SyntaxTrivia>(endOfFileToken.LeadingTrivia.Count + 1);
            leadingTrivia.Add(skippedTokensTrivia);
            leadingTrivia.AddRange(endOfFileToken.LeadingTrivia);

            var result = new SyntaxToken(endOfFileToken.Kind,
                                         endOfFileToken.ContextualKind,
                                         endOfFileToken.IsMissing,
                                         endOfFileToken.Span,
                                         endOfFileToken.Text,
                                         endOfFileToken.Value,
                                         leadingTrivia,
                                         endOfFileToken.TrailingTrivia);

            return result;
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseSubExpression(null, 0);
        }

        private ExpressionSyntax ParseSubExpression(ExpressionSyntax left, int precedence)
        {
            if (left == null)
            {
                // No left operand, so we parse one and take care about leading unary operators

                var unaryExpression = SyntaxFacts.GetUnaryOperatorExpression(Current.Kind);

                if (unaryExpression == SyntaxKind.BadToken)
                {
                    left = ParseSimpleExpression();
                }
                else
                {
                    var operatorToken = NextToken();
                    var operatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryExpression);
                    var expression = ParseSubExpression(null, operatorPrecedence);
                    left = new UnaryExpressionSyntax(operatorToken, expression);
                }
            }

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                // Special handling for NOT BETWEEN, NOT IN, NOT LIKE, NOT SIMILAR TO, and NOT SOUNDSLIKE.

                SyntaxToken? notKeyword = null;

                if (Current.Kind == SyntaxKind.NotKeyword)
                {
                    if (Lookahead.Kind == SyntaxKind.BetweenKeyword ||
                        Lookahead.Kind == SyntaxKind.InKeyword ||
                        Lookahead.Kind == SyntaxKind.LikeKeyword ||
                        Lookahead.Kind == SyntaxKind.SimilarKeyword ||
                        Lookahead.Kind == SyntaxKind.SoundslikeKeyword)
                    {
                        notKeyword = NextToken();
                    }
                }

                // Special handling for the only ternary operator BETWEEN

                if (Current.Kind == SyntaxKind.BetweenKeyword)
                {
                    var betweenPrecedence = SyntaxFacts.GetTernaryOperatorPrecedence(SyntaxKind.BetweenExpression);
                    var betweenKeyword = NextToken();
                    var lowerBound = ParseSubExpression(null, betweenPrecedence);
                    var andKeyword = Match(SyntaxKind.AndKeyword);
                    var upperBound = ParseSubExpression(null, betweenPrecedence);
                    left = new BetweenExpressionSyntax(left, notKeyword, betweenKeyword, lowerBound, andKeyword, upperBound);
                }
                else
                {
                    // If there is no binary operator we are finished

                    var binaryExpression = SyntaxFacts.GetBinaryOperatorExpression(Current.Kind);
                    if (binaryExpression == SyntaxKind.BadToken)
                        break;

                    var operatorPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression);

                    // Precedence is lower, parse it later

                    if (operatorPrecedence < precedence)
                        break;

                    // Precedence is higher

                    var operatorToken = NextToken();

                    // Special handling for some operators

                    switch (operatorToken.Kind)
                    {
                        case SyntaxKind.SimilarKeyword:
                        {
                            var toKeyword = Match(SyntaxKind.ToKeyword);
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new SimilarToExpressionSyntax(left, notKeyword, operatorToken, toKeyword, expression);
                            break;
                        }
                        case SyntaxKind.SoundslikeKeyword:
                        {
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new SoundslikeExpressionSyntax(left, notKeyword, operatorToken, expression);
                            break;
                        }
                        case SyntaxKind.LikeKeyword:
                        {
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new LikeExpressionSyntax(left, notKeyword, operatorToken, expression);
                            break;
                        }
                        case SyntaxKind.InKeyword:
                        {
                            var argumentList = ParseArgumentList();
                            left = new InExpressionSyntax(left, notKeyword, operatorToken, argumentList);
                            break;
                        }
                        case SyntaxKind.AllKeyword:
                        case SyntaxKind.SomeKeyword:
                        case SyntaxKind.AnyKeyword:
                        {
                            // TODO: I believe the following check should be done in the binder.
                            if (binaryExpression != SyntaxKind.EqualExpression &&
                                binaryExpression != SyntaxKind.NotEqualExpression &&
                                binaryExpression != SyntaxKind.NotLessExpression &&
                                binaryExpression != SyntaxKind.NotGreaterExpression &&
                                binaryExpression != SyntaxKind.LessExpression &&
                                binaryExpression != SyntaxKind.LessOrEqualExpression &&
                                binaryExpression != SyntaxKind.GreaterExpression &&
                                binaryExpression != SyntaxKind.GreaterOrEqualExpression)
                            {
                                //TODO
                                //_errorReporter.InvalidOperatorForAllAny(_token.Range, binaryOp);
                            }

                            var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
                            var query = ParseQuery();
                            var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                            left = new AllAnySubselectSyntax(left, operatorToken, leftParentheses, query, rightParentheses);
                            break;
                        }
                        default:
                        {
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new BinaryExpressionSyntax(left, operatorToken, expression);
                            break;
                        }
                    }
                }
            }

            return left;
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

                return new IsNullExpressionSyntax(expression, isToken, notToken, nullToken);
            }

            return expression;
        }

        private ExpressionSyntax ParseMemberExpression()
        {
            ExpressionSyntax target = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.DotToken)
            {
                var dot = NextToken();

                var name = Match(SyntaxKind.IdentifierToken);

                if (Current.Kind != SyntaxKind.LeftParenthesesToken)
                {
                    target = new PropertyAccessExpressionSyntax(target, dot, name);
                }
                else
                {
                    var argumentList = ParseArgumentList();
                    target = new MethodInvocationExpressionSyntax(target, dot, name, argumentList);
                }
            }

            return target;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.NullKeyword:
                    return new LiteralExpressionSyntax(NextToken(), null);

                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();

                case SyntaxKind.DateLiteralToken:
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.StringLiteralToken:
                    return ParseLiteral();

                case SyntaxKind.ExistsKeyword:
                {
                    var existsKeyword = NextToken();
                    var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
                    var query = ParseQuery();
                    var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                    return new ExistsSubselectSyntax(existsKeyword, leftParentheses, query, rightParentheses);
                }

                case SyntaxKind.ParameterMarkerToken:
                {
                    var parameterMarker = NextToken();
                    var name = Match(SyntaxKind.IdentifierToken);
                    return new ParameterExpressionSyntax(parameterMarker, name);
                }

                case SyntaxKind.CastKeyword:
                {
                    var castKeyword = NextToken();
                    var leftParenthesesToken = Match(SyntaxKind.LeftParenthesesToken);
                    var expression = ParseExpression();
                    var asKeyword = Match(SyntaxKind.AsKeyword);
                    var typeReference = ParseTypeReference();
                    var rightParenthesesToken = Match(SyntaxKind.RightParenthesesToken);

                    return new CastExpressionSyntax(castKeyword, leftParenthesesToken, expression, asKeyword, typeReference, rightParenthesesToken);
                }

                case SyntaxKind.CaseKeyword:
                {
                    var caseKeyword = NextToken();

                    var hasInput = Current.Kind != SyntaxKind.WhenKeyword &&
                                   Current.Kind != SyntaxKind.ElseKeyword &&
                                   Current.Kind != SyntaxKind.EndKeyword;

                    var inputExpression = hasInput ? ParseExpression() : null;
                    var caseLabels = new List<CaseLabelSyntax>();

                    if (Current.Kind != SyntaxKind.WhenKeyword)
                    {
                        Match(SyntaxKind.WhenKeyword);
                    }
                    else
                    {
                        while (Current.Kind == SyntaxKind.WhenKeyword)
                        {
                            var whenKeyword = NextToken();
                            var whenExpression = ParseExpression();
                            var thenKeyword = Match(SyntaxKind.ThenKeyword);
                            var thenExpression = ParseExpression();

                            var caseLabel = new CaseLabelSyntax(whenKeyword, whenExpression, thenKeyword, thenExpression);
                            caseLabels.Add(caseLabel);
                        }
                    }

                    SyntaxToken? elseKeyword = null;
                    ExpressionSyntax elseExpression = null;

                    if (Current.Kind == SyntaxKind.ElseKeyword)
                    {
                        elseKeyword = NextToken();
                        elseExpression = ParseExpression();
                    }

                    var endKeyword = Match(SyntaxKind.EndKeyword);

                    return new CaseExpressionSyntax(caseKeyword, inputExpression, caseLabels, elseKeyword, elseExpression, endKeyword);
                }

                case SyntaxKind.CoalesceKeyword:
                {
                    var coalesceKeyword = NextToken();
                    var arguments = ParseArgumentList();
                    return new CoalesceExpressionSyntax(coalesceKeyword, arguments);
                }

                case SyntaxKind.NullIfKeyword:
                {
                    var nullIfKeyword = NextToken();
                    var leftParenthesesToken = Match(SyntaxKind.LeftParenthesesToken);
                    var leftExpression = ParseExpression();
                    var commaToken = Match(SyntaxKind.CommaToken);
                    var rightExpression = ParseExpression();
                    var rightParenthesesToken = Match(SyntaxKind.RightParenthesesToken);
                    return new NullIfExpressionSyntax(nullIfKeyword, leftParenthesesToken, leftExpression, commaToken, rightExpression, rightParenthesesToken);
                }

                case SyntaxKind.IdentifierToken:
                {
                    var identifier = NextToken();

                    if (Current.Kind != SyntaxKind.LeftParenthesesToken)
                        return new NameExpressionSyntax(identifier);

                    if (Lookahead.Kind == SyntaxKind.MultiplyToken &&
                        string.Equals(identifier.Text, "COUNT", StringComparison.OrdinalIgnoreCase))
                    {
                        var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
                        var asteriskToken = Match(SyntaxKind.MultiplyToken);
                        var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                        return new CountAllExpressionSyntax(identifier, leftParentheses, asteriskToken, rightParentheses);
                    }
                    
                    var arguments = ParseArgumentList();
                    return new FunctionInvocationExpressionSyntax(identifier, arguments);
                }

                case SyntaxKind.LeftParenthesesToken:
                {
                    if (Lookahead.Kind == SyntaxKind.SelectKeyword)
                    {
                        var leftParentheses = NextToken();
                        var query = ParseQuery();
                        var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                        return new SingleRowSubselectSyntax(leftParentheses, query, rightParentheses);
                    }
                    else
                    {
                        var leftParentheses = NextToken();
                        var expression = ParseExpression();
                        var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                        return new ParenthesizedExpressionSyntax(leftParentheses, expression, rightParentheses);
                    }
                }

                default:
                    {
                        // TODO
                        //_errorReporter.SimpleExpressionExpected(_token.Range, _token.Text);
                        //return LiteralExpression.FromNull();
                        var identifier = Match(SyntaxKind.IdentifierToken);
                        return new NameExpressionSyntax(identifier);
                    }
            }
        }

        private TypeReferenceSyntax ParseTypeReference()
		{
            // TODO
			//if (_token.Id == TokenId.String)
			{
		        var token = Match(SyntaxKind.StringLiteralToken);
			    var typeName = ParseStringValue(token.Text);

			    return new TypeReferenceSyntax(token, typeName);
			}
            // TODO
            //else
            //{
            //    _rangeRecorder.Begin();

            //    StringBuilder sb = new StringBuilder();
            //    bool wasVerbatim = false;
            //    while (_token.Id != TokenId.Eof)
            //    {
            //        Identifier identifier = ParseIdentifier();

            //        if (!identifier.Verbatim && !identifier.Parenthesized)
            //        {
            //            sb.Append(identifier.Text);
            //        }
            //        else
            //        {
            //            wasVerbatim = true;

            //            // Include quotes and brackets for better error reporting.
            //            sb.Append(identifier.ToString());
            //        }

            //        if (_token.Id != TokenId.Dot)
            //            break;

            //        sb.Append(".");
            //        NextToken();
            //    }

            //    string typeName = sb.ToString();
            //    SourceRange typeNameSourceRange = _rangeRecorder.End();

            //    if (wasVerbatim)
            //        _errorReporter.InvalidTypeReference(typeNameSourceRange, typeName);

            //    typeReference.TypeName = typeName;
            //    typeReference.TypeNameSourceRange = typeNameSourceRange;
            //    typeReference.CaseSensitve = false;
            //}

            //return typeReference;
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var token = NextToken();
            var value = token.Kind == SyntaxKind.TrueKeyword;
            return new LiteralExpressionSyntax(token, value);
        }

        private ExpressionSyntax ParseLiteral()
        {
            var token = NextToken();
            return new LiteralExpressionSyntax(token, token.Value);
        }

        private static string ParseStringValue(string text)
        {
            var sb = new StringBuilder(text.Length);

            // Text includes leading/trailing/masking quotes

            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '\'' && text[i + 1] == '\'')
                    i++;

                sb.Append(text[i]);
            }

            var value = sb.ToString();
            return value;
        }

        private ArgumentListSyntax ParseArgumentList()
        {
            var leftParenthesis = Match(SyntaxKind.LeftParenthesesToken);

            var arguments = new List<ArgumentSyntax>();

            while (true)
            {
                var argument = ParseArgument();
                arguments.Add(argument);

                if (argument.Comma == null)
                    break;
            }

            var rightParenthesis = Match(SyntaxKind.RightParenthesesToken);

            return new ArgumentListSyntax(leftParenthesis, arguments, rightParenthesis);
        }

        private ArgumentSyntax ParseArgument()
        {
            var expression = ParseExpression();
            var comma = Current.Kind == SyntaxKind.CommaToken
                            ? (SyntaxToken?) NextToken()
                            : null;

            var argument = new ArgumentSyntax(expression, comma);
            return argument;
        }

        private QuerySyntax ParseQueryWithOptionalCte()
        {
            if (Current.Kind != SyntaxKind.WithKeyword)
                return ParseQuery();

            var withKeyword = Match(SyntaxKind.WithKeyword);

            var commonTableExpressions = new List<CommonTableExpressionSyntax>();

            while (true)
            {
                var commonTableExpression = ParseCommonTableExpression();
                commonTableExpressions.Add(commonTableExpression);

                if (commonTableExpression.CommaToken == null)
                    break;
            }

            var query = ParseQuery();
            return new CommonTableExpressionQuerySyntax(withKeyword, commonTableExpressions, query);
        }

        private CommonTableExpressionSyntax ParseCommonTableExpression()
        {
            var identifer = Match(SyntaxKind.IdentifierToken);
            var commonTableExpressionColumnNameList = ParseCommonTableExpressionColumnNameList();
            var asKeyword = Match(SyntaxKind.AsKeyword);
            var leftParentheses = Match(SyntaxKind.RightParenthesesToken);
            var query = ParseQuery();
            var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);

            return new CommonTableExpressionSyntax(identifer, commonTableExpressionColumnNameList, asKeyword, leftParentheses, query, rightParentheses, commaToken);
        }

        private CommonTableExpressionColumnNameListSyntax ParseCommonTableExpressionColumnNameList()
        {
            CommonTableExpressionColumnNameListSyntax commonTableExpressionColumnNameList;
            if (Current.Kind == SyntaxKind.LeftParenthesesToken)
            {
                var leftParentheses = NextToken();

                var columnNames = new List<SyntaxToken>();

                while (true)
                {
                    var columnName = Match(SyntaxKind.IdentifierToken);
                    columnNames.Add(columnName);

                    if (Current.Kind != SyntaxKind.CommaToken)
                        break;

                    NextToken();
                }

                var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                commonTableExpressionColumnNameList = new CommonTableExpressionColumnNameListSyntax(leftParentheses, columnNames, rightParentheses);
            }
            else
                commonTableExpressionColumnNameList = null;
            return commonTableExpressionColumnNameList;
        }

        private QuerySyntax ParseQuery()
        {
            var query = ParseUnifiedOrExceptionalQuery();

            // ORDER BY

            if (Current.Kind != SyntaxKind.OrderKeyword)
                return query;

            var orderKeyword = Match(SyntaxKind.OrderKeyword);
            var byKeyword = Match(SyntaxKind.ByKeyword);

            var columns = new List<OrderByColumnSyntax>();

            while (true)
            {
                var expression = ParseExpression();
                SyntaxToken? modifier;

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

                var comma = NextTokenIf(SyntaxKind.CommaToken);
                var column = new OrderByColumnSyntax(expression, modifier, comma);
                columns.Add(column);

                if (comma == null)
                    break;
            }

            return new OrderedQuerySyntax(query, orderKeyword, byKeyword, columns);
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
                    leftQuery = new UnionQuerySyntax(leftQuery, unionKeyword, allKeyword, rightQuery);
                }
                else
                {
                    var exceptKeyword = Match(SyntaxKind.ExceptKeyword);
                    var rightQuery = ParseIntersectionalQuery();
                    leftQuery = new ExceptQuerySyntax(leftQuery, exceptKeyword, rightQuery);
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

                leftQuery = new IntersectQuerySyntax(leftQuery, intersectKeyword, rightQuery);
            }

            return leftQuery;
        }

        private QuerySyntax ParseSelectQuery()
        {
            if (Current.Kind == SyntaxKind.LeftParenthesesToken)
            {
                var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
                var query = ParseQuery();
                var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
                return new ParenthesizedQuerySyntax(leftParentheses, query, rightParentheses);
            }

            var selectKeyword = Match(SyntaxKind.SelectKeyword);

            var distinctAllKeyword = Current.Kind == SyntaxKind.DistinctKeyword ||
                                     Current.Kind == SyntaxKind.AllKeyword
                                         ? (SyntaxToken?) NextToken()
                                         : null;

            var topClause = Current.Kind == SyntaxKind.TopKeyword
                                ? ParseTopClause()
                                : null;

            var selectColumns = ParseSelectColumns();

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

            return new SelectQuerySyntax(selectKeyword, distinctAllKeyword, topClause, selectColumns, fromClause, whereClause, groupByClause, havingClause);
        }

        private TopClauseSyntax ParseTopClause()
        {
            var topKeyword = NextToken();
            var value = Match(SyntaxKind.NumericLiteralToken);
            var withKeyword = NextTokenIf(SyntaxKind.WithKeyword);
            var tiesKeyword = NextTokenIf(SyntaxKind.TiesKeyword);
            return new TopClauseSyntax(topKeyword, value, withKeyword, tiesKeyword);
        }

        private List<SelectColumnSyntax> ParseSelectColumns()
        {
            var columns = new List<SelectColumnSyntax>();

            //if (Current.Kind == SyntaxKind.EndOfFileToken)
            //    _errorReporter.SimpleExpressionExpected(_token.Range, _token.Text);

            while (true)
            {
                var selectColumn = ParseColumnSource();
                columns.Add(selectColumn);

                if (selectColumn.CommaToken == null)
                    break;
            }

            return columns;
        }

        private FromClauseSyntax ParseFromClause()
        {
            var fromKeyword = NextToken();
            var tableReferences = new List<TableReferenceSyntax>();

            TableReferenceSyntax tableReference;
            do
            {
                tableReference = ParseTableReference();
                tableReferences.Add(tableReference);
            } while (tableReference.CommaToken != null);
        
            return new FromClauseSyntax(fromKeyword, tableReferences);
        }

        private TableReferenceSyntax ParseTableReference()
        {
            TableReferenceSyntax left;

            if (Current.Kind == SyntaxKind.LeftParenthesesToken)
            {
                left = Lookahead.Kind == SyntaxKind.SelectKeyword
                           ? ParseDerivedTableReference()
                           : ParseParenthesizedTableReference();
            }
            else
            {
                left = ParseNamedTableReference();
            }

            while (left.CommaToken == null)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.CrossKeyword:
                        left = ParseCrossJoinTableReference(left);
                        break;

                    case SyntaxKind.JoinKeyword:
                    case SyntaxKind.InnerKeyword:
                        left = ParseInnerJoinTableReference(left);
                        break;

                    case SyntaxKind.LeftKeyword:
                    case SyntaxKind.RightKeyword:
                    case SyntaxKind.FullKeyword:
                        left = ParseOuterJoinTableReference(left);
                        break;

                    default:
                        return left;
                }
            }

            return left;
        }

        private TableReferenceSyntax ParseParenthesizedTableReference()
        {
            var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
            var tableReference = ParseTableReference();
            var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new ParenthesizedTableReferenceSyntax(leftParentheses, tableReference, rightParentheses, commaToken);
        }

        private TableReferenceSyntax ParseNamedTableReference()
        {
            var tableName = Match(SyntaxKind.IdentifierToken);
            var alias = ParseOptionalAlias();
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new NamedTableReferenceSyntax(tableName, alias, commaToken);
        }

        private TableReferenceSyntax ParseCrossJoinTableReference(TableReferenceSyntax left)
        {
            var crossKeyword = Match(SyntaxKind.CrossKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new CrossJoinedTableReferenceSyntax(left, crossKeyword, joinKeyword, right, commaToken);
        }

        private TableReferenceSyntax ParseInnerJoinTableReference(TableReferenceSyntax left)
        {
            var innerKeyword = NextTokenIf(SyntaxKind.InnerKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            var onKeyword = Match(SyntaxKind.OnKeyword);
            var condition = ParseExpression();
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new InnerJoinedTableReferenceSyntax(left, innerKeyword, joinKeyword, right, onKeyword, condition, commaToken);
        }

        private TableReferenceSyntax ParseOuterJoinTableReference(TableReferenceSyntax left)
        {
            var typeKeyword = NextToken();
            var outerKeyword = NextTokenIf(SyntaxKind.OuterKeyword);
            var joinKeyword = Match(SyntaxKind.JoinKeyword);
            var right = ParseTableReference();
            var onKeyword = Match(SyntaxKind.OnKeyword);
            var condition = ParseExpression();
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new OuterJoinedTableReferenceSyntax(left, typeKeyword, outerKeyword, joinKeyword, right, onKeyword, condition, commaToken);
        }

        private DerivedTableReferenceSyntax ParseDerivedTableReference()
        {
            var leftParentheses = Match(SyntaxKind.LeftParenthesesToken);
            var query = ParseQuery();
            var rightParentheses = Match(SyntaxKind.RightParenthesesToken);
            var asKeyword = NextTokenIf(SyntaxKind.AsKeyword);
            var name = Match(SyntaxKind.IdentifierToken);
            var commaToken = NextTokenIf(SyntaxKind.CommaToken);
            return new DerivedTableReferenceSyntax(leftParentheses, query, rightParentheses, asKeyword, name, commaToken);
        }

        private WhereClauseSyntax ParseWhereClause()
        {
            var whereKeyword = NextToken();
            var predicate = ParseExpression();
            return new WhereClauseSyntax(whereKeyword, predicate);
        }

        private HavingClauseSyntax ParseHavingClause()
        {
            var havingKeyword = NextToken();
            var predicate = ParseExpression();
            return new HavingClauseSyntax(havingKeyword, predicate);
        }

        private SelectColumnSyntax ParseColumnSource()
        {
            var isWildcard = Peek(0).Kind == SyntaxKind.MultiplyToken;
            var isQualifiedWildcard = Peek(0).Kind == SyntaxKind.IdentifierToken &&
                                      Peek(1).Kind == SyntaxKind.DotToken &&
                                      Peek(2).Kind == SyntaxKind.MultiplyToken;

            if (isWildcard)
            {
                var asteriskToken = Match(SyntaxKind.MultiplyToken);
                var commaToken = NextTokenIf(SyntaxKind.CommaToken);
                return new WildcardSelectColumnSyntax(null, null, asteriskToken, commaToken);
            }
            
            if (isQualifiedWildcard)
            {
                var tableName = Match(SyntaxKind.IdentifierToken);
                var dotToken = Match(SyntaxKind.DotToken);
                var asteriskToken = Match(SyntaxKind.MultiplyToken);
                var commaToken = NextTokenIf(SyntaxKind.CommaToken);
                return new WildcardSelectColumnSyntax(tableName, dotToken, asteriskToken, commaToken);
            }
            else
            {
                var expression = ParseExpression();
                var alias = ParseOptionalAlias();
                var commaToken = NextTokenIf(SyntaxKind.CommaToken);
                return new ExpressionSelectColumnSyntax(expression, alias, commaToken);
            }
        }

        private GroupByClauseSyntax ParseGroupByClause()
        {
            var groupKeyword = Match(SyntaxKind.GroupKeyword);
            var byKeyword = Match(SyntaxKind.ByKeyword);

            var columns = new List<GroupByColumnSyntax>();

            while (true)
            {
                var expression = ParseExpression();
                var comma = NextTokenIf(SyntaxKind.CommaToken);
                var column = new GroupByColumnSyntax(expression, comma);
                columns.Add(column);

                if (comma == null)
                    break;
            }

            return new GroupByClauseSyntax(groupKeyword, byKeyword, columns);
        }

        private AliasSyntax ParseOptionalAlias()
        {
            var asKeyword = NextTokenIf(SyntaxKind.AsKeyword);
            if (asKeyword == null && Current.Kind != SyntaxKind.IdentifierToken)
                return null;

            var identifier = Match(SyntaxKind.IdentifierToken);
            return new AliasSyntax(asKeyword, identifier);
        }
    }
 }