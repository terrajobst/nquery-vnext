using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQuerySemanticClassificationMetadata
    {
        public const string PunctuationClassificationFormatName = "NQuery.Punctuation.Format";
        public const string PunctuationClassificationTypeName = "NQuery.Punctuation";

        public const string SchemaTableClassificationFormatName = "NQuery.SchemaTable.Format";
        public const string SchemaTableClassificationTypeName = "NQuery.SchemaTable";

        public const string DerivedTableClassificationFormatName = "NQuery.DerivedTable.Format";
        public const string DerivedTableClassificationTypeName = "NQuery.DerivedTable";

        public const string CommonTableExpressionClassificationFormatName = "NQuery.CommonTableExpression.Format";
        public const string CommonTableExpressionClassificationTypeName = "NQuery.CommonTableExpression";

        public const string ColumnClassificationFormatName = "NQuery.Column.Format";
        public const string ColumnClassificationTypeName = "NQuery.Column";

        public const string MethodClassificationFormatName = "NQuery.Method.Format";
        public const string MethodClassificationTypeName = "NQuery.Method";

        public const string PropertyClassificationFormatName = "NQuery.Property.Format";
        public const string PropertyClassificationTypeName = "NQuery.Property";

        public const string FunctionClassificationFormatName = "NQuery.Function.Format";
        public const string FunctionClassificationTypeName = "NQuery.Function";

        public const string AggregateClassificationFormatName = "NQuery.Aggregate.Format";
        public const string AggregateClassificationTypeName = "NQuery.Aggregate";

        public const string VariableClassificationFormatName = "NQuery.Variable.Format";
        public const string VariableClassificationTypeName = "NQuery.Variable";

        public const string UnnecessaryClassificationFormatName = "NQuery.Unnecessary.Format";
        public const string UnnecessaryClassificationTypeName = "NQuery.Unnecessary";

        // Types ------------------

#pragma warning disable 649

        [Export]
        [Name(PunctuationClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType;

        [Export]
        [Name(SchemaTableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ClassType;

        [Export]
        [Name(DerivedTableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition DelegateType;

        [Export]
        [Name(CommonTableExpressionClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition EnumType;

        [Export]
        [Name(FunctionClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition EventType;

        [Export]
        [Name(ColumnClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FieldType;

        [Export]
        [Name(MethodClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition MethodType;

        [Export]
        [Name(AggregateClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition MutableLocalVariableType;

        [Export]
        [Name(PropertyClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition NamespaceType;

        [Export]
        [Name(VariableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition VariableType;

        [Export]
        [Name(UnnecessaryClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition UnnecessaryType;

#pragma warning restore 649

        // Formats ----------------

        [Export(typeof(EditorFormatDefinition))]
        [Name(PunctuationClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = PunctuationClassificationTypeName)]
        [UserVisible(true)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            public PunctuationFormat()
            {
                DisplayName = "Punctuation";
                ForegroundColor = Colors.DarkCyan;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(SchemaTableClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = SchemaTableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class SchemaTableFormat : ClassificationFormatDefinition
        {
            public SchemaTableFormat()
            {
                DisplayName = "Schema Table";
                ForegroundColor = Colors.DarkBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(DerivedTableClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = DerivedTableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class DerivedTableFormat : ClassificationFormatDefinition
        {
            public DerivedTableFormat()
            {
                DisplayName = "Derived Table";
                ForegroundColor = Colors.DarkBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(CommonTableExpressionClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = CommonTableExpressionClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class CommonTableExpressionFormat : ClassificationFormatDefinition
        {
            public CommonTableExpressionFormat()
            {
                DisplayName = "Common Table Expression";
                ForegroundColor = Colors.DarkBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(FunctionClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = FunctionClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class FunctionFormat : ClassificationFormatDefinition
        {
            public FunctionFormat()
            {
                DisplayName = "Event";
                ForegroundColor = Colors.Fuchsia;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ColumnClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = ColumnClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class ColumnFormat : ClassificationFormatDefinition
        {
            public ColumnFormat()
            {
                DisplayName = "Column";
                ForegroundColor = Colors.Purple;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(MethodClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = MethodClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class MethodFormat : ClassificationFormatDefinition
        {
            public MethodFormat()
            {
                DisplayName = "Method";
                ForegroundColor = Colors.DarkCyan;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(AggregateClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = AggregateClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class AggregateFormat : ClassificationFormatDefinition
        {
            public AggregateFormat()
            {
                DisplayName = "Aggregate Function";
                ForegroundColor = Colors.OrangeRed;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(PropertyClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = PropertyClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class PropertyFormat : ClassificationFormatDefinition
        {
            public PropertyFormat()
            {
                DisplayName = "Property";
                ForegroundColor = Colors.DarkCyan;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(VariableClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = VariableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class VariableFormat : ClassificationFormatDefinition
        {
            public VariableFormat()
            {
                DisplayName = "Variable";
                ForegroundColor = Colors.DarkCyan;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(UnnecessaryClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = UnnecessaryClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class UnnecessaryFormat : ClassificationFormatDefinition
        {
            public UnnecessaryFormat()
            {
                DisplayName = "Unnecessary";
                ForegroundOpacity = 0.6;
            }
        }
    }
}