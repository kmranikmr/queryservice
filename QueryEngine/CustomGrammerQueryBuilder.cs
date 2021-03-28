using Antlr4.Runtime;
using AntlrTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEngine
{
    public class CustomGrammerQueryBuilder
    {
        public string IndexName;
        public CustomGrammerQueryBuilder()
        {

        }
        public string GetESQuery(string q)
        {
            IndexName = "";
            var inputStream = new AntlrInputStream(q);
            var lexer = new HqlLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var xparser = new HqlParser(commonTokenStream) { BuildParseTree = true };

            var visitor = new AntlrTest.HqlVistorNew(null);
            visitor.Visit(xparser.complexExpr());
           
            var query = visitor.GetQuery(out IndexName);
            return query;
        }
        public void Test(string q)
        {
            var inputStream = new AntlrInputStream(q);
            var lexer = new HqlLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var xparser = new HqlParser(commonTokenStream) { BuildParseTree = true };

           // var visitor = new HqlVistorNew();
          //  visitor.Visit(xparser.complexExpr());
        }

    }
}
