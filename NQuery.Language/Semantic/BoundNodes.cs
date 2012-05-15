using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace NQuery.Language.Semantic
{
    internal abstract class BoundNode
    {
    }

    internal abstract class BoundExpression : BoundNode
    {
        public abstract Symbol Symbol { get; }
        public abstract IEnumerable<Symbol> Candidates { get; }
        public abstract Type Type { get; }
    }

    internal sealed class BoundNameExpression : BoundExpression
    {
        private readonly Symbol _symbol;
        private readonly IEnumerable<Symbol> _candidates;

        public BoundNameExpression(Symbol symbol, IEnumerable<Symbol> candidates)
        {
            _symbol = symbol;
            _candidates = candidates;
        }

        public override Symbol Symbol
        {
            get { return _symbol; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return _candidates; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }
    }

    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly MethodInfo _methodInfo;

        public BoundUnaryExpression(BoundExpression expression, MethodInfo methodInfo)
        {
            _expression = expression;
            _methodInfo = methodInfo;
        }

        public override Symbol Symbol
        {
            get { return null; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>(); }
        }

        public override Type Type
        {
            get
            {
                return _methodInfo == null
                           ? WellKnownTypes.Unknown
                           : _methodInfo.ReturnType;
            }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }
    }

    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly BoundExpression _left;
        private readonly MethodInfo _methodInfo;
        private readonly BoundExpression _right;

        public BoundBinaryExpression(BoundExpression left, MethodInfo methodInfo, BoundExpression right)
        {
            _left = left;
            _methodInfo = methodInfo;
            _right = right;
        }

        public override Symbol Symbol
        {
            get { return null; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>(); }
        }

        public override Type Type
        {
            get
            {
                return _methodInfo == null
                           ? WellKnownTypes.Unknown
                           : _methodInfo.ReturnType;
            }
        }

        public BoundExpression Left
        {
            get { return _left; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        public BoundExpression Right
        {
            get { return _right; }
        }
    }

    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly object _value;

        public BoundLiteralExpression(object value)
        {
            _value = value;
        }

        public override Symbol Symbol
        {
            get { return null; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>(); }
        }

        public override Type Type
        {
            get
            {
                return _value == null
                           ? typeof(DBNull)
                           : _value.GetType();
            }
        }
    }

    internal abstract class BoundTableReference : BoundNode
    {
        public abstract IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances();
    }

    internal sealed class BoundNamedTableReference : BoundTableReference
    {
        private readonly TableInstanceSymbol _tableInstance;

        public BoundNamedTableReference(TableInstanceSymbol tableInstance)
        {
            _tableInstance = tableInstance;
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return new[] { _tableInstance };
        }
    }

    internal sealed class BoundDerivedTableReference : BoundTableReference
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly BoundQuery _query;

        public BoundDerivedTableReference(TableInstanceSymbol tableInstance, BoundQuery query)
        {
            _tableInstance = tableInstance;
            _query = query;
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundQuery Query
        {
            get { return _query; }
        }

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return new[] { _tableInstance };
        }
    }

    internal enum BoundJoinType
    {
        InnerJoin,
        FullOuterJoin,
        LeftOuterJoin,
        RightOuterJoin
    }

    internal sealed class BoundJoinedTableReference : BoundTableReference
    {
        private readonly BoundTableReference _left;
        private readonly BoundTableReference _right;
        private readonly BoundJoinType _joinType;
        private readonly BoundExpression _condition;

        public BoundJoinedTableReference(BoundJoinType joinType, BoundTableReference left, BoundTableReference right, BoundExpression condition)
        {
            _left = left;
            _right = right;
            _joinType = joinType;
            _condition = condition;
        }

        public BoundTableReference Left
        {
            get { return _left; }
        }

        public BoundTableReference Right
        {
            get { return _right; }
        }

        public BoundJoinType JoinType
        {
            get { return _joinType; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return _left.GetDeclaredTableInstances().Concat(_right.GetDeclaredTableInstances());
        }
    }

    internal abstract class BoundQuery : BoundNode
    {
        public abstract ReadOnlyCollection<BoundSelectColumn> SelectColumns { get; }
    }

    internal sealed class BoundSelectColumn : BoundNode
    {
        private readonly BoundExpression _expression;
        private readonly string _name;

        public BoundSelectColumn(BoundExpression expression, string name)
        {
            _expression = expression;
            _name = name;
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public string Name
        {
            get { return _name; }
        }
    }

    internal sealed class BoundSelectQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<BoundSelectColumn> _selectColumns;
        private readonly BoundTableReference _fromClause;
        private readonly BoundExpression _whereClause;
        private readonly BoundExpression _havingClause;

        public BoundSelectQuery(IList<BoundSelectColumn> selectColumns, BoundTableReference fromClause, BoundExpression whereClause, BoundExpression havingClause)
        {
            _selectColumns = new ReadOnlyCollection<BoundSelectColumn>(selectColumns);
            _fromClause = fromClause;
            _whereClause = whereClause;
            _havingClause = havingClause;
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _selectColumns; }
        }

        public BoundTableReference FromClause
        {
            get { return _fromClause; }
        }

        public BoundExpression WhereClause
        {
            get { return _whereClause; }
        }

        public BoundExpression HavingClause
        {
            get { return _havingClause; }
        }
    }

    internal enum BoundQueryCombinator
    {
        Union,
        UnionAll,
        Except,
        Intersect
    }

    internal sealed class BoundCombinedQuery : BoundQuery
    {
        private readonly BoundQuery _left;
        private readonly BoundQueryCombinator _combinator;
        private readonly BoundQuery _right;

        public BoundCombinedQuery(BoundQuery left, BoundQueryCombinator combinator, BoundQuery right)
        {
            _left = left;
            _combinator = combinator;
            _right = right;
        }

        public BoundQuery Left
        {
            get { return _left; }
        }

        public BoundQueryCombinator Combinator
        {
            get { return _combinator; }
        }

        public BoundQuery Right
        {
            get { return _right; }
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _left.SelectColumns; }
        }
    }
}