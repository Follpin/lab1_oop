using Antlr4.Runtime;
using System;
using System.IO;

namespace SpreadsheetApp
{
    public class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public static readonly ThrowExceptionErrorListener Instance = new ThrowExceptionErrorListener();

        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            throw new ArgumentException($"Синтаксична помилка на позиції {charPositionInLine + 1}: {msg}", e);
        }

        public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            int offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            throw new ArgumentException($"Лексична помилка на позиції {charPositionInLine + 1}: {msg}", e);
        }
    }
}

