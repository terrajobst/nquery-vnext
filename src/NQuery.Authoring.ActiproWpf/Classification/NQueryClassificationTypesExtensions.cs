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
            registry.Register(classificationTypes.Comment, new HighlightingStyle(Colors.Green));
            registry.Register(classificationTypes.Keyword, new HighlightingStyle(Colors.Blue));
            registry.Register(classificationTypes.Punctuation, new HighlightingStyle(Colors.DarkCyan));
            registry.Register(classificationTypes.Identifier, new HighlightingStyle(Colors.Black));
            registry.Register(classificationTypes.StringLiteral, new HighlightingStyle(Colors.Maroon));
            registry.Register(classificationTypes.NumberLiteral, new HighlightingStyle(Colors.Black));
            registry.Register(classificationTypes.SchemaTable, new HighlightingStyle(Colors.DarkBlue));
            registry.Register(classificationTypes.DerivedTable, new HighlightingStyle(Colors.DarkBlue));
            registry.Register(classificationTypes.CommonTableExpression, new HighlightingStyle(Colors.DarkBlue));
            registry.Register(classificationTypes.Column, new HighlightingStyle(Colors.Purple));
            registry.Register(classificationTypes.Method, new HighlightingStyle(Colors.DarkCyan));
            registry.Register(classificationTypes.Property, new HighlightingStyle(Colors.DarkCyan));
            registry.Register(classificationTypes.Function, new HighlightingStyle(Colors.Fuchsia));
            registry.Register(classificationTypes.Aggregate, new HighlightingStyle(Colors.OrangeRed));
            registry.Register(classificationTypes.Operator, new HighlightingStyle(Colors.DarkCyan));
            registry.Register(classificationTypes.Variable, new HighlightingStyle(Colors.DarkCyan));
            registry.Register(classificationTypes.Unnecessary, new HighlightingStyle(Colors.Gray));
        }
    }
}