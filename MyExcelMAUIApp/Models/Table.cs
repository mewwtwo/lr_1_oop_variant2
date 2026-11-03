using System.Numerics;
using MyExcelMAUIApp.Parsing;

namespace MyExcelMAUIApp.Models;

public class Table
{
	public int RowCount { get; private set; }
	public int ColumnCount { get; private set; }

	private readonly Dictionary<string, Cell> cells = new();
	private readonly HashSet<string> calculating = new();

	public Table(int rowCount, int columnCount)
	{
		RowCount = rowCount;
		ColumnCount = columnCount;
	}

	public static string GetColumnName(int colNumber)
	{
		string name = "";
		while (colNumber > 0)
		{
			colNumber--;
			name = (char)('A' + colNumber % 26) + name;
			colNumber /= 26;
		}
		return name;
	}

	public static string MakeReference(int row, int col) => GetColumnName(col) + row;

	public static bool TryParseReference(string reference, out int row, out int col)
	{
		row = 0;
		col = 0;
		int i = 0;
		while (i < reference.Length && char.IsLetter(reference[i]))
		{
			col = col * 26 + (char.ToUpper(reference[i]) - 'A' + 1);
			i++;
		}
		return i > 0 && int.TryParse(reference[i..], out row) && row > 0;
	}

	public Cell GetCell(string reference)
	{
		reference = reference.ToUpper();
		if (!cells.ContainsKey(reference))
			cells[reference] = new Cell(reference);
		return cells[reference];
	}

	public Cell? TryGetCell(string reference)
	{
		cells.TryGetValue(reference.ToUpper(), out var cell);
		return cell;
	}

	public IEnumerable<Cell> GetAllCells() => cells.Values;

	public void SetCellExpression(string reference, string expression)
	{
		expression = expression.Trim();
		if (expression.StartsWith('='))
			expression = expression[1..].Trim();
		GetCell(reference).Expression = expression;
	}

	public BigInteger GetCellValue(string reference)
	{
		reference = reference.ToUpper();
		if (!TryParseReference(reference, out int row, out int col) || row > RowCount || col > ColumnCount)
			throw new ArgumentException($"Клітинки {reference} немає в таблиці");

		var cell = GetCell(reference);
		if (cell.IsEmpty())
			return 0;

		if (calculating.Contains(reference))
			throw new ArgumentException($"Циклічне посилання на {reference}");

		CalculateCell(cell);
		if (cell.HasError())
			throw new ArgumentException($"У клітинці {reference} помилка");
		return cell.Value;
	}

	private void CalculateCell(Cell cell)
	{
		if (cell.IsEmpty())
		{
			cell.SetValue(0);
			return;
		}

		calculating.Add(cell.Reference);
		try
		{
			cell.SetValue(Calculator.Evaluate(cell.Expression, GetCellValue));
		}
		catch (DivideByZeroException)
		{
			cell.SetError("Ділення на нуль");
		}
		catch (Exception ex)
		{
			cell.SetError(ex.Message);
		}
		finally
		{
			calculating.Remove(cell.Reference);
		}
	}

	public void CalculateAll()
	{
		foreach (var cell in cells.Values.ToList())
			CalculateCell(cell);
	}

	public List<Cell> GetCellsWithErrors()
	{
		return cells.Values.Where(c => c.HasError()).OrderBy(c => c.Reference).ToList();
	}

	public void AddRow()
	{
		RowCount++;
	}

	public void AddColumn()
	{
		ColumnCount++;
	}

	public void DeleteRow(int index)
	{
		if (RowCount <= 1 || index < 1 || index > RowCount)
			return;

		var oldCells = cells.Values.ToList();
		cells.Clear();
		foreach (var cell in oldCells)
		{
			TryParseReference(cell.Reference, out int row, out int col);
			if (row == index)
				continue;
			if (row > index)
				row--;
			cell.Reference = MakeReference(row, col);
			cells[cell.Reference] = cell;
		}
		RowCount--;
	}

	public void DeleteColumn(int index)
	{
		if (ColumnCount <= 1 || index < 1 || index > ColumnCount)
			return;

		var oldCells = cells.Values.ToList();
		cells.Clear();
		foreach (var cell in oldCells)
		{
			TryParseReference(cell.Reference, out int row, out int col);
			if (col == index)
				continue;
			if (col > index)
				col--;
			cell.Reference = MakeReference(row, col);
			cells[cell.Reference] = cell;
		}
		ColumnCount--;
	}
}
