using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticClassificationMetadata
    {
        public const string SchemaTableClassificationFormatName = "NQuery.SchemaTable.Format";
        public const string SchemaTableClassificationTypeName = "NQuery.SchemaTable";

        public const string DerivedTableClassificationFormatName = "NQuery.DerivedTable.Format";
        public const string DerivedTableClassificationTypeName = "NQuery.DerivedTable";

        public const string CteTableClassificationFormatName = "NQuery.CteTable.Format";
        public const string CteTableClassificationTypeName = "NQuery.CteTable";

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

        public const string OperatorClassificationFormatName = "NQuery.Operator.Format";
        public const string OperatorClassificationTypeName = "NQuery.Operator";

        public const string ParameterClassificationFormatName = "NQuery.Parameter.Format";
        public const string ParameterClassificationTypeName = "NQuery.Parameter";

        // Types ------------------

#pragma warning disable 649

        [Export]
        [Name(SchemaTableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ClassType;

        [Export]
        [Name(DerivedTableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition DelegateType;

        [Export]
        [Name(CteTableClassificationTypeName)]
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
        [Name(OperatorClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition OperatorType;

        [Export]
        [Name(ParameterClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ParameterType;

#pragma warning restore 649

        // Formats ----------------

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
                ForegroundColor = Color.FromRgb(0, 0, 139);
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
                ForegroundColor = Color.FromRgb(0, 0, 139);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(CteTableClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = CteTableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class CteTableFormat : ClassificationFormatDefinition
        {
            public CteTableFormat()
            {
                DisplayName = "CTE Table";
                ForegroundColor = Color.FromRgb(0, 0, 139);
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
                ForegroundColor = Color.FromRgb(255, 0, 255);
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
                ForegroundColor = Color.FromRgb(128, 0, 128);
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
                ForegroundColor = Color.FromRgb(0, 139, 139);
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
                ForegroundColor = Color.FromRgb(0, 0, 0);
                IsBold = true;
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
                ForegroundColor = Color.FromRgb(0, 139, 139);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(OperatorClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = OperatorClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class OperatorFormat : ClassificationFormatDefinition
        {
            public OperatorFormat()
            {
                DisplayName = "Operator";
                ForegroundColor = Color.FromRgb(0, 139, 139);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ParameterClassificationFormatName)]
        [ClassificationType(ClassificationTypeNames = ParameterClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.Identifier)]
        [Order(After = PredefinedClassificationTypeNames.Keyword)]
        public sealed class ParameterFormat : ClassificationFormatDefinition
        {
            public ParameterFormat()
            {
                DisplayName = "Parameter";
                ForegroundColor = Color.FromRgb(0, 0, 0);
            }
        }
    }
}