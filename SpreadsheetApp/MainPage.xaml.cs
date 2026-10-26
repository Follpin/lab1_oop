using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace SpreadsheetApp
{
    public partial class MainPage : ContentPage
    {
        private SpreadsheetManager _manager;
        private string _selectedCellName;
        private readonly Dictionary<string, Entry> _entryMap = new(StringComparer.OrdinalIgnoreCase);

        public MainPage()
        {
            InitializeComponent();
            _manager = new SpreadsheetManager(15, 8);
            _selectedCellName = "A1";
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            _entryMap.Clear();
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int rowCount = _manager.CurrentTable.RowCount;
            int colCount = _manager.CurrentTable.ColumnCount;

            // стовпчик 0 - для номерів рядків
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 50 });
            for (int c = 1; c <= colCount; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 95 });
            }

            // рядок 0 - для заголовків стовпчиків
            grid.RowDefinitions.Add(new RowDefinition { Height = 35 });
            for (int r = 1; r <= rowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = 35 });
            }

            // кутова порожня клітинка (0,0)
            var corner = new BoxView { Color = Color.FromArgb("#e0e0e0") };
            Grid.SetRow(corner, 0);
            Grid.SetColumn(corner, 0);
            grid.Children.Add(corner);

            // заголовки стовпчиків (букви)
            for (int c = 1; c <= colCount; c++)
            {
                var label = new Label
                {
                    Text = Spreadsheet.GetColumnName(c),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#333333")
                };
                var headerBg = new BoxView { Color = Color.FromArgb("#f0f0f0") };
                Grid.SetRow(headerBg, 0);
                Grid.SetColumn(headerBg, c);
                grid.Children.Add(headerBg);

                Grid.SetRow(label, 0);
                Grid.SetColumn(label, c);
                grid.Children.Add(label);
            }

            // заголовки рядків (цифри) та поля вводу 
            for (int r = 1; r <= rowCount; r++)
            {
                var rowLabel = new Label
                {
                    Text = r.ToString(),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#333333")
                };
                var rowBg = new BoxView { Color = Color.FromArgb("#f0f0f0") };
                Grid.SetRow(rowBg, r);
                Grid.SetColumn(rowBg, 0);
                grid.Children.Add(rowBg);

                Grid.SetRow(rowLabel, r);
                Grid.SetColumn(rowLabel, 0);
                grid.Children.Add(rowLabel);

                for (int c = 1; c <= colCount; c++)
                {
                    string cellName = Spreadsheet.GetColumnName(c) + r;
                    var cell = _manager.CurrentTable.GetCell(cellName);

                    var entry = new Entry
                    {
                        ClassId = cellName,
                        Text = _manager.GetDisplayText(cellName),
                        BackgroundColor = (cell?.HasError == true) ? Color.FromArgb("#ffebee") : Colors.White,
                        TextColor = (cell?.HasError == true) ? Colors.Red : Colors.Black,
                        HorizontalTextAlignment = TextAlignment.Start,
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        FontSize = 13,
                        IsReadOnly = (_manager.Mode == DisplayMode.Values)
                    };

                    entry.Focused += OnCellFocused;
                    entry.Unfocused += OnCellUnfocused;

                    _entryMap[cellName] = entry;

                    Grid.SetRow(entry, r);
                    Grid.SetColumn(entry, c);
                    grid.Children.Add(entry);
                }
            }
        }

        private void RefreshGridDisplay()
        {
            bool isValuesMode = (_manager.Mode == DisplayMode.Values);
            foreach (var kvp in _entryMap)
            {
                string cellName = kvp.Key;
                var entry = kvp.Value;
                var cell = _manager.CurrentTable.GetCell(cellName);

                entry.Text = _manager.GetDisplayText(cellName);
                entry.BackgroundColor = (cell?.HasError == true) ? Color.FromArgb("#ffebee") : Colors.White;
                entry.TextColor = (cell?.HasError == true) ? Colors.Red : Colors.Black;
                entry.IsReadOnly = isValuesMode;
            }

            var currentCell = _manager.CurrentTable.GetCell(_selectedCellName);
            if (currentCell != null && currentCell.HasError)
            {
                statusLabel.Text = currentCell.ErrorMessage;
                statusLabel.TextColor = Colors.Red;
            }
            else
            {
                statusLabel.Text = "Готово";
                statusLabel.TextColor = Color.FromArgb("#2e7d32");
            }
        }

        private void OnCellFocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry && !string.IsNullOrEmpty(entry.ClassId))
            {
                _selectedCellName = entry.ClassId;
                selectedCellLabel.Text = _selectedCellName;

                var cell = _manager.CurrentTable.GetCell(_selectedCellName);
                formulaInput.Text = cell?.Expression ?? string.Empty;

                if (cell != null && cell.HasError)
                {
                    statusLabel.Text = cell.ErrorMessage;
                    statusLabel.TextColor = Colors.Red;
                }
                else
                {
                    statusLabel.Text = "Готово";
                    statusLabel.TextColor = Color.FromArgb("#2e7d32");
                }
            }
        }

        private void OnCellUnfocused(object? sender, FocusEventArgs e)
        {
            // рішення проблеми у режмі Відображення 
            if (_manager.Mode == DisplayMode.Values)
            {
                return;
            }

            if (sender is Entry entry && !string.IsNullOrEmpty(entry.ClassId))
            {
                _manager.UpdateCell(entry.ClassId, entry.Text);
                if (entry.ClassId == _selectedCellName)
                {
                    formulaInput.Text = entry.Text;
                }
            }
        }

        private void CommitFormulaInput()
        {
            if (!string.IsNullOrEmpty(_selectedCellName))
            {
                _manager.UpdateCell(_selectedCellName, formulaInput.Text);
                if (_manager.Mode == DisplayMode.Expressions && _entryMap.TryGetValue(_selectedCellName, out var cellEntry))
                {
                    cellEntry.Text = formulaInput.Text;
                }
            }
        }

        private void OnFormulaInputCompleted(object? sender, EventArgs e)
        {
            CommitFormulaInput();
            RefreshGridDisplay();
        }

        private void OnFormulaInputUnfocused(object? sender, FocusEventArgs e)
        {
            CommitFormulaInput();
            RefreshGridDisplay();
        }

        private void OnCalculateClicked(object? sender, EventArgs e)
        {
            CommitFormulaInput();
            _manager.RecalculateAll();
            RefreshGridDisplay();
        }

        private void OnToggleModeClicked(object? sender, EventArgs e)
        {
            CommitFormulaInput();
            var newMode = _manager.ToggleDisplayMode();
            modeButton.Text = newMode == DisplayMode.Expressions ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            RefreshGridDisplay();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            CommitFormulaInput();
            try
            {
                string defaultPath = Path.Combine(FileSystem.AppDataDirectory, "Spreadsheet.json");
                string path = await DisplayPromptAsync("Збереження таблиці", "Вкажіть повний шлях до файлу:", initialValue: defaultPath);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    _manager.Save(path);
                    await DisplayAlert("Успіх", $"Таблицю успішно збережено у:\n{path}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження", ex.Message, "OK");
            }
        }

        private async void OnLoadClicked(object? sender, EventArgs e)
        {
            try
            {
                string defaultPath = Path.Combine(FileSystem.AppDataDirectory, "Spreadsheet.json");
                string path = await DisplayPromptAsync("Відкриття таблиці", "Вкажіть повний шлях до файлу:", initialValue: defaultPath);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    _manager.Load(path);
                    InitializeGrid();
                    EnsureSelectedCellValid();
                    await DisplayAlert("Успіх", "Таблицю успішно завантажено!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка відкриття", ex.Message, "OK");
            }
        }

        private void EnsureSelectedCellValid()
        {
            var cell = _manager.CurrentTable.GetCell(_selectedCellName);
            if (cell == null)
            {
                _selectedCellName = "A1";
            }
            selectedCellLabel.Text = _selectedCellName;
            var currentCell = _manager.CurrentTable.GetCell(_selectedCellName);
            formulaInput.Text = currentCell?.Expression ?? string.Empty;

            if (currentCell != null && currentCell.HasError)
            {
                statusLabel.Text = currentCell.ErrorMessage;
                statusLabel.TextColor = Colors.Red;
            }
            else
            {
                statusLabel.Text = "Готово";
                statusLabel.TextColor = Color.FromArgb("#2e7d32");
            }
        }

        private void OnAddRowClicked(object? sender, EventArgs e)
        {
            _manager.AddRow();
            InitializeGrid();
            EnsureSelectedCellValid();
        }

        private void OnDeleteRowClicked(object? sender, EventArgs e)
        {
            _manager.DeleteRow();
            InitializeGrid();
            EnsureSelectedCellValid();
        }

        private void OnAddColumnClicked(object? sender, EventArgs e)
        {
            _manager.AddColumn();
            InitializeGrid();
            EnsureSelectedCellValid();
        }

        private void OnDeleteColumnClicked(object? sender, EventArgs e)
        {
            _manager.DeleteColumn();
            InitializeGrid();
            EnsureSelectedCellValid();
        }

        private async void OnHelpClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Довідка про програму",
                "Лабораторна робота №1: Таблиця\n\n" +
                "Варіант 3 (1, 2, 3, 6):\n" +
                "1. Бінарні операції: +, -, *, /\n" +
                "2. Цілочисельні операції: mod (остача), div (ціла частина)\n" +
                "3. Унарні операції: +, -\n" +
                "6. Функції: max(x, y), min(x, y)\n" +
                "Операнди: цілі числа, посилання на клітинки (пр: A1, B2), дужки ().\n\n" +
                "Підтримується 2 режими відображення: ВИРАЗ та ЗНАЧЕННЯ.\n" +
                "Для перерахунку значень натисніть Порахувати.",
                "OK");
        }

        private async void OnExitClicked(object? sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (answer)
            {
                Application.Current?.Quit();
            }
        }
    }
}

