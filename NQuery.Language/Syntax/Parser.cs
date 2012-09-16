using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    internal sealed class Parser
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly List<SyntaxToken> _tokens = new List<SyntaxToken>();
        private int _tokenIndex;

        public Parser(string source, SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
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
                    var trivia = new[] { CreateSkippedTokensTrivia(badTokens) }
                                 .Concat(token.LeadingTrivia).ToArray();
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

        private SyntaxToken NextToken()
        {
            var result = Current;
            _tokenIndex = Math.Min(_tokenIndex + 1, _tokens.Count - 1);
            return result;
        }

        private SyntaxToken NextTokenAsKeyword()
        {
            var result = NextToken();
            return result.WithKind(result.ContextualKind);
        }

        private SyntaxToken? NextTokenIf(SyntaxKind kind)
        {
            return Current.Kind == kind
                       ? (SyntaxToken?)NextToken()
                       : null;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            return Current.Kind == kind
                       ? NextToken()
                       : CreateMissingToken(kind, SyntaxKind.BadToken);
        }

        private SyntaxToken CreateMissingToken(SyntaxKind kind, SyntaxKind contextualKind)
        {
            var start = Current.FullSpan.Start;
            var span = new TextSpan(start, 0);
            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(Current, kind);
            return new SyntaxToken(kind, contextualKind, true, span, string.Empty, null, new SyntaxTrivia[0], new SyntaxTrivia[0], diagnostics);
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

            var tokens = new List<SyntaxToken>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
                tokens.Add(NextToken());

            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            var diagnostics = new List<Diagnostic>(1);
            diagnostics.ReportTokenExpected(tokens[0], SyntaxKind.EndOfFileToken);
            tokens[0] = tokens[0].WithDiagnotics(diagnostics);

            var skippedTokensTrivia = CreateSkippedTokensTrivia(tokens);

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
                                         endOfFileToken.TrailingTrivia,
                                         endOfFileToken.Diagnostics);

            return result;
        }

        private SyntaxTrivia CreateSkippedTokensTrivia(IList<SyntaxToken> tokens)
        {
            var start = tokens.First().Span.Start;
            var end = tokens.Last().Span.End;
            var span = TextSpan.FromBounds(start, end);
            var structure = new SkippedTokensTriviaSyntax(_syntaxTree, tokens);
            var trivia = new SyntaxTrivia(SyntaxKind.SkippedTokensTrivia, null, span, structure, new Diagnostic[0]);
            return trivia;
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
                    left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, expression);
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
                    left = new BetweenExpressionSyntax(_syntaxTree, left, notKeyword, betweenKeyword, lowerBound, andKeyword, upperBound);
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
                            left = new SimilarToExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, toKeyword, expression);
                            break;
                        }
                        case SyntaxKind.SoundslikeKeyword:
                        {
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new SoundslikeExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, expression);
                            break;
                        }
                        case SyntaxKind.LikeKeyword:
                        {
                            var expression = ParseSubExpression(null, operatorPrecedence);
                            left = new LikeExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, expression);
                            break;
                        }
                        case SyntaxKind.InKeyword:
                        {
                            var argumentList = ParseArgumentList();
                            left = new InExpressionSyntax(_syntaxTree, left, notKeyword, operatorToken, argumentList);
                            break;
                        }
                        default:
                        {
                            var isAllAny = Current.Kind == SyntaxKind.AllKeyword ||
                                           Current.Kind == SyntaxKind.SomeKeyword ||
                                           Current.Kind == SyntaxKind.AnyKeyword;

                            if (isAllAny)
                            {
                                var allAnyOperatorToken = binaryExpression.IsValidAllAnyOperator()
                                                              ? operatorToken
                                                              : operatorToken.WithInvalidOperatorForAllAnyDiagnostics();
                                var keyword = NextToken();
                                var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
                                var query = ParseQuery();
                                var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                                left = new AllAnySubselectSyntax(_syntaxTree, left, allAnyOperatorToken, keyword, leftParenthesis, query, rightParenthesis);
                            }
                            else
                            {
                                var expression = ParseSubExpression(null, operatorPrecedence);
                                left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, expression);
                            }
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

                return new IsNullExpressionSyntax(_syntaxTree, expression, isToken, notToken, nullToken);
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
                    return new LiteralExpressionSyntax(_syntaxTree, NextToken(), null);

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
                    var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
                    var query = ParseQuery();
                    var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                    return new ExistsSubselectSyntax(_syntaxTree, existsKeyword, leftParenthesis, query, rightParenthesis);
                }

                case SyntaxKind.AtToken:
                {
                    var atToken = NextToken();
                    var name = Match(SyntaxKind.IdentifierToken);
                    return new VariableExpressionSyntax(_syntaxTree, atToken, name);
                }

                case SyntaxKind.CastKeyword:
                {
                    var castKeyword = NextToken();
                    var leftParenthesisToken = Match(SyntaxKind.LeftParenthesisToken);
                    var expression = ParseExpression();
                    var asKeyword = Match(SyntaxKind.AsKeyword);
                    var typeName = Match(SyntaxKind.IdentifierToken);
                    var rightParenthesisToken = Match(SyntaxKind.RightParenthesisToken);

                    return new CastExpressionSyntax(_syntaxTree, castKeyword, leftParenthesisToken, expression, asKeyword, typeName, rightParenthesisToken);
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

                            var caseLabel = new CaseLabelSyntax(_syntaxTree, whenKeyword, whenExpression, thenKeyword, thenExpression);
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

                    return new CaseExpressionSyntax(_syntaxTree, caseKeyword, inputExpression, caseLabels, elseKeyword, elseExpression, endKeyword);
                }

                case SyntaxKind.CoalesceKeyword:
                {
                    var coalesceKeyword = NextToken();
                    var arguments = ParseArgumentList();
                    return new CoalesceExpressionSyntax(_syntaxTree, coalesceKeyword, arguments);
                }

                case SyntaxKind.NullIfKeyword:
                {
                    var nullIfKeyword = NextToken();
                    var leftParenthesisToken = Match(SyntaxKind.LeftParenthesisToken);
                    var leftExpression = ParseExpression();
                    var commaToken = Match(SyntaxKind.CommaToken);
                    var rightExpression = ParseExpression();
                    var rightParenthesisToken = Match(SyntaxKind.RightParenthesisToken);
                    return new NullIfExpressionSyntax(_syntaxTree, nullIfKeyword, leftParenthesisToken, leftExpression, commaToken, rightExpression, rightParenthesisToken);
                }

                case SyntaxKind.IdentifierToken:
                {
                    var identifier = NextToken();

                    if (Current.Kind != SyntaxKind.LeftParenthesisToken)
                        return new NameExpressionSyntax(_syntaxTree, identifier);

                    if (Lookahead.Kind == SyntaxKind.AsteriskToken && identifier.Matches("COUNT"))
                    {
                        var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
                        var asteriskToken = Match(SyntaxKind.AsteriskToken);
                        var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                        return new CountAllExpressionSyntax(_syntaxTree, identifier, leftParenthesis, asteriskToken, rightParenthesis);
                    }
                    
                    var arguments = ParseArgumentList();
                    return new FunctionInvocationExpressionSyntax(_syntaxTree, identifier, arguments);
                }

                case SyntaxKind.LeftParenthesisToken:
                {
                    if (Lookahead.Kind == SyntaxKind.SelectKeyword)
                    {
                        var leftParenthesis = NextToken();
                        var query = ParseQuery();
                        var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                        return new SingleRowSubselectSyntax(_syntaxTree, leftParenthesis, query, rightParenthesis);
                    }
                    else
                    {
                        var leftParenthesis = NextToken();
                        var expression = ParseExpression();
                        var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
                        return new ParenthesizedExpressionSyntax(_syntaxTree, leftParenthesis, expression, rightParenthesis);
                    }
                }

                default:
                {
                    var identifier = Match(SyntaxKind.IdentifierToken);
                    return new NameExpressionSyntax(_syntaxTree, identifier);
                }
            }
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

        private ArgumentListSyntax ParseArgumentList()
        {
            var leftParenthesis = Match(SyntaxKind.LeftParenthesisToken);
            var expressionList = ParseArgumentExpressionList();
            var rightParenthesis = Match(SyntaxKind.RightParenthesisToken);
            return new ArgumentListSyntax(_syntaxTree, leftParenthesis, expressionList, rightParenthesis);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArgumentExpressionList()
        {
            if (Current.Kind == SyntaxKind.RightParenthesisToken)
                return SeparatedSyntaxList<ExpressionSyntax>.Empty;

            var expressionsWithCommas = new List<SyntaxNodeOrToken>();

            while (true)
            {
                var argument = ParseExpression();
                expressionsWithCommas.Add(argument);

                if (Current.Kind != SyntaxKind.CommaToken)
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
                                         ? (SyntaxToken?) NextToken()
                                         : null;

            var topClause = Current.Kind == SyntaxKind.TopKeyword
                                ? ParseTopClause()
                                : null;

            var columns = ParseSelectColumnList();

            return new SelectClauseSyntax(_syntaxTree, selectKeyword, distinctAllKeyword, topClause, columns);
        }

        private TopClauseSyntax ParseTopClause()
        {
            var topKeyword = NextToken();
            var value = Match(SyntaxKind.NumericLiteralToken);
            var withKeyword = NextTokenIf(SyntaxKind.WithKeyword);
            var tiesKeyword = NextTokenIf(SyntaxKind.TiesKeyword);
            return new TopClauseSyntax(_syntaxTree, topKeyword, value, withKeyword, tiesKeyword);
        }

        private SeparatedSyntaxList<SelectColumnSyntax> ParseSelectColumnList()
        {
            var columnsWithCommas = new List<SyntaxNodeOrToken>();

            //if (Current.Kind == SyntaxKind.EndOfFileToken)
            //    _errorReporter.SimpleExpressionExpected(_token.Range, _token.Text);

            while (true)
            {
                var selectColumn = ParseSelectColumn();
                columnsWithCommas.Add(selectColumn);

                if (Current.Kind != SyntaxKind.CommaToken)
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
            var alias = ParseOptionalAlias();
            return new ExpressionSelectColumnSyntax(_syntaxTree, expression, alias);
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
            var asKeyword = NextTokenIf(SyntaxKind.AsKeyword);
            if (asKeyword == null && Current.Kind != SyntaxKind.IdentifierToken)
                return null;

            var identifier = Match(SyntaxKind.IdentifierToken);
            return new AliasSyntax(_syntaxTree, asKeyword, identifier);
        }
    }
 }