using System.Numerics;
using Antlr4.Runtime;

namespace MyExcelMAUIApp.Parsing;

public static class Calculator
{
	public static BigInteger Evaluate(string expression, Func<string, BigInteger> cellValue)
	{
		var lexer = new TableManagerLexer(new AntlrInputStream(expression));
		lexer.RemoveErrorListeners();
		lexer.AddErrorListener(new ThrowExceptionErrorListener());
		var tokens = new CommonTokenStream(lexer);
		var parser = new TableManagerParser(tokens);
		parser.RemoveErrorListeners();
		parser.AddErrorListener(new ThrowExceptionErrorListener());
		
		var tree = parser.compileUnit();

		var visitor = new Visitor(cellValue);
		return visitor.Visit(tree);
	}
}
