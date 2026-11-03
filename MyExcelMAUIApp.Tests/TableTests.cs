using MyExcelMAUIApp.Models;

namespace MyExcelMAUIApp.Tests;

public class TableTests
{
	[Fact]
	public void CellCanUseOtherCell()
	{
		var table = new Table(10, 10);
		table.SetCellExpression("A1", "10");
		table.SetCellExpression("A2", "A1 + 5");

		table.CalculateAll();

		Assert.Equal(15, (int)table.GetCell("A2").Value);
	}

	[Fact]
	public void EmptyCellIsZero()
	{
		var table = new Table(10, 10);
		table.SetCellExpression("A1", "B5 + 7");

		table.CalculateAll();

		Assert.Equal(7, (int)table.GetCell("A1").Value);
	}

	[Fact]
	public void WrongExpressionGivesCellError()
	{
		var table = new Table(10, 10);
		table.SetCellExpression("A1", "1 +");

		table.CalculateAll();

		Assert.True(table.GetCell("A1").HasError());
	}

	[Fact]
	public void CircularReferenceGivesError()
	{
		var table = new Table(10, 10);
		table.SetCellExpression("A1", "A2");
		table.SetCellExpression("A2", "A1");

		table.CalculateAll();

		Assert.True(table.GetCell("A1").HasError());
		Assert.True(table.GetCell("A2").HasError());
	}

	[Fact]
	public void CellOutsideTableGivesError()
	{
		var table = new Table(5, 5);
		table.SetCellExpression("A1", "Z99");

		table.CalculateAll();

		Assert.True(table.GetCell("A1").HasError());
	}

	[Fact]
	public void DeleteRowMovesCellsUp()
	{
		var table = new Table(5, 5);
		table.SetCellExpression("A1", "1");
		table.SetCellExpression("A2", "2");
		table.SetCellExpression("A3", "3");

		table.DeleteRow(2);
		table.CalculateAll();

		Assert.Equal(4, table.RowCount);
		Assert.Equal(3, (int)table.GetCell("A2").Value);
	}

	[Fact]
	public void ColumnNamesAreLikeInExcel()
	{
		Assert.Equal("A", Table.GetColumnName(1));
		Assert.Equal("Z", Table.GetColumnName(26));
		Assert.Equal("AA", Table.GetColumnName(27));
	}

	[Fact]
	public void SavedTableCanBeReadBack()
	{
		var table = new Table(7, 8);
		table.SetCellExpression("A1", "5");
		table.SetCellExpression("B2", "A1 * 2");
		string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
		var storage = new FileStorage();

		storage.SaveToFile(table, path);
		var loaded = storage.LoadFromFile(path);
		loaded.CalculateAll();
		File.Delete(path);

		Assert.Equal(7, loaded.RowCount);
		Assert.Equal(8, loaded.ColumnCount);
		Assert.Equal(10, (int)loaded.GetCell("B2").Value);
	}
}
