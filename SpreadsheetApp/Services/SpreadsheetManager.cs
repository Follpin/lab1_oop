using System;
using System.Globalization;

namespace SpreadsheetApp
{
    public class SpreadsheetManager
    {
        private Spreadsheet _spreadsheet;
        private FormulaCalculator _calculator;
        private FileManager _fileManager;
        private DisplayMode _mode;

        public Spreadsheet CurrentTable => _spreadsheet;
        public DisplayMode Mode => _mode;

        public SpreadsheetManager(int initialRows, int initialCols)
        {
            _spreadsheet = new Spreadsheet(initialRows, initialCols);
            _calculator = new FormulaCalculator();
            _fileManager = new FileManager();
            _mode = DisplayMode.Expressions;
        }

        public void UpdateCell(string cellName, string expr)
        {
            var cell = _spreadsheet.GetCell(cellName);
            if (cell != null)
            {
                cell.Expression = expr ?? string.Empty;
                cell.HasError = false;
                cell.ErrorMessage = string.Empty;
            }
        }

        public void RecalculateAll()
        {
            // очищаємо стани помилок перед новим  розрахунком
            foreach (var cell in _spreadsheet.Cells.Values)
            {
                cell.HasError = false;
                cell.ErrorMessage = string.Empty;
            }

            // кожна клітинка обчислюється щонайбільше один раз (для оптимізації)
            var evaluated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in _spreadsheet.Cells.Values)
            {
                if (evaluated.Contains(cell.Name))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(cell.Expression))
                {
                    cell.Value = 0.0;
                    cell.HasError = false;
                    cell.ErrorMessage = string.Empty;
                    evaluated.Add(cell.Name);
                    continue;
                }

                try
                {
                    cell.Value = _calculator.Evaluate(cell.Expression, _spreadsheet, evaluated);
                    cell.HasError = false;
                    cell.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    cell.HasError = true;
                    cell.ErrorMessage = ex.Message;
                }
                finally
                {
                    evaluated.Add(cell.Name);
                }
            }
        }

        public DisplayMode ToggleDisplayMode()
        {
            _mode = (_mode == DisplayMode.Expressions) ? DisplayMode.Values : DisplayMode.Expressions;
            return _mode;
        }

        public string GetDisplayText(string cellName)
        {
            var cell = _spreadsheet.GetCell(cellName);
            if (cell == null)
            {
                return string.Empty;
            }

            if (_mode == DisplayMode.Expressions)
            {
                return cell.Expression;
            }

            if (cell.HasError)
            {
                return "#ПОМИЛКА";
            }

            if (string.IsNullOrWhiteSpace(cell.Expression))
            {
                return string.Empty;
            }

            return cell.Value.ToString(CultureInfo.InvariantCulture);
        }

        public void Save(string filePath)
        {
            _fileManager.SaveToFile(filePath, _spreadsheet);
        }

        public void Load(string filePath)
        {
            _spreadsheet = _fileManager.LoadFromFile(filePath);
            RecalculateAll();
        }

        public void AddRow()
        {
            _spreadsheet.AddRow();
        }

        public void DeleteRow()
        {
            _spreadsheet.DeleteRow();
        }

        public void AddColumn()
        {
            _spreadsheet.AddColumn();
        }

        public void DeleteColumn()
        {
            _spreadsheet.DeleteColumn();
        }
    }
}

