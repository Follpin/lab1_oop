namespace SpreadsheetApp
{
    public class Cell
    {
        private string _name;
        private int _rowIndex;
        private int _columnIndex;
        private string _expression;
        private double _value;
        private bool _hasError;
        private string _errorMessage;

        public string Name => _name;
        public int RowIndex => _rowIndex;
        public int ColumnIndex => _columnIndex;

        public string Expression
        {
            get => _expression;
            set => _expression = value ?? string.Empty;
        }

        public double Value
        {
            get => _value;
            set => _value = value;
        }

        public bool HasError
        {
            get => _hasError;
            set => _hasError = value;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value ?? string.Empty;
        }

        public Cell(string name, int row, int col)
        {
            _name = name;
            _rowIndex = row;
            _columnIndex = col;
            _expression = string.Empty;
            _value = 0.0;
            _hasError = false;
            _errorMessage = string.Empty;
        }

        public void Clear()
        {
            _expression = string.Empty;
            _value = 0.0;
            _hasError = false;
            _errorMessage = string.Empty;
        }
    }
}

