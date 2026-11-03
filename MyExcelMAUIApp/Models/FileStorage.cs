using System.Text.Json;

namespace MyExcelMAUIApp.Models;

public class FileStorage
{
	private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

	private class TableData
	{
		public int Rows { get; set; }
		public int Columns { get; set; }
		public SortedDictionary<string, string> Cells { get; set; } = new();
	}
	

	public void SaveToFile(Table table, string path)
	{
		var data = new TableData { Rows = table.RowCount, Columns = table.ColumnCount };
		foreach (var c in table.GetAllCells().Where(c => !c.IsEmpty()))
			data.Cells[c.Reference] = c.Expression;
		File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
	}

	public Table LoadFromFile(string path)
	{
		var data = JsonSerializer.Deserialize<TableData>(File.ReadAllText(path))
			?? throw new InvalidDataException("Файл порожній або пошкоджений");
		if (data.Rows < 1 || data.Columns < 1)
			throw new InvalidDataException("У файлі неправильний розмір таблиці");

		var table = new Table(data.Rows, data.Columns);
		foreach (var (reference, expression) in data.Cells)
			table.SetCellExpression(reference, expression);
		return table;
	}
}
