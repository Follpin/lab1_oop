using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace SpreadsheetApp
{
    public class FormulaCalculator
    {
        private static readonly Dictionary<string, LabCalculatorParser.CompileUnitContext> _astCache = new(StringComparer.OrdinalIgnoreCase);

        public static void ClearCache()
        {
            lock (_astCache)
            {
                _astCache.Clear();
            }
        }

        private static LabCalculatorParser.CompileUnitContext GetOrCreateParseTree(string cleanExpr)
        {
            lock (_astCache)
            {
                if (_astCache.TryGetValue(cleanExpr, out var cachedTree))
                {
                    return cachedTree;
                }

                var inputStream = new AntlrInputStream(cleanExpr);
                var lexer = new LabCalculatorLexer(inputStream);
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(ThrowExceptionErrorListener.Instance);

                var tokens = new CommonTokenStream(lexer);
                var parser = new LabCalculatorParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(ThrowExceptionErrorListener.Instance);

                var tree = parser.compileUnit();
                _astCache[cleanExpr] = tree;
                return tree;
            }
        }

        public double Evaluate(string expression, Spreadsheet table, HashSet<string>? evaluated = null)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return EvaluateInternal(expression, table, visited, evaluated);
        }

        public bool Validate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return true;
            }

            try
            {
                string cleanExpr = expression.Trim();
                if (cleanExpr.StartsWith("="))
                {
                    cleanExpr = cleanExpr.Substring(1).Trim();
                }

                if (string.IsNullOrWhiteSpace(cleanExpr))
                {
                    return true;
                }

                GetOrCreateParseTree(cleanExpr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static double EvaluateInternal(string expression, Spreadsheet table, HashSet<string> visited, HashSet<string>? evaluated = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return 0.0;
            }

            string cleanExpr = expression.Trim();
            if (cleanExpr.StartsWith("="))
            {
                cleanExpr = cleanExpr.Substring(1).Trim();
            }

            if (string.IsNullOrWhiteSpace(cleanExpr))
            {
                return 0.0;
            }

            var tree = GetOrCreateParseTree(cleanExpr);
            var visitor = new LabCalculatorVisitor(table, visited, evaluated);
            return visitor.Visit(tree);
        }
    }
}

