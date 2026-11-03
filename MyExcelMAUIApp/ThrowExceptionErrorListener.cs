using Antlr4.Runtime;

namespace MyExcelMAUIApp.Parsing;

public class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
	public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{

		throw new ArgumentException($"Неправильний вираз (позиція {charPositionInLine + 1}): {msg}", e);
	}

	public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		throw new ArgumentException($"Неправильний вираз (позиція {charPositionInLine + 1}): {msg}", e);
	}
}
