using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.VSEditorWpf.Classification;
using NQuery.Authoring.Wpf;
using NQuery.Symbols;

using Span = Microsoft.VisualStudio.Text.Span;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    internal sealed class NQueryQuickInfoSource : IQuickInfoSource
    {
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;
        private readonly INQueryClassificationService _classificationService;

        public NQueryQuickInfoSource(IClassificationFormatMap classificationFormatMap, IEditorFormatMap editorFormatMap, INQueryClassificationService classificationService)
        {
            _classificationFormatMap = classificationFormatMap;
            _editorFormatMap = editorFormatMap;
            _classificationService = classificationService;
        }

        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            IQuickInfoManager quickInfoManager;
            if (!session.Properties.TryGetProperty(typeof(IQuickInfoManager), out quickInfoManager))
                return;

            var model = quickInfoManager.Model;
            var textSpan = model.Span;
            var span = new Span(textSpan.Start, textSpan.Length);
            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;
            var content = GetContent(model);
            if (content == null)
                return;

            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(content);
        }

        private FrameworkElement GetContent(QuickInfoModel model)
        {
            if (model.Markup.Tokens.Count == 0)
                return null;

            var glyph = GetGlyph(model.Glyph);
            var textBlock = GetTextBlock(model.Markup);
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(glyph);
            stackPanel.Children.Add(textBlock);
            return stackPanel;
        }

        private Image GetGlyph(NQueryGlyph glyph)
        {
            return new Image
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Source = NQueryGlyphImageSource.Get(glyph)
            };
        }

        private TextBlock GetTextBlock(SymbolMarkup markup)
        {
            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas")
            };
            textBlock.Inlines.AddRange(markup.Tokens.Select(GetInline));
            return textBlock;
        }

        private Inline GetInline(SymbolMarkupToken markupToken)
        {
            switch (markupToken.Kind)
            {
                case SymbolMarkupKind.Keyword:
                    return GetClassifiedText(markupToken.Text, _classificationService.Keyword);
                case SymbolMarkupKind.Punctuation:
                    return GetClassifiedText(markupToken.Text, _classificationService.Punctuation);
                case SymbolMarkupKind.Whitespace:
                    return GetClassifiedText(markupToken.Text, _classificationService.WhiteSpace);
                case SymbolMarkupKind.TableName:
                    return GetClassifiedText(markupToken.Text, _classificationService.SchemaTable);
                case SymbolMarkupKind.CommonTableExpressionName:
                    return GetClassifiedText(markupToken.Text, _classificationService.CommonTableExpression);
                case SymbolMarkupKind.ColumnName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Column);
                case SymbolMarkupKind.VariableName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Variable);
                case SymbolMarkupKind.ParameterName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Identifier); // TODO: Fix this
                case SymbolMarkupKind.FunctionName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Function);
                case SymbolMarkupKind.AggregateName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Aggregate);
                case SymbolMarkupKind.MethodName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Method);
                case SymbolMarkupKind.PropertyName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Property);
                case SymbolMarkupKind.TypeName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Identifier); // TODO: Fix this
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Inline GetClassifiedText(string text, IClassificationType classificationType)
        {
            var properties = _classificationFormatMap.GetTextProperties(classificationType);
            var editorFormatMapKey = _classificationFormatMap.GetEditorFormatMapKey(classificationType);
            var resourceDictionary = _editorFormatMap.GetProperties(editorFormatMapKey);

            var isItalicValue = resourceDictionary[ClassificationFormatDefinition.IsItalicId];
            var fontStyle = isItalicValue != null && Convert.ToBoolean(isItalicValue)
                                ? FontStyles.Italic
                                : FontStyles.Normal;

            var isBoldValue = resourceDictionary[ClassificationFormatDefinition.IsBoldId];
            var fontWeights = isBoldValue != null && Convert.ToBoolean(isBoldValue)
                                  ? FontWeights.Bold
                                  : FontWeights.Normal;

            return new Run(text)
                       {
                           Foreground = properties.ForegroundBrush,
                           Background = properties.BackgroundBrush,
                           FontStyle = fontStyle,
                           FontWeight = fontWeights,
                           TextEffects = properties.TextEffects,
                           TextDecorations = properties.TextDecorations
                       };
        }
    }
}