using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQueryViewer.Syntax
{
    #region Expression

    public abstract class ExpressionSyntax : SyntaxNode
    {
    }

    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _token;
        private readonly object _value;

        public LiteralExpressionSyntax(SyntaxToken token, object value)
        {
            _token = token;
            _value = value;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LiteralExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _token;
        }

        public SyntaxToken Token
        {
            get { return _token; }
        }

        public object Value
        {
            get { return _value; }
        }
    }

    public sealed class CoalesceExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _coalesceKeyword;
        private readonly ArgumentListSyntax _arguments;

        public CoalesceExpressionSyntax(SyntaxToken coalesceKeyword, ArgumentListSyntax arguments)
        {
            _coalesceKeyword = coalesceKeyword;
            _arguments = arguments;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CoalesceExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _coalesceKeyword;
            yield return _arguments;
        }

        public SyntaxToken CoalesceKeyword
        {
            get { return _coalesceKeyword; }
        }

        public ArgumentListSyntax Arguments
        {
            get { return _arguments; }
        }
    }

    public sealed class CaseExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _caseKeyword;
        private readonly ExpressionSyntax _inputExpression;
        private readonly ReadOnlyCollection<CaseLabelSyntax> _caseLabels;
        private readonly SyntaxToken? _elseKeyword;
        private readonly ExpressionSyntax _elseExpression;
        private readonly SyntaxToken _endKeyword;

        public CaseExpressionSyntax(SyntaxToken caseKeyword, ExpressionSyntax inputExpression, IList<CaseLabelSyntax> caseLabels, SyntaxToken? elseKeyword, ExpressionSyntax elseExpression, SyntaxToken endKeyword)
        {
            _caseKeyword = caseKeyword;
            _inputExpression = inputExpression;
            _caseLabels = new ReadOnlyCollection<CaseLabelSyntax>(caseLabels);
            _elseKeyword = elseKeyword;
            _elseExpression = elseExpression;
            _endKeyword = endKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _caseKeyword;
            yield return _inputExpression;

            foreach (var caseLabel in _caseLabels)
                yield return caseLabel;

            if (_elseKeyword != null)
                yield return _elseKeyword.Value;

            yield return _elseExpression;
            yield return _endKeyword;
        }

        public SyntaxToken CaseKeyword
        {
            get { return _caseKeyword; }
        }

        public ExpressionSyntax InputExpression
        {
            get { return _inputExpression; }
        }

        public ReadOnlyCollection<CaseLabelSyntax> CaseLabels
        {
            get { return _caseLabels; }
        }

        public SyntaxToken? ElseKeyword
        {
            get { return _elseKeyword; }
        }

        public ExpressionSyntax ElseExpression
        {
            get { return _elseExpression; }
        }

        public SyntaxToken EndKeyword
        {
            get { return _endKeyword; }
        }
    }

    public sealed class CaseLabelSyntax : SyntaxNode
    {
        private readonly SyntaxToken _whenKeyword;
        private readonly ExpressionSyntax _whenExpression;
        private readonly SyntaxToken _thenKeyword;
        private readonly ExpressionSyntax _thenExpression;

        public CaseLabelSyntax(SyntaxToken whenKeyword, ExpressionSyntax whenExpression, SyntaxToken thenKeyword, ExpressionSyntax thenExpression)
        {
            _whenKeyword = whenKeyword;
            _whenExpression = whenExpression;
            _thenKeyword = thenKeyword;
            _thenExpression = thenExpression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseLabel; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _whenKeyword;
            yield return _whenExpression;
            yield return _thenKeyword;
            yield return _thenExpression;
        }

        public SyntaxToken WhenKeyword
        {
            get { return _whenKeyword; }
        }

        public ExpressionSyntax WhenExpression
        {
            get { return _whenExpression; }
        }

        public SyntaxToken ThenKeyword
        {
            get { return _thenKeyword; }
        }

        public ExpressionSyntax ThenExpression
        {
            get { return _thenExpression; }
        }
    }

    public sealed class CastExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _castKeyword;
        private readonly SyntaxToken _leftParenthesesToken;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _asKeyword;
        private readonly TypeReferenceSyntax _typeReference;
        private readonly SyntaxToken _rightParenthesesToken;

        public CastExpressionSyntax(SyntaxToken castKeyword, SyntaxToken leftParenthesesToken, ExpressionSyntax expression, SyntaxToken asKeyword, TypeReferenceSyntax typeReference, SyntaxToken rightParenthesesToken)
        {
            _castKeyword = castKeyword;
            _leftParenthesesToken = leftParenthesesToken;
            _expression = expression;
            _asKeyword = asKeyword;
            _typeReference = typeReference;
            _rightParenthesesToken = rightParenthesesToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CastExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _castKeyword;
            yield return _leftParenthesesToken;
            yield return _expression;
            yield return _asKeyword;
            yield return _typeReference;
            yield return _rightParenthesesToken;
        }
    }

    public sealed class ParameterExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _atToken;
        private readonly SyntaxToken _name;

        public ParameterExpressionSyntax(SyntaxToken atToken, SyntaxToken name)
        {
            _atToken = atToken;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParameterExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _atToken;
            yield return _name;
        }

        public SyntaxToken AtToken
        {
            get { return _atToken; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }

    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _rightParentheses;

        public ParenthesizedExpressionSyntax(SyntaxToken leftParentheses, ExpressionSyntax expression, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _expression = expression;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _expression;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly ExpressionSyntax _right;

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            _left = left;
            _operatorToken = operatorToken;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetBinaryOperatorExpression(_operatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            yield return _operatorToken;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }

    public sealed class InExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _inKeyword;
        private readonly ArgumentListSyntax _argumentList;

        public InExpressionSyntax(ExpressionSyntax expression, SyntaxToken? notKeyword, SyntaxToken inKeyword, ArgumentListSyntax argumentList)
        {
            _expression = expression;
            _notKeyword = notKeyword;
            _inKeyword = inKeyword;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _inKeyword;
            yield return _argumentList;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken InKeyword
        {
            get { return _inKeyword; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }

    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _operatorToken;
        private readonly ExpressionSyntax _expression;

        public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax expression)
        {
            _operatorToken = operatorToken;
            _expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetUnaryOperatorExpression(_operatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _operatorToken;
            yield return _expression;
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }
    }

    public sealed class BetweenExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _betweenKeyword;
        private readonly ExpressionSyntax _lowerBound;
        private readonly SyntaxToken _andKeyword;
        private readonly ExpressionSyntax _upperBound;

        public BetweenExpressionSyntax(ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken betweenKeyword, ExpressionSyntax lowerBound, SyntaxToken andKeyword, ExpressionSyntax upperBound)
        {
            _left = left;
            _notKeyword = notKeyword;
            _betweenKeyword = betweenKeyword;
            _lowerBound = lowerBound;
            _andKeyword = andKeyword;
            _upperBound = upperBound;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.BetweenExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _betweenKeyword;
            yield return _lowerBound;
            yield return _andKeyword;
            yield return _upperBound;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken BetweenKeyword
        {
            get { return _betweenKeyword; }
        }

        public ExpressionSyntax LowerBound
        {
            get { return _lowerBound; }
        }

        public SyntaxToken AndKeyword
        {
            get { return _andKeyword; }
        }

        public ExpressionSyntax UpperBound
        {
            get { return _upperBound; }
        }
    }

    public sealed class NullIfExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _nullIfKeyword;
        private readonly SyntaxToken _leftParenthesesToken;
        private readonly ExpressionSyntax _leftExpression;
        private readonly SyntaxToken _commaToken;
        private readonly ExpressionSyntax _rightExpression;
        private readonly SyntaxToken _rightParenthesesToken;

        public NullIfExpressionSyntax(SyntaxToken nullIfKeyword, SyntaxToken leftParenthesesToken, ExpressionSyntax leftExpression, SyntaxToken commaToken, ExpressionSyntax rightExpression, SyntaxToken rightParenthesesToken)
        {
            _nullIfKeyword = nullIfKeyword;
            _leftParenthesesToken = leftParenthesesToken;
            _leftExpression = leftExpression;
            _commaToken = commaToken;
            _rightExpression = rightExpression;
            _rightParenthesesToken = rightParenthesesToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NullIfExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _nullIfKeyword;
            yield return _leftParenthesesToken;
            yield return _leftExpression;
            yield return _commaToken;
            yield return _rightExpression;
            yield return _rightParenthesesToken;
        }

        public SyntaxToken NullIfKeyword
        {
            get { return _nullIfKeyword; }
        }

        public SyntaxToken LeftParenthesesToken
        {
            get { return _leftParenthesesToken; }
        }

        public ExpressionSyntax LeftExpression
        {
            get { return _leftExpression; }
        }

        public SyntaxToken CommaToken
        {
            get { return _commaToken; }
        }

        public ExpressionSyntax RightExpression
        {
            get { return _rightExpression; }
        }

        public SyntaxToken RightParenthesesToken
        {
            get { return _rightParenthesesToken; }
        }
    }

    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;

        public NameExpressionSyntax(SyntaxToken identifier)
        {
            _identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NameExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Identifier;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }

    public sealed class FunctionInvocationExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly ArgumentListSyntax _arguments;

        public FunctionInvocationExpressionSyntax(SyntaxToken identifier, ArgumentListSyntax arguments)
        {
            _identifier = identifier;
            _arguments = arguments;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FunctionInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            yield return _arguments;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public ArgumentListSyntax Arguments
        {
            get { return _arguments; }
        }
    }

    public sealed class PropertyAccessExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _target;
        private readonly SyntaxToken _dot;
        private readonly SyntaxToken _name;

        public PropertyAccessExpressionSyntax(ExpressionSyntax target, SyntaxToken dot, SyntaxToken name)
        {
            _target = target;
            _dot = dot;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.PropertyAccessExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _target;
            yield return _dot;
            yield return _name;
        }

        public ExpressionSyntax Target
        {
            get { return _target; }
        }

        public SyntaxToken Dot
        {
            get { return _dot; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }

    public sealed class MethodInvocationExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _target;
        private readonly SyntaxToken _dot;
        private readonly SyntaxToken _name;
        private readonly ArgumentListSyntax _argumentList;

        public MethodInvocationExpressionSyntax(ExpressionSyntax target, SyntaxToken dot, SyntaxToken name, ArgumentListSyntax argumentList)
        {
            _target = target;
            _dot = dot;
            _name = name;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.MethodInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _target;
            yield return _dot;
            yield return _name;
            yield return _argumentList;
        }

        public ExpressionSyntax Target
        {
            get { return _target; }
        }

        public SyntaxToken Dot
        {
            get { return _dot; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }

    public sealed class IsNullExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _isToken;
        private readonly SyntaxToken? _notToken;
        private readonly SyntaxToken _nullToken;

        public IsNullExpressionSyntax(ExpressionSyntax expression, SyntaxToken isToken, SyntaxToken? notToken, SyntaxToken nullToken)
        {
            _expression = expression;
            _isToken = isToken;
            _notToken = notToken;
            _nullToken = nullToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IsNullExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            yield return _isToken;
            if (_notToken != null)
                yield return _notToken.Value;
            yield return _nullToken;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken IsToken
        {
            get { return _isToken; }
        }

        public SyntaxToken? NotToken
        {
            get { return _notToken; }
        }

        public SyntaxToken NullToken
        {
            get { return _nullToken; }
        }
    }

    #endregion

    #region SubselectExpressionSyntax

    public abstract class SubselectExpressionSyntax : ExpressionSyntax
    {
    }

    public sealed class ExistsSubselectSyntax : SubselectExpressionSyntax
    {
        private readonly SyntaxToken _existsKeyword;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public ExistsSubselectSyntax(SyntaxToken existsKeyword, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _existsKeyword = existsKeyword;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExistsSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _existsKeyword;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public SyntaxToken ExistsKeyword
        {
            get { return _existsKeyword; }
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public AllAnySubselectSyntax(ExpressionSyntax left, SyntaxToken operatorToken, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _left = left;
            _operatorToken = operatorToken;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.AllAnySubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            yield return _operatorToken;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    public sealed class SingleRowSubselectSyntax : SubselectExpressionSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public SingleRowSubselectSyntax(SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SingleRowSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    #endregion

    #region TableReferenceSyntax

    public abstract class TableReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected TableReferenceSyntax(SyntaxToken? commaToken)
        {
            _commaToken = commaToken;
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }

    public sealed class ParenthesizedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly TableReferenceSyntax _tableReference;
        private readonly SyntaxToken _rightParentheses;

        public ParenthesizedTableReferenceSyntax(SyntaxToken leftParentheses, TableReferenceSyntax tableReference, SyntaxToken rightParentheses, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _leftParentheses = leftParentheses;
            _tableReference = tableReference;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _tableReference;
            yield return _rightParentheses;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public TableReferenceSyntax TableReference
        {
            get { return _tableReference; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    public sealed class DerivedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;
        private readonly SyntaxToken? _asKeyword;
        private readonly SyntaxToken _name;

        public DerivedTableReferenceSyntax(SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses, SyntaxToken? asKeyword, SyntaxToken name, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
            _asKeyword = asKeyword;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.DerivedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
            if (_asKeyword != null)
                yield return _asKeyword.Value;
            yield return _name;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }

        public SyntaxToken? AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }

    public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _tableName;
        private readonly AliasSyntax _alias;

        public NamedTableReferenceSyntax(SyntaxToken tableName, AliasSyntax alias, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _tableName = tableName;
            _alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NamedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _tableName;
            yield return _alias;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken TableName
        {
            get { return _tableName; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }

    public abstract class JoinedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly TableReferenceSyntax _left;
        private readonly TableReferenceSyntax _right;

        protected JoinedTableReferenceSyntax(TableReferenceSyntax left, TableReferenceSyntax right, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _left = left;
            _right = right;
        }

        public TableReferenceSyntax Left
        {
            get { return _left; }
        }

        public TableReferenceSyntax Right
        {
            get { return _right; }
        }
    }

    public sealed class CrossJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _crossKeyword;
        private readonly SyntaxToken _joinKeyword;

        public CrossJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken crossKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _crossKeyword = crossKeyword;
            _joinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CrossJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            yield return _crossKeyword;
            yield return _joinKeyword;
            yield return Right;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }

    public sealed class InnerJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken? _innerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;
        private readonly ExpressionSyntax _condition;

        public InnerJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken? innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _innerKeyword = innerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
            _condition = condition;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InnerJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            if (_innerKeyword != null)
                yield return _innerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return _condition;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }

    public sealed class OuterJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _typeKeyword;
        private readonly SyntaxToken? _outerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;
        private readonly ExpressionSyntax _condition;

        public OuterJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken typeKeyword, SyntaxToken? outerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _typeKeyword = typeKeyword;
            _outerKeyword = outerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
            _condition = condition;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OuterJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            yield return _typeKeyword;
            if (_outerKeyword != null)
                yield return _outerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return _condition;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }

    #endregion

    #region QuerySyntax

    public abstract class QuerySyntax : SyntaxNode
    {
    }

    public sealed class SelectQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _selectKeyword;
        private readonly SyntaxToken? _distinctKeyword;
        private readonly TopClauseSyntax _topClause;
        private readonly ReadOnlyCollection<SelectColumnSyntax> _selectColumns;
        private readonly FromClauseSyntax _fromClause;
        private readonly WhereClauseSyntax _whereClause;
        private readonly GroupByClauseSyntax _groupByClause;
        private readonly HavingClauseSyntax _havingClause;

        public SelectQuerySyntax(SyntaxToken selectKeyword, SyntaxToken? distinctKeyword, TopClauseSyntax topClause, IList<SelectColumnSyntax> selectColumns, FromClauseSyntax fromClause, WhereClauseSyntax whereClause, GroupByClauseSyntax groupByClause, HavingClauseSyntax havingClause)
        {
            _selectKeyword = selectKeyword;
            _distinctKeyword = distinctKeyword;
            _topClause = topClause;
            _selectColumns = new ReadOnlyCollection<SelectColumnSyntax>(selectColumns);
            _fromClause = fromClause;
            _whereClause = whereClause;
            _groupByClause = groupByClause;
            _havingClause = havingClause;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _selectKeyword;
            if (_distinctKeyword != null)
                yield return _distinctKeyword.Value;
            if (_topClause != null)
                yield return _topClause;
            foreach (var selectColumn in _selectColumns)
                yield return selectColumn;
            if (_fromClause != null)
                yield return _fromClause;
            if (_whereClause != null)
                yield return _whereClause;
            if (_groupByClause != null)
                yield return _groupByClause;
            if (_havingClause != null)
                yield return _havingClause;
        }

        public SyntaxToken SelectKeyword
        {
            get { return _selectKeyword; }
        }

        public SyntaxToken? DistinctKeyword
        {
            get { return _distinctKeyword; }
        }

        public TopClauseSyntax TopClause
        {
            get { return _topClause; }
        }

        public ReadOnlyCollection<SelectColumnSyntax> SelectColumns
        {
            get { return _selectColumns; }
        }

        public FromClauseSyntax FromClause
        {
            get { return _fromClause; }
        }

        public WhereClauseSyntax WhereClause
        {
            get { return _whereClause; }
        }

        public GroupByClauseSyntax GroupByClause
        {
            get { return _groupByClause; }
        }

        public HavingClauseSyntax HavingClause
        {
            get { return _havingClause; }
        }
    }

    public sealed class OrderedQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _orderKeyword;
        private readonly SyntaxToken _byKeyword;
        private readonly List<OrderByColumnSyntax> _columns;

        public OrderedQuerySyntax(QuerySyntax query, SyntaxToken orderKeyword, SyntaxToken byKeyword, List<OrderByColumnSyntax> columns)
        {
            _query = query;
            _orderKeyword = orderKeyword;
            _byKeyword = byKeyword;
            _columns = columns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _query;
            yield return _orderKeyword;
            yield return _byKeyword;
            foreach (var column in _columns)
                yield return column;
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken OrderKeyword
        {
            get { return _orderKeyword; }
        }

        public SyntaxToken ByKeyword
        {
            get { return _byKeyword; }
        }

        public List<OrderByColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }

    public sealed class CommonTableExpressionQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _withKeyword;
        private readonly ReadOnlyCollection<CommonTableExpressionSyntax> _commonTableExpressions;
        private readonly QuerySyntax _query;

        public CommonTableExpressionQuerySyntax(SyntaxToken withKeyword, IList<CommonTableExpressionSyntax> commonTableExpressions, QuerySyntax query)
        {
            _withKeyword = withKeyword;
            _query = query;
            _commonTableExpressions = new ReadOnlyCollection<CommonTableExpressionSyntax>(commonTableExpressions);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _withKeyword;

            foreach (var commonTableExpression in _commonTableExpressions)
                yield return commonTableExpression;

            yield return _query;
        }

        public SyntaxToken WithKeyword
        {
            get { return _withKeyword; }
        }

        public ReadOnlyCollection<CommonTableExpressionSyntax> CommonTableExpressions
        {
            get { return _commonTableExpressions; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }
    }

    #endregion

    #region General Elements

    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _columnSelector;
        private readonly SyntaxToken? _modifier;
        private readonly SyntaxToken? _comma;

        public OrderByColumnSyntax(ExpressionSyntax columnSelector, SyntaxToken? modifier, SyntaxToken? comma)
        {
            _columnSelector = columnSelector;
            _modifier = modifier;
            _comma = comma;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _columnSelector;
            if (_modifier != null)
                yield return _modifier.Value;
            if (_comma != null)
                yield return _comma.Value;
        }

        public ExpressionSyntax ColumnSelector
        {
            get { return _columnSelector; }
        }

        public SyntaxToken? Modifier
        {
            get { return _modifier; }
        }

        public SyntaxToken? Comma
        {
            get { return _comma; }
        }
    }

    public sealed class TopClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _topKeyword;
        private readonly SyntaxToken _value;
        private readonly SyntaxToken? _withKeyword;
        private readonly SyntaxToken? _tiesKeyword;

        public TopClauseSyntax(SyntaxToken topKeyword, SyntaxToken value, SyntaxToken? withKeyword, SyntaxToken? tiesKeyword)
        {
            _topKeyword = topKeyword;
            _value = value;
            _withKeyword = withKeyword;
            _tiesKeyword = tiesKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TopClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _topKeyword;
            yield return _value;
            if (_withKeyword != null)
                yield return _withKeyword.Value;
            if (_tiesKeyword != null)
                yield return _tiesKeyword.Value;
        }

        public SyntaxToken TopKeyword
        {
            get { return _topKeyword; }
        }

        public SyntaxToken Value
        {
            get { return _value; }
        }

        public SyntaxToken? WithKeyword
        {
            get { return _withKeyword; }
        }

        public SyntaxToken? TiesKeyword
        {
            get { return _tiesKeyword; }
        }
    }

    public sealed class TypeReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken _token;
        private readonly string _typeName;

        public TypeReferenceSyntax(SyntaxToken token, string typeName)
        {
            _token = token;
            _typeName = typeName;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TypeReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _token;
        }

        public SyntaxToken Token
        {
            get { return _token; }
        }

        public string TypeName
        {
            get { return _typeName; }
        }
    }

    public sealed class CommonTableExpressionSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifer;
        private readonly CommonTableExpressionColumnNameListSyntax _commonTableExpressionColumnNameList;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionSyntax(SyntaxToken identifer, CommonTableExpressionColumnNameListSyntax commonTableExpressionColumnNameList, SyntaxToken asKeyword, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses, SyntaxToken? commaToken)
        {
            _identifer = identifer;
            _commonTableExpressionColumnNameList = commonTableExpressionColumnNameList;
            _asKeyword = asKeyword;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifer;
            yield return _commonTableExpressionColumnNameList;
            yield return _asKeyword;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public SyntaxToken Identifer
        {
            get { return _identifer; }
        }

        public CommonTableExpressionColumnNameListSyntax CommonTableExpressionColumnNameList
        {
            get { return _commonTableExpressionColumnNameList; }
        }

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }

    #endregion

    public sealed class ArgumentSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken? _comma;

        public ArgumentSyntax(ExpressionSyntax expression, SyntaxToken? comma)
        {
            _expression = expression;
            _comma = comma;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.Argument; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;

            if (_comma != null)
                yield return _comma.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken? Comma
        {
            get { return _comma; }
        }
    }

    public sealed class ArgumentListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly IList<ArgumentSyntax> _arguments;
        private readonly SyntaxToken _rightParenthesis;

        public ArgumentListSyntax(SyntaxToken leftParenthesis, IList<ArgumentSyntax> arguments, SyntaxToken rightParenthesis)
        {
            _leftParenthesis = leftParenthesis;
            _arguments = new ReadOnlyCollection<ArgumentSyntax>(arguments);
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ArgumentList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParenthesis;

            foreach (var argument in _arguments)
                yield return argument;

            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public IList<ArgumentSyntax> Arguments
        {
            get { return _arguments; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }

    public sealed class SimilarToExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _similarKeyword;
        private readonly SyntaxToken _toKeyword;
        private readonly ExpressionSyntax _right;

        public SimilarToExpressionSyntax(ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken similarKeyword, SyntaxToken toKeyword, ExpressionSyntax right)
        {
            _left = left;
            _notKeyword = notKeyword;
            _similarKeyword = similarKeyword;
            _toKeyword = toKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SimilarToExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _similarKeyword;
            yield return _toKeyword;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken SimilarKeyword
        {
            get { return _similarKeyword; }
        }

        public SyntaxToken ToKeyword
        {
            get { return _toKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }
   
    public sealed class LikeExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _likeKeyword;
        private readonly ExpressionSyntax _right;

        public LikeExpressionSyntax(ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken likeKeyword, ExpressionSyntax right)
        {
            _left = left;
            _notKeyword = notKeyword;
            _likeKeyword = likeKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _likeKeyword;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken LikeKeyword
        {
            get { return _likeKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }

    public sealed class SoundslikeExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _soundslikeKeyword;
        private readonly ExpressionSyntax _right;

        public SoundslikeExpressionSyntax(ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken soundslikeKeyword, ExpressionSyntax right)
        {
            _left = left;
            _notKeyword = notKeyword;
            _soundslikeKeyword = soundslikeKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _soundslikeKeyword;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken SoundslikeKeyword
        {
            get { return _soundslikeKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }

    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken _leftParentheses;
        private readonly SyntaxToken _asteriskToken;
        private readonly SyntaxToken _rightParentheses;

        public CountAllExpressionSyntax(SyntaxToken identifier, SyntaxToken leftParentheses, SyntaxToken asteriskToken, SyntaxToken rightParentheses)
        {
            _identifier = identifier;
            _leftParentheses = leftParentheses;
            _asteriskToken = asteriskToken;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            yield return _leftParentheses;
            yield return _asteriskToken;
            yield return _rightParentheses;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
    public sealed class CommonTableExpressionColumnNameListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly IList<SyntaxToken> _columnNames;
        private readonly SyntaxToken _rightParentheses;

        public CommonTableExpressionColumnNameListSyntax(SyntaxToken leftParentheses, IList<SyntaxToken> columnNames, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _columnNames = columnNames;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnNameList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            foreach (var columnName in _columnNames)
                yield return columnName;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public IList<SyntaxToken> ColumnNames
        {
            get { return _columnNames; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }

    public sealed class CommonTableExpressionColumnNameSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionColumnNameSyntax(SyntaxToken identifier, SyntaxToken? commaToken)
        {
            _identifier = identifier;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnName; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
    public sealed class ParenthesizedQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public ParenthesizedQuerySyntax(SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
    public sealed class UnionQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _unionKeyword;
        private readonly SyntaxToken? _allKeyword;
        private readonly QuerySyntax _rightQuery;

        public UnionQuerySyntax(QuerySyntax leftQuery, SyntaxToken unionKeyword, SyntaxToken? allKeyword, QuerySyntax rightQuery)
        {
            _leftQuery = leftQuery;
            _unionKeyword = unionKeyword;
            _allKeyword = allKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.UnionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftQuery;
            yield return _unionKeyword;
            if (_allKeyword != null)
                yield return _allKeyword.Value;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken UnionKeyword
        {
            get { return _unionKeyword; }
        }

        public SyntaxToken? AllKeyword
        {
            get { return _allKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
    public sealed class ExceptQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _exceptKeyword;
        private readonly QuerySyntax _rightQuery;

        public ExceptQuerySyntax(QuerySyntax leftQuery, SyntaxToken exceptKeyword, QuerySyntax rightQuery)
        {
            _leftQuery = leftQuery;
            _exceptKeyword = exceptKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExceptQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftQuery;
            yield return _exceptKeyword;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken ExceptKeyword
        {
            get { return _exceptKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
    public sealed class IntersectQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _intersectKeyword;
        private readonly QuerySyntax _rightQuery;

        public IntersectQuerySyntax(QuerySyntax leftQuery, SyntaxToken intersectKeyword, QuerySyntax rightQuery)
        {
            _leftQuery = leftQuery;
            _intersectKeyword = intersectKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IntersectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftQuery;
            yield return _intersectKeyword;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken IntersectKeyword
        {
            get { return _intersectKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
    public sealed class WhereClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _whereKeyword;
        private readonly ExpressionSyntax _predicate;

        public WhereClauseSyntax(SyntaxToken whereKeyword, ExpressionSyntax predicate)
        {
            _whereKeyword = whereKeyword;
            _predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WhereClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _whereKeyword;
            yield return _predicate;
        }

        public SyntaxToken WhereKeyword
        {
            get { return _whereKeyword; }
        }

        public ExpressionSyntax Predicate
        {
            get { return _predicate; }
        }
    }
    public sealed class HavingClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _havingKeyword;
        private readonly ExpressionSyntax _predicate;

        public HavingClauseSyntax(SyntaxToken havingKeyword, ExpressionSyntax predicate)
        {
            _havingKeyword = havingKeyword;
            _predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.HavingClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _havingKeyword;
            yield return _predicate;
        }

        public SyntaxToken HavingKeyword
        {
            get { return _havingKeyword; }
        }

        public ExpressionSyntax Predicate
        {
            get { return _predicate; }
        }
    }
    public abstract class SelectColumnSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected SelectColumnSyntax(SyntaxToken? commaToken)
        {
            _commaToken = commaToken;
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }

    public sealed class ExpressionSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly AliasSyntax _alias;
        private readonly SyntaxToken? _commaToken;

        public ExpressionSelectColumnSyntax(ExpressionSyntax expression, AliasSyntax alias, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _expression = expression;
            _alias = alias;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            if (_alias != null)
                yield return _alias;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }

    public sealed class WildcardSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly SyntaxToken? _tableName;
        private readonly SyntaxToken? _dotToken;
        private readonly SyntaxToken _asteriskToken;

        public WildcardSelectColumnSyntax(SyntaxToken? tableName, SyntaxToken? dotToken, SyntaxToken asteriskToken, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _tableName = tableName;
            _dotToken = dotToken;
            _asteriskToken = asteriskToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WildcardSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            if (_tableName != null)
                yield return _tableName.Value;
            if (_dotToken != null)
                yield return _dotToken.Value;
            yield return _asteriskToken;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken? TableName
        {
            get { return _tableName; }
        }

        public SyntaxToken? DotToken
        {
            get { return _dotToken; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }
    }

    public sealed class AliasSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _asKeyword;
        private readonly SyntaxToken _identifier;

        public AliasSyntax(SyntaxToken? asKeyword, SyntaxToken identifier)
        {
            _asKeyword = asKeyword;
            _identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.Alias; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            if (_asKeyword != null)
                yield return _asKeyword.Value;
            yield return _identifier;
        }

        public SyntaxToken? AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }

    public sealed class FromClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _fromKeyword;
        private readonly ReadOnlyCollection<TableReferenceSyntax> _tableReferences;

        public FromClauseSyntax(SyntaxToken fromKeyword, IList<TableReferenceSyntax> tableReferences)
        {
            _fromKeyword = fromKeyword;
            _tableReferences = new ReadOnlyCollection<TableReferenceSyntax>(tableReferences);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FromClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _fromKeyword;
            foreach (var tableReference in TableReferences)
                yield return tableReference;
        }

        public SyntaxToken FromKeyword
        {
            get { return _fromKeyword; }
        }

        public ReadOnlyCollection<TableReferenceSyntax> TableReferences
        {
            get { return _tableReferences; }
        }
    }
    public sealed class GroupByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken? _comma;

        public GroupByColumnSyntax(ExpressionSyntax expression, SyntaxToken? comma)
        {
            _expression = expression;
            _comma = comma;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            if (_comma != null)
                yield return _comma.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken? Comma
        {
            get { return _comma; }
        }
    }

    public sealed class GroupByClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _groupKeyword;
        private readonly SyntaxToken _byKeyword;
        private readonly ReadOnlyCollection<GroupByColumnSyntax> _columns;

        public GroupByClauseSyntax(SyntaxToken groupKeyword, SyntaxToken byKeyword, IList<GroupByColumnSyntax> columns)
        {
            _groupKeyword = groupKeyword;
            _byKeyword = byKeyword;
            _columns = new ReadOnlyCollection<GroupByColumnSyntax>(columns);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _groupKeyword;
            yield return _byKeyword;
            foreach (var columnSyntax in _columns)
                yield return columnSyntax;
        }

        public SyntaxToken GroupKeyword
        {
            get { return _groupKeyword; }
        }

        public SyntaxToken ByKeyword
        {
            get { return _byKeyword; }
        }

        public ReadOnlyCollection<GroupByColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}