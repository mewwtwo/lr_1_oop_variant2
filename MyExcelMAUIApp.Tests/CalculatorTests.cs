using System.Numerics;
using MyExcelMAUIApp.Parsing;

namespace MyExcelMAUIApp.Tests;

public class CalculatorTests
{
	private static BigInteger Calc(string expression)
	{
		var cells = new Dictionary<string, BigInteger> { ["A1"] = 10, ["A2"] = 3 };
		return Calculator.Evaluate(expression, name => cells.GetValueOrDefault(name));
	}

	[Theory]
	[InlineData("2 + 3", 5)]
	[InlineData("10 - 4", 6)]
	[InlineData("6 * 7", 42)]
	[InlineData("20 / 6", 3)]
	[InlineData("20 div 6", 3)]
	[InlineData("20 mod 6", 2)]
	[InlineData("-5", -5)]
	[InlineData("+5", 5)]
	[InlineData("inc 5", 6)]
	[InlineData("dec 5", 4)]
	[InlineData("2 + 3 * 4", 14)]
	[InlineData("(2 + 3) * 4", 20)]
	[InlineData("A1 + A2", 13)]
	[InlineData("inc A1 * 2", 22)]
	[InlineData("INC 5", 6)]
	public void Calculates(string expression, int expected)
	{
		Assert.Equal(new BigInteger(expected), Calc(expression));
	}

	[Fact]
	public void SupportsVeryBigNumbers()
	{
		var result = Calc("99999999999999999999 * 99999999999999999999");

		Assert.Equal(BigInteger.Parse("9999999999999999999800000000000000000001"), result);
	}

	[Theory]
	[InlineData("1 +")]
	[InlineData("(1 + 2")]
	[InlineData("1 + 2)")]
	[InlineData("1 $ 2")]
	[InlineData("* 5")]
	public void WrongSyntaxThrowsError(string expression)
	{
		Assert.Throws<ArgumentException>(() => Calc(expression));
	}

	[Fact]
	public void DivisionByZeroThrowsError()
	{
		Assert.Throws<DivideByZeroException>(() => Calc("5 / 0"));
	}
}
