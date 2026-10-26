using System;
using System.Collections.Generic;

namespace SpreadsheetApp
{
    public class Spreadsheet
    {
        private Dictionary<string, Cell> _cells;
        private int _rowCount;
        private int _columnCount;

        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;
        public IReadOnlyDictionary<string, Cell> Cells => _cells;

        public Spreadsheet(int rows, int cols)
        {
            _rowCount = Math.Max(1, rows);
            _columnCount = Math.Max(1, cols);
            _cells = new Dictionary<string, Cell>(StringComparer.OrdinalIgnoreCase);

            for (int r = 1; r <= _rowCount; r++)
            {
                for (int c = 1; c <= _columnCount; c++)
                {
                    string name = GetColumnName(c) + r;
                    _cells[name] = new Cell(name, r, c);
                }
            }
        }

        public Cell? GetCell(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string normalized = name.Trim().ToUpperInvariant();
            if (_cells.TryGetValue(normalized, out var cell))
            {
                return cell;
            }

            return null;
        }

        public Cell? GetCell(int row, int col)
        {
            string name = GetColumnName(col) + row;
            return GetCell(name);
        }

        public void SetCellExpression(string name, string expr)
        {
            var cell = GetCell(name);
            if (cell != null)
            {
                cell.Expression = expr ?? string.Empty;
            }
        }

        public void AddRow()
        {
            _rowCount++;
            for (int c = 1; c <= _columnCount; c++)
            {
                string name = GetColumnName(c) + _rowCount;
                if (!_cells.ContainsKey(name))
                {
                    _cells[name] = new Cell(name, _rowCount, c);
                }
            }
        }

        public void DeleteRow()
        {
            if (_rowCount <= 1)
            {
                return;
            }

            for (int c = 1; c <= _columnCount; c++)
            {
                string name = GetColumnName(c) + _rowCount;
                _cells.Remove(name);
            }
            _rowCount--;
        }

        public void AddColumn()
        {
            _columnCount++;
            for (int r = 1; r <= _rowCount; r++)
            {
                string name = GetColumnName(_columnCount) + r;
                if (!_cells.ContainsKey(name))
                {
                    _cells[name] = new Cell(name, r, _columnCount);
                }
            }
        }

        public void DeleteColumn()
        {
            if (_columnCount <= 1)
            {
                return;
            }

            for (int r = 1; r <= _rowCount; r++)
            {
                string name = GetColumnName(_columnCount) + r;
                _cells.Remove(name);
            }
            _columnCount--;
        }

        public static string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}

