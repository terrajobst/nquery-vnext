using System.CodeDom.Compiler;

using NQuery.Authoring;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public static class TestGenerationExtensions
    {
        public static async Task<string> GenerateParserTest(this DocumentView documentView)
        {
            var document = documentView.Document;
            var textSpan = documentView.Selection;

            if (textSpan.Length == 0)
                textSpan = new TextSpan(0, document.Text.Length);

            var syntaxTree = await document.GetSyntaxTreeAsync();
            var node = syntaxTree.Root.DescendantNodes()
                                      .Last(n => n.Span.Contains(textSpan));
            var nodeOrTokens = syntaxTree.Root.Root.DescendantNodesAndTokensAndSelf(true)
                                                   .Where(n => textSpan.Contains(n.Span));

            var isExpression = node is ExpressionSyntax;

            string text;

            if (node.Span == textSpan)
            {
                text = document.Text.GetText(node.Span);
            }
            else if (syntaxTree.Root.Root.Span == textSpan)
            {
                text = document.Text.GetText();
            }
            else
            {
                text = document.Text.WithChanges(
                    TextChange.ForInsertion(textSpan.Start, "{"),
                    TextChange.ForInsertion(textSpan.End, "}")
                ).GetText();
            }

            using (var stringWriter = new StringWriter())
            {
                using (var writer = new IndentedTextWriter(stringWriter))
                {
                    writer.Indent = 2;

                    var testName = node.GetType().Name.Substring(0, node.GetType().Name.Length - "Syntax".Length);

                    writer.WriteLine("[Fact]");
                    writer.WriteLine("public void Parser_Parse_{0}()", testName);
                    writer.WriteLine("{");

                    writer.Indent++;

                    writer.WriteLine("const string text = @\"");

                    using (var stringReader = new StringReader(text))
                    {
                        writer.Indent++;

                        string line;
                        while ((line = stringReader.ReadLine()) != null)
                        {
                            writer.WriteLine(line.Replace("\"", "\"\""));
                        }

                        writer.Indent--;
                    }

                    var method = isExpression ? "ForExpression" : "ForQuery";

                    writer.WriteLine("\";");
                    writer.WriteLine();
                    writer.WriteLine("using (var enumerator = AssertingEnumerator.{0}(text))", method);
                    writer.WriteLine("{");

                    writer.Indent++;

                    foreach (var nodesOrToken in nodeOrTokens)
                    {
                        if (nodesOrToken.IsNode)
                        {
                            var missingModifier = nodesOrToken.IsMissing ? "Missing" : "";
                            writer.WriteLine($"enumerator.AssertNode{missingModifier}(SyntaxKind.{nodesOrToken.Kind});");
                        }
                        else
                        {
                            var token = nodesOrToken.AsToken();
                            var tokenText = token.Text.Replace("\"", "\"\"");

                            if (token.IsMissing)
                            {
                                writer.WriteLine($"enumerator.AssertTokenMissing(SyntaxKind.{nodesOrToken.Kind});");
                            }
                            else
                            {
                                writer.WriteLine($"enumerator.AssertToken(SyntaxKind.{nodesOrToken.Kind}, @\"{tokenText}\");");
                            }

                            foreach (var diagnostic in token.Diagnostics)
                            {
                                writer.WriteLine($"enumerator.AssertDiagnostic(DiagnosticId.{diagnostic.DiagnosticId}, @\"{diagnostic.Message}\");");
                            }
                        }
                    }

                    writer.Indent--;
                    writer.WriteLine("}");

                    writer.Indent--;
                    writer.WriteLine("}");
                }

                return stringWriter.ToString();
            }
        }
    }
}