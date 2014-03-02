using System;
using System.Windows.Media;

using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    public static class NQueryClassificationTypesExtensions
    {
        public static void RegisterAll(this INQueryClassificationTypes classificationTypes)
        {
            classificationTypes.RegisterAll(AmbientHighlightingStyleRegistry.Instance);
        }

        public static void RegisterAll(this INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry registry)
        {
            registry.Register(classificationTypes.WhiteSpace, new HighlightingStyle());
            registry.Register(classificationTypes.Comment, new HighlightingStyle(Brushes.Green));
            registry.Register(classificationTypes.Keyword, new HighlightingStyle(Brushes.Blue));
            registry.Register(classificationTypes.Punctuation, new HighlightingStyle(new SolidColorBrush(Colors.DarkCyan)));
            registry.Register(classificationTypes.Identifier, new HighlightingStyle(Brushes.Black));
            registry.Register(classificationTypes.StringLiteral, new HighlightingStyle(new SolidColorBrush(Colors.Maroon)));
            registry.Register(classificationTypes.NumberLiteral, new HighlightingStyle(Brushes.Black));
            registry.Register(classificationTypes.SchemaTable, new HighlightingStyle(new SolidColorBrush(Colors.DarkBlue)));
            registry.Register(classificationTypes.DerivedTable, new HighlightingStyle(new SolidColorBrush(Colors.DarkBlue)));
            registry.Register(classificationTypes.CommonTableExpression, new HighlightingStyle(new SolidColorBrush(Colors.DarkBlue)));
            registry.Register(classificationTypes.Column, new HighlightingStyle(new SolidColorBrush(Colors.Purple)));
            registry.Register(classificationTypes.Method, new HighlightingStyle(new SolidColorBrush(Colors.DarkCyan)));
            registry.Register(classificationTypes.Property, new HighlightingStyle(new SolidColorBrush(Colors.DarkCyan)));
            registry.Register(classificationTypes.Function, new HighlightingStyle(new SolidColorBrush(Colors.Fuchsia)));
            registry.Register(classificationTypes.Aggregate, new HighlightingStyle(new SolidColorBrush(Colors.OrangeRed)));
            registry.Register(classificationTypes.Operator, new HighlightingStyle(new SolidColorBrush(Colors.DarkCyan)));
            registry.Register(classificationTypes.Variable, new HighlightingStyle(new SolidColorBrush(Colors.DarkCyan)));
            registry.Register(classificationTypes.Unnecessary, new HighlightingStyle(new SolidColorBrush(Colors.Gray)));
        }
    }
}