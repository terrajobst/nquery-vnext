using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NQuery.Language.Binding;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
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

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.BinaryExpression; }
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
}