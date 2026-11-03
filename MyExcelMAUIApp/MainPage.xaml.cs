using MyExcelMAUIApp.Models;

namespace MyExcelMAUIApp;

public partial class MainPage : ContentPage
{
	const int CountColumn = 20;
	const int CountRow = 50;

	private Table table = new(CountRow, CountColumn);
	private readonly FileStorage fileStorage = new();
	private readonly Dictionary<string, Entry> entries = new();

	private readonly List<Label> rowLabels = new();
	private readonly List<Label> columnLabels = new();
	private readonly HashSet<string> redCells = new();
	private DisplayMode displayMode = DisplayMode.Expression;
	private int selectedRow = 1;
	private int selectedCol = 1;

	public MainPage()
	{
		InitializeComponent();
		CreateGrid();
	}

	private void CreateGrid()
	{
		entries.Clear();
		rowLabels.Clear();
		columnLabels.Clear();
		redCells.Clear();
		grid.Children.Clear();
		grid.RowDefinitions.Clear();
		grid.ColumnDefinitions.Clear();

		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
		grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(44)));

		AddColumnsAndColumnLabels();
		AddRowsAndCellEntries();
		ShowAllCells();
	}

	private void AddColumnsAndColumnLabels()
	{

		for (int col = 1; col <= table.ColumnCount; col++) AddColumnView(col, createEntries: false);
	}

	private void AddRowsAndCellEntries()
	{

		for (int row = 1; row <= table.RowCount; row++) AddRowView(row);
	}

	private void AddColumnView(int col, bool createEntries)
	{
		grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(110)));

		var label = MakeLabel(GetColumnName(col));
		Grid.SetRow(label, 0);
		Grid.SetColumn(label, col);
		grid.Children.Add(label);
		columnLabels.Add(label);

		if (!createEntries) return;
		for (int row = 1; row <= rowLabels.Count; row++) AddEntry(row, col);
	}

	private void AddRowView(int row)
	{
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

		var label = MakeLabel(row.ToString());
		Grid.SetRow(label, row);
		Grid.SetColumn(label, 0);
		grid.Children.Add(label);
		rowLabels.Add(label);

		for (int col = 1; col <= columnLabels.Count; col++) AddEntry(row, col);
	}

	private void AddEntry(int row, int col)
	{
		var entry = new Entry
		{
			Text = "",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Fill
		};
		entry.Focused += Entry_Focused;
		entry.Unfocused += Entry_Unfocused;
		Grid.SetRow(entry, row);
		Grid.SetColumn(entry, col);
		grid.Children.Add(entry);
		entries[Table.MakeReference(row, col)] = entry;
	}

	private static Label MakeLabel(string text) => new()
	{
		Text = text,
		FontAttributes = FontAttributes.Bold,
		VerticalOptions = LayoutOptions.Center,
		HorizontalOptions = LayoutOptions.Center
	};

	private void RemoveLastRowView()
	{
		int row = rowLabels.Count;
		for (int col = 1; col <= columnLabels.Count; col++)
			RemoveEntry(Table.MakeReference(row, col));
		grid.Children.Remove(rowLabels[^1]);
		rowLabels.RemoveAt(rowLabels.Count - 1);
		grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
	}

	private void RemoveLastColumnView()
	{
		int col = columnLabels.Count;
		for (int row = 1; row <= rowLabels.Count; row++)
			RemoveEntry(Table.MakeReference(row, col));
		grid.Children.Remove(columnLabels[^1]);
		columnLabels.RemoveAt(columnLabels.Count - 1);
		grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
	}

	private void RemoveEntry(string reference)
	{
		if (!entries.Remove(reference, out var entry)) return;
		redCells.Remove(reference);
		grid.Children.Remove(entry);
	}

	private string GetColumnName(int colIndex) => Table.GetColumnName(colIndex);

	private void ShowCell(string reference)
	{
		if (!entries.TryGetValue(reference, out var entry)) return;
		var cell = table.TryGetCell(reference);

		string text = cell?.GetTextToShow(displayMode) ?? "";
		if (entry.Text != text) entry.Text = text;

		bool hasError = cell != null && cell.HasError();
		if (hasError != redCells.Contains(reference))
		{
			if (hasError) { entry.TextColor = Colors.Red; redCells.Add(reference); }
			else { entry.ClearValue(Entry.TextColorProperty); redCells.Remove(reference); }
		}
	}

	private void ShowAllCells()
	{
		foreach (var reference in entries.Keys) ShowCell(reference);
	}

	private void ShowChangedCells()
	{
		var refs = new HashSet<string>(redCells);
		foreach (var c in table.GetAllCells()) refs.Add(c.Reference);
		foreach (var reference in refs) ShowCell(reference);
	}

	private void CommitAllEntries()
	{
		if (displayMode != DisplayMode.Expression) return;
		foreach (var (reference, entry) in entries)
		{
			string text = entry.Text ?? "";
			string current = table.TryGetCell(reference)?.Expression ?? "";
			if (text != current) table.SetCellExpression(reference, text);
		}
	}

	private void Entry_Focused(object? sender, FocusEventArgs e)
	{
		if (sender is not Entry entry) return;
		selectedRow = Grid.GetRow(entry);
		selectedCol = Grid.GetColumn(entry);
		string reference = Table.MakeReference(selectedRow, selectedCol);
		cellNameLabel.Text = reference;
		textInput.Text = table.TryGetCell(reference)?.Expression ?? "";
	}

	private void Entry_Unfocused(object? sender, FocusEventArgs e)
	{
		if (sender is not Entry entry || displayMode != DisplayMode.Expression) 
			return;

		string reference = Table.MakeReference(Grid.GetRow(entry), Grid.GetColumn(entry));

		if (!entries.TryGetValue(reference, out var current) || !ReferenceEquals(current, entry))
			return;
		
		if ((entry.Text ?? "") == (table.TryGetCell(reference)?.Expression ?? ""))
			return;

		table.SetCellExpression(reference, entry.Text ?? "");
		table.CalculateAll();
		ShowChangedCells();
		ShowStatusFor(reference);
	}

	private void TextInput_Completed(object? sender, EventArgs e)
	{
		string reference = Table.MakeReference(selectedRow, selectedCol);
		table.SetCellExpression(reference, textInput.Text ?? "");
		table.CalculateAll();
		ShowChangedCells();
		ShowStatusFor(reference);
	}

	private void ShowStatusFor(string reference)
	{
		var cell = table.TryGetCell(reference);
		if (cell == null || cell.IsEmpty())
			statusLabel.Text = $"{reference}: порожня клітинка";
		else statusLabel.Text = cell.HasError() ? $"{reference}: {cell.ErrorMessage}" : $"{reference}: вираз правильний";
	}

	private async void CalculateButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		table.CalculateAll();
		ShowChangedCells();

		var errors = table.GetCellsWithErrors();
		if (errors.Count == 0)
		{
			statusLabel.Text = "Таблицю пораховано, помилок немає";
			return;
		}
		statusLabel.Text = $"Знайдено помилок: {errors.Count}";
		string list = string.Join("\n", errors.Take(15).Select(c => $"{c.Reference}: {c.ErrorMessage}"));
		
		if (errors.Count > 15)
			list += $"\n... і ще {errors.Count - 15}";
		await DisplayAlertAsync("Помилки у виразах", list, "OK");
	}

	private void ModeButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		table.CalculateAll();
		displayMode = displayMode == DisplayMode.Expression ? DisplayMode.Value : DisplayMode.Expression;
		ModeButton.Text = displayMode == DisplayMode.Expression ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";

		grid.InputTransparent = displayMode == DisplayMode.Value;
		ShowChangedCells();
		statusLabel.Text = displayMode == DisplayMode.Expression
			? "Показано вирази (можна редагувати)"
			: "Показано значення (лише перегляд)";
	}

	private async void SaveButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		string? name = await DisplayPromptAsync("Збереження", "Введіть назву файлу:",
			"Зберегти", "Скасувати", initialValue: "table");

		if (string.IsNullOrWhiteSpace(name))
			return;

		try
		{
			string path = Path.Combine(FileSystem.AppDataDirectory, name.Trim() + ".json");
			fileStorage.SaveToFile(table, path);
			statusLabel.Text = $"Збережено: {path}";
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
		}
	}

	private async void ReadButton_Clicked(object? sender, EventArgs e)
	{
		var files = Directory.GetFiles(FileSystem.AppDataDirectory, "*.json")
			.Select(Path.GetFileNameWithoutExtension).ToList();
		string hint = files.Count > 0 ? "Є файли: " + string.Join(", ", files) : "Збережених файлів ще немає";
		string? name = await DisplayPromptAsync("Читання", $"Введіть назву файлу.\n{hint}",
			"Відкрити", "Скасувати", initialValue: files.FirstOrDefault() ?? "table");
		if (string.IsNullOrWhiteSpace(name))
			return;

		try
		{
			string path = Path.Combine(FileSystem.AppDataDirectory, name.Trim() + ".json");
			var loaded = fileStorage.LoadFromFile(path);
			loaded.CalculateAll();
			table = loaded;
			selectedRow = selectedCol = 1;
			CreateGrid();
			statusLabel.Text = $"Прочитано: {path}";
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Помилка", $"Не вдалося прочитати файл: {ex.Message}", "OK");
		}
	}

	private async void ExitButton_Clicked(object? sender, EventArgs e)
	{
		bool exit = await DisplayAlertAsync("Вихід", "Ви дійсно хочете вийти з програми?", "Так", "Ні");
		if (exit)
			Application.Current?.Quit();
	}

	private async void HelpButton_Clicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("Довідка",
			"Програма — електронна таблиця з обчисленням виразів.\n\n" +
			"Вираз вводиться в клітинку (знак \"=\" на початку необов'язковий). У виразі можна використовувати:\n" +
			" • цілі числа довільної довжини;\n" +
			" • круглі дужки;\n" +
			" • імена клітинок, наприклад A3 або B12;\n" +
			" • операції: + - * / (бінарні), mod, div, унарні + -, inc (додати 1), dec (відняти 1).\n\n" +
			"Приклад: =(A1+B2)*3 mod 5 - inc A1\n\n" +
			"Кнопки:\n" +
			" • Порахувати — перевірити всі вирази й обчислити їх;\n" +
			" • Режим — перемикає ВИРАЗ / ЗНАЧЕННЯ;\n" +
			" • Додати/Видалити рядок і стовпчик — для виділеної клітинки;\n" +
			" • Зберегти / Прочитати — робота з файлом.\n\n" +
			"Помилки показуються червоним, а внизу вікна — пояснення з позицією.\n\n" +
			"Автор: Демчук Дмитро, студент 2 курсу.\n" +
			"Лабораторна робота №1 з ООП: «Розробка програми для роботи з електронними таблицями».",
			"OK");
	}

	private void AddRowButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		table.AddRow();
		AddRowView(table.RowCount);
		statusLabel.Text = $"Рядків: {table.RowCount}";
	}

	private void AddColumnButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		table.AddColumn();
		AddColumnView(table.ColumnCount, createEntries: true);
		statusLabel.Text = $"Стовпчиків: {table.ColumnCount}";
	}

	private void DeleteRowButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		int before = table.RowCount;
		table.DeleteRow(selectedRow);
		if (table.RowCount == before)
		{
			statusLabel.Text = "Не можна видалити останній рядок";
			return;
		}

		RemoveLastRowView();
		selectedRow = Math.Min(selectedRow, table.RowCount);
		table.CalculateAll();
		ShowAllCells();
		statusLabel.Text = $"Видалено рядок. Рядків: {table.RowCount}";
	}

	private void DeleteColumnButton_Clicked(object? sender, EventArgs e)
	{
		CommitAllEntries();
		int before = table.ColumnCount;
		table.DeleteColumn(selectedCol);
		if (table.ColumnCount == before)
		{
			statusLabel.Text = "Не можна видалити останній стовпчик";
			return;
		}

		RemoveLastColumnView();
		selectedCol = Math.Min(selectedCol, table.ColumnCount);
		table.CalculateAll();
		ShowAllCells();
		statusLabel.Text = $"Видалено стовпчик. Стовпчиків: {table.ColumnCount}";
	}
}
