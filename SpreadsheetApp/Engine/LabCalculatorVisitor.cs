using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpreadsheetApp
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        private Spreadsheet _table;
        private HashSet<string> _visitedCells;
        private HashSet<string> _evaluatedCells;

        public LabCalculatorVisitor(Spreadsheet table, HashSet<string> visited, HashSet<string>? evaluated = null)
        {
            _table = table;
            _visitedCells = visited ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _evaluatedCells = evaluated ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));
            int opType = context.operatorToken.Type;

            if (opType == LabCalculatorParser.MULTIPLY)
            {
                return left * right;
            }

            if (opType == LabCalculatorParser.DIVIDE)
            {
                if (right == 0.0)
                {
                    throw new DivideByZeroException("Ділення на нуль.");
                }
                return left / right;
            }

            if (opType == LabCalculatorParser.DIV)
            {
                if ((long)right == 0)
                {
                    throw new DivideByZeroException("Цілочисельне ділення (div) на нуль.");
                }
                return (long)left / (long)right;
            }

            if (opType == LabCalculatorParser.MOD)
            {
                if ((long)right == 0)
                {
                    throw new DivideByZeroException("Остача від ділення (mod) на нуль.");
                }
                return (long)left % (long)right;
            }

            throw new InvalidOperationException($"Невідома мультиплікативна операція: {context.operatorToken.Text}");
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));
            int opType = context.operatorToken.Type;

            if (opType == LabCalculatorParser.ADD)
            {
                return left + right;
            }

            if (opType == LabCalculatorParser.SUBTRACT)
            {
                return left - right;
            }

            throw new InvalidOperationException($"Невідома адитивна операція: {context.operatorToken.Text}");
        }

        public override double VisitUnaryExpr(LabCalculatorParser.UnaryExprContext context)
        {
            double val = Visit(context.expression());
            int opType = context.operatorToken.Type;

            if (opType == LabCalculatorParser.ADD)
            {
                return +val;
            }

            if (opType == LabCalculatorParser.SUBTRACT)
            {
                return -val;
            }

            throw new InvalidOperationException($"Невідома унарна операція: {context.operatorToken.Text}");
        }

        public override double VisitFunctionExpr(LabCalculatorParser.FunctionExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));
            int funcType = context.func.Type;

            if (funcType == LabCalculatorParser.MAX)
            {
                return Math.Max(left, right);
            }

            if (funcType == LabCalculatorParser.MIN)
            {
                return Math.Min(left, right);
            }

            throw new InvalidOperationException($"Невідома функція: {context.func.Text}");
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            string text = context.NUMBER().GetText();
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }

            throw new FormatException($"Некоректний числовий формат: {text}");
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            string cellName = context.IDENTIFIER().GetText().Trim().ToUpperInvariant();

            if (_visitedCells.Contains(cellName))
            {
                throw new InvalidOperationException($"Виявлено циклічне посилання: {string.Join(" -> ", _visitedCells)} -> {cellName}");
            }

            if (_table == null)
            {
                throw new InvalidOperationException($"Таблиця не ініціалізована для пошуку клітинки {cellName}.");
            }

            var cell = _table.GetCell(cellName);
            if (cell == null)
            {
                throw new ArgumentException($"Клітинка {cellName} виходить за межі поточної таблиці.");
            }

          // якщо клітинка вже обчислена в поточному раунді перерахунку
            if (_evaluatedCells.Contains(cellName))
            {
                if (cell.HasError)
                {
                    throw new InvalidOperationException($"Помилка у залежній клітинці {cellName}: {cell.ErrorMessage}");
                }
                return cell.Value;
            }

            if (string.IsNullOrWhiteSpace(cell.Expression))
            {
                _evaluatedCells.Add(cellName);
                return 0.0;
            }

            _visitedCells.Add(cellName);
            try
            {
                double val = FormulaCalculator.EvaluateInternal(cell.Expression, _table, _visitedCells, _evaluatedCells);
                cell.Value = val;
                cell.HasError = false;
                cell.ErrorMessage = string.Empty;
                _evaluatedCells.Add(cellName);
                return val;
            }
            catch (Exception ex)
            {
                cell.HasError = true;
                cell.ErrorMessage = ex.Message;
                _evaluatedCells.Add(cellName);
                throw new InvalidOperationException($"Помилка у залежній клітинці {cellName}: {ex.Message}", ex);
            }
            finally
            {
                _visitedCells.Remove(cellName);
            }
        }
    }
}

