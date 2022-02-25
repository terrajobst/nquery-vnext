﻿using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class UnusedCommonTableExpressionCodeIssueProvider : CodeIssueProvider<CommonTableExpressionQuerySyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, CommonTableExpressionQuerySyntax node)
        {
            var nodes = node.DescendantNodes().OfType<NamedTableReferenceSyntax>();
            var referencedTables = nodes.Where(n => !IsRecursiveUsage(semanticModel, n))
                                        .Select(semanticModel.GetDeclaredSymbol)
                                        .Where(s => s != null)
                                        .Select(s => s.Table);
            var referencedTableSet = new HashSet<TableSymbol>(referencedTables);

            return from tableExpression in node.CommonTableExpressions
                   let declaredTable = semanticModel.GetDeclaredSymbol(tableExpression)
                   where declaredTable != null && !referencedTableSet.Contains(declaredTable)
                   let actions = new[] { new RemoveCommonTableExpressionCodeAction(tableExpression) }
                   select new CodeIssue(CodeIssueKind.Unnecessary, tableExpression.Name.Span, actions);
        }

        private static bool IsRecursiveUsage(SemanticModel semanticModel, NamedTableReferenceSyntax tableReference)
        {
            var symbol = semanticModel.GetDeclaredSymbol(tableReference);
            if (symbol == null)
                return false;

            var table = symbol.Table;
            return tableReference.Ancestors().OfType<CommonTableExpressionSyntax>().Any(c => semanticModel.GetDeclaredSymbol(c) == table);
        }

        private sealed class RemoveCommonTableExpressionCodeAction : CodeAction
        {
            private readonly CommonTableExpressionSyntax _node;

            public RemoveCommonTableExpressionCodeAction(CommonTableExpressionSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return Resources.CodeActionRemoveUnusedCommonTableExpression; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var previousToken = _node.FirstToken().GetPreviousToken();
                var nextToken = _node.LastToken().GetNextToken();

                var isFirst = previousToken.Kind == SyntaxKind.WithKeyword;
                var isLast = nextToken.Kind != SyntaxKind.CommaToken;
                var isSingle = isFirst && isLast;

                var removePreviousToken = isLast;
                var removeNextToken = !isLast;

                var start = removePreviousToken ? previousToken.Span.Start : _node.FullSpan.Start;
                var end = removeNextToken
                    ? nextToken.FullSpan.End
                    : isSingle
                        ? nextToken.Span.Start
                        : _node.Span.End;
                var span = TextSpan.FromBounds(start, end);

                changeSet.DeleteText(span);
            }
        }
    }
}