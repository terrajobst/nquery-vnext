namespace NQuery.Tests.Syntax
{
    partial class ParserTests
    {
        public static IEnumerable<object[]> GetContextualKeywords()
        {
            return SyntaxFacts.GetContextualKeywordKinds().Select(k => new object[] { k });
        }

        [Fact]
        public void Parser_Parse_Expression_ArgumentList_WithNoArguments()
        {
            const string text = @"
                TO_INT16{()}
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_ArgumentList_WithSingleArgument()
        {
            const string text = @"
                TO_INT16{('1')}
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'1'");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_ArgumentList_WithMultipleArguments()
        {
            const string text = @"
                TO_INT16{(1, 2)}
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Between()
        {
            const string text = @"
                1 BETWEEN 2 AND 3
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.BetweenExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.BetweenKeyword, @"BETWEEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.AndKeyword, @"AND");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"3");
        }

        [Fact]
        public void Parser_Parse_Expression_Between_WithNotKeyword()
        {
            const string text = @"
                1 NOT BETWEEN 2 AND 3
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.BetweenExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.BetweenKeyword, @"BETWEEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.AndKeyword, @"AND");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"3");
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Binary_Left()
        {
            var betweenPrecedence = SyntaxFacts.GetTernaryOperatorPrecedence(SyntaxKind.BetweenExpression);

            foreach (var op in SyntaxFacts.GetBinaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(opExpressionKind);
                var opText = op.GetText();
                var text = $"x {opText} y BETWEEN a AND b";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                if (betweenPrecedence > opPrecedence)
                {
                    //    op
                    //  x     BETWEEN
                    //       y   a   b

                    using var enumerator = AssertingEnumerator.ForNode(expression);
                    enumerator.AssertNode(opExpressionKind);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                    enumerator.AssertToken(op, opText);
                    enumerator.AssertNode(SyntaxKind.BetweenExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                    enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
                else
                {
                    //    BETWEEN
                    //  op    a   b
                    // x   y

                    using var enumerator = AssertingEnumerator.ForNode(expression);
                    enumerator.AssertNode(SyntaxKind.BetweenExpression);
                    enumerator.AssertNode(opExpressionKind);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                    enumerator.AssertToken(op, opText);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                    enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Binary_Middle()
        {
            var andPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(SyntaxKind.LogicalAndExpression);

            foreach (var op in SyntaxFacts.GetBinaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(opExpressionKind);
                if (opPrecedence <= andPrecedence)
                    continue;

                var opText = op.GetText();
                var text = $"a BETWEEN x {opText} y AND b";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                //  BETWEEN
                // a   op   b
                //    x  y

                using var enumerator = AssertingEnumerator.ForNode(expression);
                enumerator.AssertNode(SyntaxKind.BetweenExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                enumerator.AssertNode(opExpressionKind);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                enumerator.AssertToken(op, opText);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Binary_Right()
        {
            var andPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(SyntaxKind.LogicalAndExpression);

            foreach (var op in SyntaxFacts.GetBinaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(opExpressionKind);
                if (opPrecedence <= andPrecedence)
                    continue;

                var opText = op.GetText();
                var text = $"a BETWEEN b AND x {opText} y";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                //  BETWEEN
                // a   b   op
                //        x  y

                using var enumerator = AssertingEnumerator.ForNode(expression);
                enumerator.AssertNode(SyntaxKind.BetweenExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                enumerator.AssertNode(opExpressionKind);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                enumerator.AssertToken(op, opText);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Unary_Left()
        {
            var betweenPrecedence = SyntaxFacts.GetTernaryOperatorPrecedence(SyntaxKind.BetweenExpression);

            foreach (var op in SyntaxFacts.GetUnaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetUnaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(opExpressionKind);
                var opText = op.GetText();
                var text = $"{opText} x BETWEEN a AND b";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                if (betweenPrecedence > opPrecedence)
                {
                    //     op
                    //   BETWEEN
                    //  x   a   b

                    using var enumerator = AssertingEnumerator.ForNode(expression);
                    enumerator.AssertNode(opExpressionKind);
                    enumerator.AssertToken(op, opText);
                    enumerator.AssertNode(SyntaxKind.BetweenExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                    enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
                else
                {
                    //    BETWEEN
                    //  op    a   b
                    //   x

                    using var enumerator = AssertingEnumerator.ForNode(expression);
                    enumerator.AssertNode(SyntaxKind.BetweenExpression);
                    enumerator.AssertNode(opExpressionKind);
                    enumerator.AssertToken(op, opText);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                    enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Unary_Middle()
        {
            var andPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(SyntaxKind.LogicalAndExpression);

            foreach (var op in SyntaxFacts.GetUnaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetUnaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(opExpressionKind);
                if (opPrecedence <= andPrecedence)
                    continue;

                var opText = op.GetText();
                var text = $"a BETWEEN {opText} x AND b";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                //  BETWEEN
                // a   op   b
                //    x  y

                using var enumerator = AssertingEnumerator.ForNode(expression);
                enumerator.AssertNode(SyntaxKind.BetweenExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                enumerator.AssertNode(opExpressionKind);
                enumerator.AssertToken(op, opText);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Between_HonorsPriorities_Unary_Right()
        {
            var andPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(SyntaxKind.LogicalAndExpression);

            foreach (var op in SyntaxFacts.GetUnaryExpressionTokenKinds())
            {
                var opExpressionKind = SyntaxFacts.GetUnaryOperatorExpression(op);
                var opPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(opExpressionKind);
                if (opPrecedence <= andPrecedence)
                    continue;

                var opText = op.GetText();
                var text = $"a BETWEEN b AND {opText} x";

                var syntaxTree = SyntaxTree.ParseExpression(text);
                Assert.Empty(syntaxTree.GetDiagnostics());

                var expression = syntaxTree.Root.Root;

                //  BETWEEN
                // a   b   op
                //         x

                using var enumerator = AssertingEnumerator.ForNode(expression);
                enumerator.AssertNode(SyntaxKind.BetweenExpression);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                enumerator.AssertToken(SyntaxKind.BetweenKeyword, "BETWEEN");
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                enumerator.AssertToken(SyntaxKind.AndKeyword, "AND");
                enumerator.AssertNode(opExpressionKind);
                enumerator.AssertToken(op, opText);
                enumerator.AssertNode(SyntaxKind.NameExpression);
                enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Binary()
        {
            const string text = @"
                2 * 3
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.MultiplyExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"3");
        }

        [Fact]
        public void Parser_Parse_Expression_Binary_HonorsPriorities()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryExpressionTokenKinds())
            {
                foreach (var op2 in SyntaxFacts.GetBinaryExpressionTokenKinds())
                {
                    var op1Text = op1.GetText();
                    var op2Text = op2.GetText();
                    var text = $"x {op1Text} y {op2Text} z";

                    var syntaxTree = SyntaxTree.ParseExpression(text);
                    Assert.Empty(syntaxTree.GetDiagnostics());

                    var expression = syntaxTree.Root.Root;

                    var op1ExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op1);
                    var op2ExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op2);
                    var op1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op1ExpressionKind);
                    var op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2ExpressionKind);
                    var leftToRight = op1Precedence >= op2Precedence;

                    if (leftToRight)
                    {
                        //     op2
                        //  op1   z
                        // x   y

                        using var enumerator = AssertingEnumerator.ForNode(expression);
                        enumerator.AssertNode(op2ExpressionKind);
                        enumerator.AssertNode(op1ExpressionKind);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                        enumerator.AssertToken(op1, op1Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                        enumerator.AssertToken(op2, op2Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "z");
                    }
                    else
                    {
                        //  op1
                        // x   op2
                        //    y   z

                        using var enumerator = AssertingEnumerator.ForNode(expression);
                        enumerator.AssertNode(op1ExpressionKind);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                        enumerator.AssertToken(op1, op1Text);
                        enumerator.AssertNode(op2ExpressionKind);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                        enumerator.AssertToken(op2, op2Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "z");
                    }
                }
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Case_Searched_WithoutElse()
        {
            const string text = @"
                CASE 1
                    WHEN 1 THEN 'One'
                END
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseExpression);
            enumerator.AssertToken(SyntaxKind.CaseKeyword, @"CASE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertNode(SyntaxKind.CaseLabel);
            enumerator.AssertToken(SyntaxKind.WhenKeyword, @"WHEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ThenKeyword, @"THEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'One'");
            enumerator.AssertToken(SyntaxKind.EndKeyword, @"END");
        }

        [Fact]
        public void Parser_Parse_Expression_Case_Searched_WithElse()
        {
            const string text = @"
                CASE 1
                    WHEN 1 THEN 'One'
                    ELSE 'Unknown'
                END
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseExpression);
            enumerator.AssertToken(SyntaxKind.CaseKeyword, @"CASE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertNode(SyntaxKind.CaseLabel);
            enumerator.AssertToken(SyntaxKind.WhenKeyword, @"WHEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ThenKeyword, @"THEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'One'");
            enumerator.AssertNode(SyntaxKind.CaseElseLabel);
            enumerator.AssertToken(SyntaxKind.ElseKeyword, @"ELSE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'Unknown'");
            enumerator.AssertToken(SyntaxKind.EndKeyword, @"END");
        }

        [Fact]
        public void Parser_Parse_Expression_Case_Regular_WithoutElse()
        {
            const string text = @"
                CASE
                    WHEN 1 THEN 'One'
                END
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseExpression);
            enumerator.AssertToken(SyntaxKind.CaseKeyword, @"CASE");
            enumerator.AssertNode(SyntaxKind.CaseLabel);
            enumerator.AssertToken(SyntaxKind.WhenKeyword, @"WHEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ThenKeyword, @"THEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'One'");
            enumerator.AssertToken(SyntaxKind.EndKeyword, @"END");
        }

        [Fact]
        public void Parser_Parse_Expression_Case_Regular_WithElse()
        {
            const string text = @"
                CASE
                    WHEN 1 THEN 'One'
                    ELSE 'Unknown'
                END
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseExpression);
            enumerator.AssertToken(SyntaxKind.CaseKeyword, @"CASE");
            enumerator.AssertNode(SyntaxKind.CaseLabel);
            enumerator.AssertToken(SyntaxKind.WhenKeyword, @"WHEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ThenKeyword, @"THEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'One'");
            enumerator.AssertNode(SyntaxKind.CaseElseLabel);
            enumerator.AssertToken(SyntaxKind.ElseKeyword, @"ELSE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'Unknown'");
            enumerator.AssertToken(SyntaxKind.EndKeyword, @"END");
        }

        [Fact]
        public void Parser_Parse_Expression_Case_Label()
        {
            const string text = @"
                CASE
                    {WHEN 1 THEN 'One'}
                    ELSE 'Unknown'
                END
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseLabel);
            enumerator.AssertToken(SyntaxKind.WhenKeyword, @"WHEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ThenKeyword, @"THEN");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'One'");
        }

        [Fact]
        public void Parser_Parse_Expression_Case_ElseLabel()
        {
            const string text = @"
                CASE
                    WHEN 1 THEN 'One'
                    {ELSE 'Unknown'}
                END AS X
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CaseElseLabel);
            enumerator.AssertToken(SyntaxKind.ElseKeyword, @"ELSE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'Unknown'");
        }

        [Fact]
        public void Parser_Parse_Expression_Cast()
        {
            const string text = @"
                CAST(1 AS BYTE)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CastExpression);
            enumerator.AssertToken(SyntaxKind.CastKeyword, @"CAST");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.AsKeyword, @"AS");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"BYTE");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Coalesce_WithNoArguments()
        {
            const string text = @"
                COALESCE()
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CoalesceExpression);
            enumerator.AssertToken(SyntaxKind.CoalesceKeyword, @"COALESCE");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNodeMissing(SyntaxKind.NameExpression);
            enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected '<Identifier>'.");
            enumerator.AssertTokenMissing(SyntaxKind.CommaToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected ','.");
            enumerator.AssertNodeMissing(SyntaxKind.NameExpression);
            enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected '<Identifier>'.");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Coalesce_WithSingleArguments()
        {
            const string text = @"
                COALESCE(1)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CoalesceExpression);
            enumerator.AssertToken(SyntaxKind.CoalesceKeyword, @"COALESCE");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertTokenMissing(SyntaxKind.CommaToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected ','.");
            enumerator.AssertNodeMissing(SyntaxKind.NameExpression);
            enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected '<Identifier>'.");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Coalesce_WithTwoArguments()
        {
            const string text = @"
                COALESCE(1, 2)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CoalesceExpression);
            enumerator.AssertToken(SyntaxKind.CoalesceKeyword, @"COALESCE");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Coalesce_WithThreeArguments()
        {
            const string text = @"
                COALESCE(1, 2, 3)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CoalesceExpression);
            enumerator.AssertToken(SyntaxKind.CoalesceKeyword, @"COALESCE");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"3");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_CountAll()
        {
            const string text = @"
                COUNT(*)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.CountAllExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"COUNT");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_FunctionInvocation()
        {
            const string text = @"
                TO_INT32('1')
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.FunctionInvocationExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"TO_INT32");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'1'");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Theory]
        [MemberData(nameof(GetContextualKeywords))]
        public void Parser_Parse_Expression_FunctionInvocation_WithContextualKeyword(SyntaxKind kind)
        {
            var kindText = kind.GetText();
            var text = $"{kindText}('2')";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.FunctionInvocationExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, kindText);
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'2'");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_FunctionInvocation_WithMultipleArguments()
        {
            const string text = @"
                FORMAT(0.75, 'P2')
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.FunctionInvocationExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"FORMAT");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"0.75");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'P2'");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_In()
        {
            const string text = @"
                1 IN (1)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_In_WithNoValues()
        {
            const string text = @"
                1 IN ()
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNodeMissing(SyntaxKind.NameExpression);
            enumerator.AssertTokenMissing(SyntaxKind.IdentifierToken);
            enumerator.AssertDiagnostic(DiagnosticId.TokenExpected, @"Found ')' but expected '<Identifier>'.");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_In_WithMultipleValues()
        {
            const string text = @"
                1 IN (1, 2)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_In_WithNotKeyword()
        {
            const string text = @"
                1 NOT IN (1, 2)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_InQuery()
        {
            const string text = @"
                1 IN (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InQueryExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_InQuery_WithNotKeyword()
        {
            const string text = @"
                1 NOT IN (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InQueryExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_InQuery_IfQueryIsParenthesized()
        {
            const string text = @"
                1 IN ((((SELECT *))))
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.InQueryExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.InKeyword, @"IN");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.ParenthesizedQuery);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.ParenthesizedQuery);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.ParenthesizedQuery);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_IsNull()
        {
            const string text = @"
                 X IS NULL
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.IsNullExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"X");
            enumerator.AssertToken(SyntaxKind.IsKeyword, @"IS");
            enumerator.AssertToken(SyntaxKind.NullKeyword, @"NULL");
        }

        [Fact]
        public void Parser_Parse_Expression_IsNull_WithNotKeyword()
        {
            const string text = @"
                 X IS NOT NULL
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.IsNullExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"X");
            enumerator.AssertToken(SyntaxKind.IsKeyword, @"IS");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.NullKeyword, @"NULL");
        }

        [Fact]
        public void Parser_Parse_Expression_Like()
        {
            const string text = @"
                'x' LIKE 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LikeExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.LikeKeyword, @"LIKE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_Like_WithNotKeyword()
        {
            const string text = @"
                'x' NOT LIKE 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LikeExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.LikeKeyword, @"LIKE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithStringLiteral()
        {
            const string text = @"
                'x'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithNumericLiteral()
        {
            const string text = @"
                1
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithNullLiteral()
        {
            const string text = @"
                NULL
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NullKeyword, @"NULL");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithDateLiteral()
        {
            const string text = @"
                #2015-09-14#
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.DateLiteralToken, @"#2015-09-14#");
        }

        [Fact]
        public void Parser_Parse_Expression_MethodInvocation()
        {
            const string text = @"
                x.SomeMethod(1)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.MethodInvocationExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"SomeMethod");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_MethodInvocation_WithMultipleArguments()
        {
            const string text = @"
                x.SomeMethod(1, '2')
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.MethodInvocationExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"SomeMethod");
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'2'");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithNumericLiteralWithMember()
        {
            const string text = @"10.Property";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"10");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Property");
        }

        [Fact]
        public void Parser_Parse_Expression_Literal_WithNumericLiteralWithMemberInvocation()
        {
            const string text = @"1.0.EqMethod";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1.0");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"EqMethod");
        }

        [Theory]
        [MemberData(nameof(GetContextualKeywords))]
        public void Parser_Parse_Expression_MethodInvocation_WithContextualKeyword(SyntaxKind kind)
        {
            var keyword = kind.GetText();
            var text = $@"x.{keyword}(1)";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.MethodInvocationExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, keyword);
            enumerator.AssertNode(SyntaxKind.ArgumentList);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Name()
        {
            const string text = @"
                x
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
        }

        [Theory]
        [MemberData(nameof(GetContextualKeywords))]
        public void Parser_Parse_Expression_Name_WithContextualKeyword(SyntaxKind kind)
        {
            var text = kind.GetText();

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, text);
        }

        [Fact]
        public void Parser_Parse_Expression_NullIf()
        {
            const string text = @"
                NULLIF(1, 2)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.NullIfExpression);
            enumerator.AssertToken(SyntaxKind.NullIfKeyword, @"NULLIF");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.CommaToken, @",");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"2");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Parenthesized()
        {
            const string text = @"
                (1)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.ParenthesizedExpression);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_PropertyAccess()
        {
            const string text = @"
                x.Prop
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"Prop");
        }

        [Theory]
        [MemberData(nameof(GetContextualKeywords))]
        public void Parser_Parse_Expression_PropertyAccess_WithContextualKeyword(SyntaxKind kind)
        {
            var keyword = kind.GetText();
            var text = $@"x.{keyword}";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.PropertyAccessExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.DotToken, @".");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, keyword);
        }

        [Fact]
        public void Parser_Parse_Expression_SimilarTo()
        {
            const string text = @"
                'x' SIMILAR TO 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.SimilarToExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.SimilarKeyword, @"SIMILAR");
            enumerator.AssertToken(SyntaxKind.ToKeyword, @"TO");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_SimilarTo_WithNotKeyword()
        {
            const string text = @"
                'x' NOT SIMILAR TO 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.SimilarToExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.SimilarKeyword, @"SIMILAR");
            enumerator.AssertToken(SyntaxKind.ToKeyword, @"TO");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_SoundsLike()
        {
            const string text = @"
                'x' SOUNDS LIKE 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.SoundsLikeExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.SoundsKeyword, @"SOUNDS");
            enumerator.AssertToken(SyntaxKind.LikeKeyword, @"LIKE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_SoundsLike_WithNotKeyword()
        {
            const string text = @"
                'x' NOT SOUNDS LIKE 'y'
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.SoundsLikeExpression);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'x'");
            enumerator.AssertToken(SyntaxKind.NotKeyword, @"NOT");
            enumerator.AssertToken(SyntaxKind.SoundsKeyword, @"SOUNDS");
            enumerator.AssertToken(SyntaxKind.LikeKeyword, @"LIKE");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.StringLiteralToken, @"'y'");
        }

        [Fact]
        public void Parser_Parse_Expression_AllAnySubselect_WithAll()
        {
            const string text = @"
                x >= ALL (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.AllAnySubselect);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.GreaterEqualToken, @">=");
            enumerator.AssertToken(SyntaxKind.AllKeyword, @"ALL");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_AllAnySubselect_WithAny()
        {
            const string text = @"
                x < ANY (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.AllAnySubselect);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"x");
            enumerator.AssertToken(SyntaxKind.LessToken, @"<");
            enumerator.AssertToken(SyntaxKind.AnyKeyword, @"ANY");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_AllAnySubselect_WithSome()
        {
            const string text = @"
                1 !> SOME (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.AllAnySubselect);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.ExclamationGreaterToken, @"!>");
            enumerator.AssertToken(SyntaxKind.SomeKeyword, @"SOME");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_AllAnySubselect_WithSome_AndInvalidOperator()
        {
            const string text = @"
                1 + SOME (SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.AllAnySubselect);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.PlusToken, @"+");
            enumerator.AssertDiagnostic(DiagnosticId.InvalidOperatorForAllAny, @"SOME, ANY and ALL can only be applied on =, <>, <, <=, >, or >=.");
            enumerator.AssertToken(SyntaxKind.SomeKeyword, @"SOME");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_ExistsSubselect()
        {
            const string text = @"
                EXISTS(SELECT *)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.ExistsSubselect);
            enumerator.AssertToken(SyntaxKind.ExistsKeyword, @"EXISTS");
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.WildcardSelectColumn);
            enumerator.AssertToken(SyntaxKind.AsteriskToken, @"*");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_SingleRowSubselect()
        {
            const string text = @"
                (SELECT 1)
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.SingleRowSubselect);
            enumerator.AssertToken(SyntaxKind.LeftParenthesisToken, @"(");
            enumerator.AssertNode(SyntaxKind.SelectQuery);
            enumerator.AssertNode(SyntaxKind.SelectClause);
            enumerator.AssertToken(SyntaxKind.SelectKeyword, @"SELECT");
            enumerator.AssertNode(SyntaxKind.ExpressionSelectColumn);
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
            enumerator.AssertToken(SyntaxKind.RightParenthesisToken, @")");
        }

        [Fact]
        public void Parser_Parse_Expression_Unary()
        {
            const string text = @"
                -1
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.NegationExpression);
            enumerator.AssertToken(SyntaxKind.MinusToken, @"-");
            enumerator.AssertNode(SyntaxKind.LiteralExpression);
            enumerator.AssertToken(SyntaxKind.NumericLiteralToken, @"1");
        }

        [Fact]
        public void Parser_Parse_Expression_Unary_HonorsPriorities()
        {
            foreach (var op1 in SyntaxFacts.GetUnaryExpressionTokenKinds())
            {
                foreach (var op2 in SyntaxFacts.GetBinaryExpressionTokenKinds())
                {
                    var op1Text = op1.GetText();
                    var op2Text = op2.GetText();
                    var text = $"{op1Text} x {op2Text} y";

                    var syntaxTree = SyntaxTree.ParseExpression(text);
                    Assert.Empty(syntaxTree.GetDiagnostics());

                    var expression = syntaxTree.Root.Root;

                    var op1ExpressionKind = SyntaxFacts.GetUnaryOperatorExpression(op1);
                    var op2ExpressionKind = SyntaxFacts.GetBinaryOperatorExpression(op2);
                    var op1Precedence = SyntaxFacts.GetUnaryOperatorPrecedence(op1ExpressionKind);
                    var op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2ExpressionKind);
                    var leftToRight = op1Precedence >= op2Precedence;

                    if (leftToRight)
                    {
                        //    op2
                        // op1   y
                        //  x

                        using var enumerator = AssertingEnumerator.ForNode(expression);
                        enumerator.AssertNode(op2ExpressionKind);
                        enumerator.AssertNode(op1ExpressionKind);
                        enumerator.AssertToken(op1, op1Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                        enumerator.AssertToken(op2, op2Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                    }
                    else
                    {
                        //  op1
                        //  op2
                        // x   y

                        using var enumerator = AssertingEnumerator.ForNode(expression);
                        enumerator.AssertNode(op1ExpressionKind);
                        enumerator.AssertToken(op1, op1Text);
                        enumerator.AssertNode(op2ExpressionKind);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "x");
                        enumerator.AssertToken(op2, op2Text);
                        enumerator.AssertNode(SyntaxKind.NameExpression);
                        enumerator.AssertToken(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
        }

        [Fact]
        public void Parser_Parse_Expression_Variable()
        {
            const string text = @"
                @var
            ";

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.VariableExpression);
            enumerator.AssertToken(SyntaxKind.AtToken, @"@");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, @"var");
        }

        [Theory]
        [MemberData(nameof(GetContextualKeywords))]
        public void Parser_Parse_Expression_Variable_WithContextualKeyword(SyntaxKind kind)
        {
            var keyword = kind.GetText();
            var text = "@" + keyword;

            using var enumerator = AssertingEnumerator.ForExpression(text);
            enumerator.AssertNode(SyntaxKind.VariableExpression);
            enumerator.AssertToken(SyntaxKind.AtToken, @"@");
            enumerator.AssertToken(SyntaxKind.IdentifierToken, keyword);
        }
    }
}