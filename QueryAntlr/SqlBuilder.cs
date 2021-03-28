using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace AntlrTest
{
    public class SqlBuilder
    {
        public static StringBuilder where = new StringBuilder();
       public static void WhereBuilder(ITerminalNode node)
        {
            if (node.Symbol.Type == HqlParser.PHRASE)
            {
                where.Append("'" + node.ToString() + "'" );
            }
            else
            {

                where.Append(node.ToString() + " ");
            }
        }

        public static string WhereQuery()
        {
            return where.ToString();
        }

        public static void ClearWhereQuery()
        {
            where.Clear();
        }
 
    }
}
