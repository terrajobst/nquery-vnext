using System;

using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions
{
    internal static class SyntaxTreeExtensions
    {
        public static SyntaxTree ReplaceText(this SyntaxTree syntaxTree, TextSpan span, string text)
        {
            var currentText = syntaxTree.TextBuffer.Text;
            var prefix = currentText.Substring(0, span.Start);
            var suffix = currentText.Substring(span.End);
            var newText = prefix + text + suffix;

            var isQuery = syntaxTree.Root.Root is QuerySyntax;
            return isQuery
                ? SyntaxTree.ParseQuery(newText)
                : SyntaxTree.ParseExpression(newText);
        }

        public static SyntaxTree InsertText(this SyntaxTree syntaxTree, int position, string text)
        {
            return syntaxTree.ReplaceText(new TextSpan(position, 0), text);
        }

        public static SyntaxTree RemoveText(this SyntaxTree syntaxTree, TextSpan span)
        {
            return syntaxTree.ReplaceText(span, string.Empty);
        }
    }
}