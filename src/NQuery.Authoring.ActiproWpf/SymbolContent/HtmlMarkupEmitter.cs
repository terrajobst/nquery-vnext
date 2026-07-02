using System.Globalization;
using System.Text;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.Rendering;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent;

internal static class HtmlMarkupEmitter
{
    public static string GetHtml(Glyph glyph, SymbolMarkup symbolMarkup, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
    {
        var sb = new StringBuilder();
        sb.AppendGlyph(glyph);
        sb.AppendMarkup(symbolMarkup, classificationTypes, highlightingStyleRegistry);
        return sb.ToString();
    }

    private static IClassificationType GetClassificationType(SymbolMarkupKind kind, INQueryClassificationTypes classificationTypes)
    {
        switch (kind)
        {
            case SymbolMarkupKind.Whitespace:
                return classificationTypes.WhiteSpace;
            case SymbolMarkupKind.Punctuation:
                return classificationTypes.Punctuation;
            case SymbolMarkupKind.Keyword:
                return classificationTypes.Keyword;
            case SymbolMarkupKind.TableName:
                return classificationTypes.SchemaTable;
            case SymbolMarkupKind.CommonTableExpressionName:
                return classificationTypes.CommonTableExpression;
            case SymbolMarkupKind.ColumnName:
                return classificationTypes.Column;
            case SymbolMarkupKind.VariableName:
                return classificationTypes.Variable;
            case SymbolMarkupKind.ParameterName:
                return classificationTypes.Identifier;
            case SymbolMarkupKind.FunctionName:
                return classificationTypes.Function;
            case SymbolMarkupKind.AggregateName:
                return classificationTypes.Aggregate;
            case SymbolMarkupKind.MethodName:
                return classificationTypes.Method;
            case SymbolMarkupKind.PropertyName:
                return classificationTypes.Property;
            case SymbolMarkupKind.TypeName:
                return classificationTypes.Identifier;
            default:
                throw ExceptionBuilder.UnexpectedValue(kind);
        }

    }

    extension(StringBuilder sb)
    {
        private void AppendGlyph(Glyph glyph)
        {
            sb.Append(@"<img src=""");
            sb.Append(glyph);
            sb.Append(@""" align=""absbottom"" />");
        }

        private void AppendMarkup(SymbolMarkup symbolMarkup, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            foreach (var node in symbolMarkup.Tokens)
                sb.AppendNode(node, classificationTypes, highlightingStyleRegistry);
        }

        private void AppendNode(SymbolMarkupToken token, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var classificationType = GetClassificationType(token.Kind, classificationTypes);
            sb.AppendText(token.Text, classificationType, highlightingStyleRegistry);
        }

        private void AppendText(string text, IClassificationType classificationType, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var highlightingStyle = highlightingStyleRegistry[classificationType];
            var styleBuilder = new StringBuilder();
            styleBuilder.AppendStyle(highlightingStyle);
            var hasStyle = styleBuilder.Length > 0;

            if (hasStyle)
            {
                sb.Append(@"<span style=""");
                sb.Append(styleBuilder);
                sb.Append(@""">");
            }

            sb.Append(HtmlContentProvider.Escape(text));

            if (hasStyle)
                sb.Append(@"</span>");
        }

        private void AppendStyle(IHighlightingStyle highlightingStyle)
        {
            sb.AppendColor(@"background-color", highlightingStyle.Background);
            sb.AppendColor(@"color", highlightingStyle.Foreground);
            sb.AppendFontFamily(@"font-family", highlightingStyle.FontFamilyName);
            sb.AppendFontSize(@"font-size", highlightingStyle.FontSize);
            sb.AppendFontWeight(@"font-weight", highlightingStyle.Bold);
            sb.AppendFontStyle(@"font-style", highlightingStyle.Italic);
            sb.AppendTextDecoration(@"text-decoration", highlightingStyle.UnderlineKind);
        }

        private void AppendFontFamily(string key, string fontFamilyName)
        {
            if (string.IsNullOrEmpty(fontFamilyName))
                return;

            sb.AppendKeyValue(key, fontFamilyName);
        }

        private void AppendColor(string key, Color? color)
        {
            if (color is null)
                return;

            var value = color.ToString()!;
            sb.AppendKeyValue(key, value);
        }

        private void AppendFontSize(string key, double size)
        {
            if (size == 0.0)
                return;

            var value = size.ToString(CultureInfo.InvariantCulture);
            sb.AppendKeyValue(key, value);
        }

        private void AppendFontWeight(string key, bool? isBold)
        {
            if (isBold is null)
                return;

            var value = isBold.Value ? @"bold" : @"normal";
            sb.AppendKeyValue(key, value);
        }

        private void AppendFontStyle(string key, bool? isItalic)
        {
            if (isItalic is null)
                return;

            var value = isItalic.Value ? @"italic" : @"normal";
            sb.AppendKeyValue(key, value);
        }

        private void AppendTextDecoration(string key, LineKind underlineKind)
        {
            var hasUnderlineStyle = underlineKind != LineKind.None;

            if (!hasUnderlineStyle)
                return;

            sb.AppendKeyValue(key, @"underline");
        }

        private void AppendKeyValue(string name, string value)
        {
            sb.Append(name);
            sb.Append(@": ");
            sb.Append(value);
            sb.Append(@";");
        }
    }
}
