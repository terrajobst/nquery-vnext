using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;

using NQuery.Text;

namespace NQuery.Tests.Legacy
{
    partial class LegacyTests
    {
        private sealed class TestDefinition
        {
            public TestDefinition(string commandText, string expectedRuntimeError, ImmutableArray<Diagnostic> expectedDiagnostics, DataTable expectedResults)
            {
                CommandText = commandText;
                ExpectedRuntimeError = expectedRuntimeError;
                ExpectedDiagnostics = expectedDiagnostics;
                ExpectedResults = expectedResults;
            }

            public string CommandText { get; private set; }

            public string ExpectedRuntimeError { get; private set; }

            public ImmutableArray<Diagnostic> ExpectedDiagnostics { get; private set; }

            public DataTable ExpectedResults { get; private set; }

            public static TestDefinition FromResource(string name)
            {
                var resourceName = GetFullName(name);
                var assembly = typeof(TestDefinition).Assembly;
                using (var testDefinitionStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (testDefinitionStream == null)
                        return null;

                    var testDefinitionXml = new XmlDocument();
                    testDefinitionXml.Load(testDefinitionStream);
                    return FromXml(testDefinitionXml);
                }
            }

            public static IEnumerable<string> GetResourceNames()
            {
                var assembly = typeof(TestDefinition).Assembly;
                return assembly.GetManifestResourceNames()
                               .Where(n => n.StartsWith("NQuery.Tests.Legacy."))
                               .Select(GetSimpleName);
            }

            private static string GetSimpleName(string resourceName)
            {
                return resourceName.Replace("NQuery.Tests.Legacy.", string.Empty)
                                   .Replace(".xml", string.Empty);
            }

            private static string GetFullName(string resourceName)
            {
                return "NQuery.Tests.Legacy." + resourceName + ".xml";
            }

            public static TestDefinition FromXml(XmlDocument xmlDocument)
            {
                var commandText = xmlDocument.SelectSingleNode("/test/sql").InnerText;

                var expectedRuntimeErrorNode = xmlDocument.SelectSingleNode("/test/expectedRuntimeError");
                var expectedRuntimeError = expectedRuntimeErrorNode == null ? null : expectedRuntimeErrorNode.InnerText;

                var errorList = new List<Diagnostic>();
                var expectedErrorsNode = xmlDocument.SelectSingleNode("/test/expectedErrors");
                if (expectedErrorsNode != null)
                {
                    foreach (XmlNode expectedErrorNode in expectedErrorsNode.SelectNodes("expectedError"))
                    {
                        var diagnosticId = (DiagnosticId)Enum.Parse(typeof(DiagnosticId), expectedErrorNode.Attributes["id"].Value);
                        var errorText = expectedErrorNode.Attributes["text"].Value;
                        var compilationError = new Diagnostic(new TextSpan(), diagnosticId, errorText);
                        errorList.Add(compilationError);
                    }
                }

                DataTable expectedResults = null;
                var expectedResultsNode = xmlDocument.SelectSingleNode("/test/expectedResults");

                if (expectedResultsNode != null)
                {
                    using (var stringReader = new StringReader(expectedResultsNode.InnerXml))
                    {
                        var dataSet = new DataSet();
                        dataSet.ReadXml(stringReader);
                        expectedResults = dataSet.Tables[0];
                    }
                }

                //XmlNode expectedPlanNode = xmlDocument.SelectSingleNode("/test/expectedPlan");
                //if (expectedPlanNode != null)
                //{
                //    result.ExpectedPlan = ShowPlan.FromXml(expectedPlanNode);
                //}

                return new TestDefinition(commandText, expectedRuntimeError, errorList.ToImmutableArray(), expectedResults);
            }
        }
    }
}