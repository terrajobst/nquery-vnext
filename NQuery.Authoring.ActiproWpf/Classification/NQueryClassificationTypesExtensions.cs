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
            registry.Register(classificationTypes.Punctuation, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 139, 139))));
            registry.Register(classificationTypes.Identifier, new HighlightingStyle(Brushes.Black));
            registry.Register(classificationTypes.StringLiteral, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(163, 21, 23))));
            registry.Register(classificationTypes.NumberLiteral, new HighlightingStyle(Brushes.Black));
            registry.Register(classificationTypes.SchemaTable, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 0, 139))));
            registry.Register(classificationTypes.DerivedTable, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 0, 139))));
            registry.Register(classificationTypes.CommonTableExpression, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 0, 139))));
            registry.Register(classificationTypes.Column, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(128, 0, 128))));
            registry.Register(classificationTypes.Method, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 139, 139))));
            registry.Register(classificationTypes.Property, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 139, 139))));
            registry.Register(classificationTypes.Function, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(255, 0, 255))));
            registry.Register(classificationTypes.Aggregate, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(255, 0, 255)), null, true, null, HighlightingStyleLineStyle.None));
            registry.Register(classificationTypes.Operator, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 139, 139))));
            registry.Register(classificationTypes.Variable, new HighlightingStyle(new SolidColorBrush(Color.FromRgb(0, 139, 139))));
        }
    }
}