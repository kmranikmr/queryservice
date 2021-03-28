using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryAntlr
{
    public class Program
    {
        static public void Main(String[] args)
        {
            string dd = "schema1_4244_39.model1";
            string q = "(schema1_4244_39.model1.host search 1)";// and (schema1_4244_39.model1.remote search \"ANIP\")";
            var inputStream = new AntlrInputStream(q);
            var lexer = new HqlLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var xparser = new HqlParser(commonTokenStream) { BuildParseTree = true };

            var visitor = new AntlrTest.HqlVistorNew(null);
            visitor.Visit(xparser.complexExpr());
            string ind = "";
            var query = visitor.GetQuery(out ind);
            int g = 0;
        }
    }
}
