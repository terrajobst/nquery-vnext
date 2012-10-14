using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using NQuery.Language.Symbols;

using Span = Microsoft.VisualStudio.Text.Span;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryQuickInfoSource : IQuickInfoSource
    {
        private readonly INQueryGlyphService _glyphService;
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;
        private readonly IStandardClassificationService _standardClassificationService;
        private readonly INQuerySemanticClassificationService _semanticClassificationService;

        public NQueryQuickInfoSource(INQueryGlyphService glyphService, IClassificationFormatMap classificationFormatMap, IEditorFormatMap editorFormatMap, IStandardClassificationService standardClassificationService, INQuerySemanticClassificationService semanticClassificationService)
        {
            _glyphService = glyphService;
            _classificationFormatMap = classificationFormatMap;
            _editorFormatMap = editorFormatMap;
            _standardClassificationService = standardClassificationService;
            _semanticClassificationService = semanticClassificationService;
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
            if (model.Markup.Nodes.Count == 0)
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
                Source = _glyphService.GetGlyph(glyph)
            };
        }

        private TextBlock GetTextBlock(SymbolMarkup markup)
        {
            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas")
            };
            textBlock.Inlines.AddRange(markup.Nodes.Select(GetInline));
            return textBlock;
        }

        private Inline GetInline(SymbolMarkupNode markupNode)
        {
            switch (markupNode.Kind)
            {
                case SymbolMarkupKind.Keyword:
                    return GetClassifiedText(markupNode.Text, _standardClassificationService.Keyword);
                case SymbolMarkupKind.Punctuation:
                    return GetClassifiedText(markupNode.Text, _standardClassificationService.Operator);
                case SymbolMarkupKind.Whitespace:
                    return GetClassifiedText(markupNode.Text, _standardClassificationService.WhiteSpace);
                case SymbolMarkupKind.TableName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.SchemaTable);
                case SymbolMarkupKind.DerivedTableName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.DerivedTable);
                case SymbolMarkupKind.CommonTableExpressionName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.CommonTableExpression);
                case SymbolMarkupKind.ColumnName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Column);
                case SymbolMarkupKind.VariableName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Variable);
                case SymbolMarkupKind.ParameterName:
                    return GetClassifiedText(markupNode.Text, _standardClassificationService.Identifier); // TODO: Fix this
                case SymbolMarkupKind.FunctionName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Function);
                case SymbolMarkupKind.AggregateName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Aggregate);
                case SymbolMarkupKind.MethodName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Method);
                case SymbolMarkupKind.PropertyName:
                    return GetClassifiedText(markupNode.Text, _semanticClassificationService.Property);
                case SymbolMarkupKind.TypeName:
                    return GetClassifiedText(markupNode.Text, _standardClassificationService.Identifier); // TODO: Fix this
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