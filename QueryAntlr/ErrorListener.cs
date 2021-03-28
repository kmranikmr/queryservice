using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueryAntlr
{
    public class HqlErrorListener : IAntlrErrorListener<IToken>
    {
        public List<string> Errors { get; set; } = new List<string>();
        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Errors.Add($"Error at {charPositionInLine} - {offendingSymbol.Text}");
        }

        void IAntlrErrorListener<IToken>.SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            // throw new NotImplementedException();
            Errors.Add($"Error at {charPositionInLine} - {offendingSymbol.Text}");
        }
    }
}
