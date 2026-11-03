using System.Numerics;

namespace MyExcelMAUIApp.Models;

public class Cell
{
	public string Reference { get; set; }
	public string Expression { get; set; } = "";
	public BigInteger Value { get; private set; }
	public string ErrorMessage { get; private set; } = "";

	public Cell(string reference)
	{
		Reference = reference;
	}

	public void SetValue(BigInteger value)
	{
		Value = value;
		ErrorMessage = "";
	}

	public void SetError(string message)
	{
		ErrorMessage = message;
	}

	public bool HasError() => ErrorMessage != "";

	public bool IsEmpty() => Expression == "";

	public string GetTextToShow(DisplayMode mode)
	{
		if (mode == DisplayMode.Expression)
			return Expression;
		if (IsEmpty())
			return "";
		return HasError() ? "#ПОМИЛКА" : Value.ToString();
	}
}
