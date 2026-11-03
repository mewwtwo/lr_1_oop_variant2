using System.Numerics;
using Antlr4.Runtime.Misc;

namespace MyExcelMAUIApp.Parsing;

public class Visitor : TableManagerBaseVisitor<BigInteger>
{

	private readonly Func<string, BigInteger> variables;

	public Visitor(Func<string, BigInteger> variables)
	{
		this.variables = variables;
	}

	public override BigInteger VisitCompileUnit([NotNull] TableManagerParser.CompileUnitContext context)
	{
		return Visit(context.expression());
	}

	public override BigInteger VisitNumberExpr([NotNull] TableManagerParser.NumberExprContext context)
	{
		return BigInteger.Parse(context.GetText());
	}

	public override BigInteger VisitIdentifierExpr([NotNull] TableManagerParser.IdentifierExprContext context)
	{
		return variables(context.GetText());
	}

	public override BigInteger VisitParenthesizedExpr([NotNull] TableManagerParser.ParenthesizedExprContext context)
	{
		return Visit(context.expression());
	}

	public override BigInteger VisitUnaryExpr([NotNull] TableManagerParser.UnaryExprContext context)
	{
		var value = Visit(context.expression());
		return context.operatorToken.Type switch
		{
			TableManagerLexer.SUBTRACT => -value,
			TableManagerLexer.INC => value + 1,
			TableManagerLexer.DEC => value - 1,
			_ => value
		};
	}

	public override BigInteger VisitAdditiveExpr([NotNull] TableManagerParser.AdditiveExprContext context)
	{
		var left = WalkLeft(context);
		var right = WalkRight(context);

		if (context.operatorToken.Type == TableManagerLexer.ADD)
		{
			return left + right;
		}
		else
		{
			return left - right;
		}
	}

	public override BigInteger VisitMultiplicativeExpr([NotNull] TableManagerParser.MultiplicativeExprContext context)
	{
		var left = WalkLeft(context);
		var right = WalkRight(context);

		return context.operatorToken.Type switch
		{
			TableManagerLexer.MULTIPLY => left * right,
			TableManagerLexer.DIVIDE => left / right,
			TableManagerLexer.DIV => left / right,
			TableManagerLexer.MOD => left % right,
			_ => throw new NotSupportedException($"Невідомий оператор: {context.operatorToken.Text}")
		};
	}

	private BigInteger WalkLeft(TableManagerParser.ExpressionContext context)
	{
		return Visit(context.GetRuleContext<TableManagerParser.ExpressionContext>(0));
	}

	private BigInteger WalkRight(TableManagerParser.ExpressionContext context)
	{
		return Visit(context.GetRuleContext<TableManagerParser.ExpressionContext>(1));
	}
}
