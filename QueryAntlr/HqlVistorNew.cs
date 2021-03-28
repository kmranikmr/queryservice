using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DataAccess;
using System.Linq;
namespace AntlrTest
{
    public class HqlVistorNew : HqlBaseVisitor<object>
    {
        private TypeConfig _typeConfig { get; set; }
        private string SmallExpression { get; set; }
        private bool SmallExpressionStart { get; set; }
        private bool SmallExpressionEnd { get; set; }
        private ESBuilder esBuilder;
        public HqlVistorNew(TypeConfig typeConfig)
        {
            Models = new HashSet<string>();
            _typeConfig = typeConfig;
            esBuilder = new ESBuilder();
        }
        public StringBuilder queryBuilder = new StringBuilder();
        
        public HashSet<string> Models;
        public bool UseElastic;
        public override object VisitBracesComplexExpression([NotNull] HqlParser.BracesComplexExpressionContext context)
        {
            if (context.Start.InputStream.ToString().Contains("search")||
                context.Start.InputStream.ToString().Contains("range")||
                context.Start.InputStream.ToString().Contains("match"))//check upfront if elastic ..for now  we assume all elastic
            {
                //useElastic
                UseElastic = true;
            }
            return base.VisitBracesComplexExpression(context);
        }
        public override object VisitSearch([NotNull] HqlParser.SearchContext context)
        {
            return base.VisitSearch(context);
        }
        public override object Visit(IParseTree tree)
        {
            return base.Visit(tree);
        }

        public override object VisitQuery([NotNull] HqlParser.QueryContext context)
        {
            return base.VisitQuery(context);
        }
        public override object VisitField([NotNull] HqlParser.FieldContext context)
        {
            return base.VisitField(context);
        }
        public override object VisitEq([NotNull] HqlParser.EqContext context)
        {
            return base.VisitEq(context);
        }
        public override object VisitMultipleComplexExpression([NotNull] HqlParser.MultipleComplexExpressionContext context)
        {
            //need beter logic
            if ( context.Start.InputStream.ToString().Contains("search")
                || context.Start.InputStream.ToString().Contains("match")
               || context.Start.InputStream.ToString().Contains("range"))//check upfront if elastic ..for now  we assume all elastic
            {
                //useElastic
                UseElastic = true;
            }
            return base.VisitMultipleComplexExpression(context);
        }

        public override object VisitTermExpr([NotNull] HqlParser.TermExprContext context)
        {
            return base.VisitTermExpr(context);
        }
        public override object VisitExistsExpr([NotNull] HqlParser.ExistsExprContext context)
        {
            return base.VisitExistsExpr(context);
        }
        public override object VisitErrorNode(IErrorNode node)
        {
            return base.VisitErrorNode(node);
        }
        public override object VisitAnd([NotNull] HqlParser.AndContext context)
        {
            return base.VisitAnd(context);
        }
        public override object VisitOr([NotNull] HqlParser.OrContext context)
        {
            return base.VisitOr(context);
        }
        public override object VisitTerminal(ITerminalNode node)
        {
            if ( UseElastic )
            {
                esBuilder.TermQuery(node);
                return base.VisitTerminal(node);
            }
            var k = _typeConfig.ModelInfoList.Any(x=>node.Symbol.Text.Contains(x.ModelName)) ;// node.Symbol.Text)
            if ( node.Symbol.Text.Contains(".") && k == true)
            {
               // SqlBuilder.WhereBuilder(node);
                string modelName = node.ToString().Split('.')[0];
                if (!Models.Contains(modelName))
                {
                    Models.Add(modelName);
                }
            }

            if ((SmallExpressionStart == false) && (node.Symbol.Text == "(" || k == true))
            {                
                SmallExpressionStart = true;
            }
            if(SmallExpressionStart && node.Symbol.Type == HqlParser.SEARCH)
            {
                //elastic
                SqlBuilder.WhereQuery();
            }
            if (SmallExpressionStart || node.Symbol.Type == HqlLexer.AND || node.Symbol.Type == HqlLexer.OR)
            {
                SqlBuilder.WhereBuilder(node);
            }
            if (SmallExpressionStart && node.Symbol.Text == ")")
            {
                SmallExpressionEnd = true;
                SmallExpressionStart = false;
                queryBuilder.Append(SqlBuilder.WhereQuery());
                
                SqlBuilder.ClearWhereQuery();
                if(Models.Count == 2)
                {
                   string join=   Models.ElementAt(0) + "  Join  " + Models.ElementAt(1) + " on " + Models.ElementAt(0) + ".id=" +  Models.ElementAt(1)+ ".id where ";
                    queryBuilder.Insert(0, join);
                }
            }

          
            return base.VisitTerminal(node);
        }

        public override object VisitValue([NotNull] HqlParser.ValueContext context)
        {
            return base.VisitValue(context);
        }

        public string GetQuery(out string indexName)
        {
            if (queryBuilder.Length > 0)
            {
                indexName = "";
                return queryBuilder.ToString();
            }
            else if (UseElastic)
            {
                esBuilder.searchHelper.EvaluateRParam();
                if (esBuilder.searchHelper.useIndices)
                {
                    indexName = esBuilder.searchHelper.indexes.Count > 0 ? esBuilder.searchHelper.indexes[0] : "";
                }
                else
                {
                    indexName = "";
                }
                string retString = esBuilder.searchHelper.GetOperandExp();
                return retString;
            }
            indexName = "";
            return "";
        }
    }
}

