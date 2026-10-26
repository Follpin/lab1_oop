using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SpreadsheetApp
{
    public class FileManager
    {
        private class SpreadsheetData
        {
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
            public Dictionary<string, string> Cells { get; set; } = new Dictionary<string, string>();
        }

        public void SaveToFile(string path, Spreadsheet table)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(path));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var data = new SpreadsheetData
            {
                RowCount = table.RowCount,
                ColumnCount = table.ColumnCount,
                Cells = new Dictionary<string, string>()
            };

            foreach (var kvp in table.Cells)
            {
                if (!string.IsNullOrEmpty(kvp.Value.Expression))
                {
                    data.Cells[kvp.Key] = kvp.Value.Expression;
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        public Spreadsheet LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException("Файл не знайдено.", path);
            }

            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SpreadsheetData>(json);
            if (data == null)
            {
                throw new InvalidDataException("Некоректний формат файлу таблиці.");
            }

            var table = new Spreadsheet(data.RowCount, data.ColumnCount);
            if (data.Cells != null)
            {
                foreach (var kvp in data.Cells)
                {
                    table.SetCellExpression(kvp.Key, kvp.Value);
                }
            }

            return table;
        }
    }
}

