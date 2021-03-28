using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AntlrTest
{
    public interface IExpression
    {

    }
    public enum TokenType
    {
        lParen,
        rParen,
        field,
        value,
        [Description("should")]
        operOr,
        [Description("must")]
        operAnd,
        operSearch,
        operMatch,
        operRange,
        exp
    }
    public class Expression : IExpression
    {
        public TokenType Type { get; set; }
    }

    public class FieldExpression  : Expression
    {
        public FieldExpression()
        {
            Type = TokenType.field;
        }
         public string searchField { get; set; }
    }

    public class OperatorExpression : Expression
    {
        public OperatorExpression()
        {
            Type = TokenType.operSearch;
        }
        public string searchOperator { get; set; }
    }
    public class ValueExpression : Expression
    {
        public ValueExpression()
        {
            Type = TokenType.value;
        }
        public string value { get; set; }
    }

    public class SearchExpression : Expression
    {
        
        public string expression { get; set; }
        public SearchExpression()
        {
            Type = TokenType.exp;
        }
        
    }

    public class SearchHelper
    {
        public Stack<Expression> Operand;
        public Stack<Expression> Operator;
        public List<string> indexes;
        public bool useIndices;
        public SearchHelper()
        {
            Operand = new Stack<Expression>();
            Operator = new Stack<Expression>();
            indexes = new List<string>();
            useIndices = true;
        }

        public int GetPrecedence(OperatorExpression exp)
        {
            if (exp.searchOperator == "search")
                return 3;
            else if (exp.searchOperator == "match")
                return 3;
            else if (exp.searchOperator == "range")
                return 3;
            else if (exp.searchOperator == "AND")
                return 2;
            else if (exp.searchOperator == "OR")
                return 1;
            return 0;
        }
        public bool StackToken(string token, IToken type)
        {
           
            if (type.Type == HqlLexer.AND)
                StackOperator(new OperatorExpression() { searchOperator = "AND" , Type = TokenType.operAnd});
            else if (type.Type == HqlLexer.OR)
                StackOperator(new OperatorExpression() { searchOperator = "OR" , Type = TokenType.operOr });
            else if (token=="search")//type.Type == HqlLexer.SEARCH)
                StackOperator(new OperatorExpression() { searchOperator = "search"  ,Type = TokenType.operSearch });
            else if (token == "match")//type.Type == HqlLexer.SEARCH)
                StackOperator(new OperatorExpression() { searchOperator = "match", Type = TokenType.operMatch });
            else if (token == "range")//type.Type == HqlLexer.SEARCH)
                StackOperator(new OperatorExpression() { searchOperator = "range", Type = TokenType.operRange });
            else if (token == "(")
                StackOperator(new OperatorExpression() { searchOperator = "(", Type = TokenType.lParen });
            else if (token == ")")
                StackOperator(new OperatorExpression() { searchOperator = ")" , Type = TokenType.rParen });
            else if (token.Contains(".") == true)
                StackOperand(new FieldExpression { searchField = token });
            else
                StackOperand(new ValueExpression { value = token });
            
            return true;
        }
        public bool StackOperator(Expression op)
        {
            if (op.Type == TokenType.rParen)
            {
                EvaluateRParam();
            }
            else if (op.Type != TokenType.lParen)
            {
                EvaluateOperator((OperatorExpression)op);
                
            }
            if (op.Type != TokenType.rParen)
            {
                Operator.Push(op);
            }

            return true;
        }
        public bool StackOperand(Expression op)
        {
            if (op.Type == TokenType.field)
            {
                int index = ((FieldExpression)op).searchField.LastIndexOf(".");
                string field = ((FieldExpression)op).searchField;
                if (index >= 0)
                {
                    string indexStr = ((FieldExpression)op).searchField.Substring(0, index);//get index
                    if (!indexes.Contains(indexStr))
                    {
                        indexes.Add(indexStr);
                    }
                    field = ((FieldExpression)op).searchField.Substring(index+1);
                    Operand.Push(new FieldExpression { searchField = field.Trim() });
                }

            }
            else
            {

                Operand.Push(op);
            }
           

            return true;
        }

        public string boolQuery(TokenType type, List<string> exp)
        {
            string boolQueries = string.Join(",", exp);
            string indexList = "_all";
            if (indexes.Count > 0)
            {
                useIndices = false;
                indexList = string.Join(",", indexes);
            }
            string x = string.Format(
                       
                        "{{" +
                            "\"bool\": {{" +
                                "\"{0}\": [" +
                                    "{1}"+ 
                                "]," +
                                "\"filter\": ["  +
                                 "{{"+
                                    "\"terms\": {{"+
                                        "\"_index\": ["+
                                          "\"{2}\"" +
                                      "]" +
                                   "}}"+
                                  "}}"+
                                "]"+
                            "}}" +
                        "}}"
                   
                    , type.GetDescription(), boolQueries, indexList);
            indexes.Clear();
            return x;

        }

        public bool EvaluateRParam()
        {

            while (Operator.Count != 0 && Operator.Peek().Type != TokenType.lParen)
            {
                Expression operand1 = Operand.Pop();
                Expression operand2 = Operand.Pop();
                Expression operator1 = Operator.Pop();

                if (operand2.Type == TokenType.field)
                {
                    if (operator1.Type == TokenType.operSearch)
                    {
                        SearchExpression exp = new SearchExpression() { expression ="{\""+ "wildcard"+"\""+ ":"+"{\"" + $"{((FieldExpression)operand2).searchField}" +"\""+ ":" + "\"" + $"{((ValueExpression)operand1).value}" + "\"}"+ "}" };
                        // Operand.Push(exp);
                        StackOperand(exp);
                    }
                    else if (operator1.Type == TokenType.operMatch)
                    {
                        SearchExpression exp = new SearchExpression() { expression = "{\"" + "term" + "\"" + ":" + "{\"" + $"{((FieldExpression)operand2).searchField}.keyword" + "\"" + ":" + "\"" + $"{((ValueExpression)operand1).value}" + "\"}" + "}" };
                        // Operand.Push(exp);
                        StackOperand(exp);
                    }
                    else if (operator1.Type == TokenType.operRange)
                    {
                        SearchExpression exp = new SearchExpression() { expression = "{\"" + "range" + "\"" + ":" + "{\"" + $"{((FieldExpression)operand2).searchField}" + "\"" + ":" + "{\"" + $"{((ValueExpression)operand1).value.Split('|')[0]}"+"\""+ ":" + $"{ ((ValueExpression)operand1).value.Split('|')[1] }"  + "}"+ "}" + "}" };
                        // Operand.Push(exp);
                        StackOperand(exp);
                    }
                }
                else if (operand2.Type == TokenType.exp)
                {
                    string query = boolQuery(operator1.Type, new List<string> { ((SearchExpression)operand1).expression, ((SearchExpression)operand2).expression });
                    SearchExpression exp = new SearchExpression() { expression= query };
                    // Operand.Push(exp);
                    StackOperand(exp);
                }
            }
            if (Operator.Count > 0 )
            Operator.Pop();
            return true;
        }


        public bool EvaluateOperator(OperatorExpression Opexp)
        {
            while(Operator.Count != 0 && GetPrecedence(((OperatorExpression)Operator.Peek())) >= GetPrecedence(Opexp) )
            {
                Expression operand1 = Operand.Pop();
                Expression operand2 = Operand.Pop();
                Expression operator1 = Operator.Pop();
                if (operand2.Type == TokenType.field)
                {
                    if (operator1.Type == TokenType.operSearch)
                    {
                        //SearchExpression exp = new SearchExpression() { expression = "{" + $"{ ((FieldExpression)operand2).searchField }" + ":" + $"{ ((ValueExpression)operand1).value }" + "}" };
                        ////Operand.Push(exp);
                        //StackOperand(exp);
                        SearchExpression exp = new SearchExpression() { expression = "{\"" + "wildcard" +"\"" + ":" + "{\"" + $"{((FieldExpression)operand2).searchField}" + "\"" + ":" + "\"" + $"{ ((ValueExpression)operand1).value}" + "\"}" + "}" };
                        // Operand.Push(exp);
                        StackOperand(exp);
                    }
                }
                else if (operand2.Type == TokenType.exp)
                {
                    string query = boolQuery(operator1.Type, new List<string> { ((SearchExpression)operand1).expression, ((SearchExpression)operand2).expression });
                    SearchExpression exp = new SearchExpression() { expression = query };
                    // Operand.Push(exp);
                    StackOperand(exp);
                }
            }
            //Operator.Push(Opexp);
            return true;
        }
        public string GetOperandExp1()
        {
            if ( Operand.Count > 0 )
            {
                Expression exp = Operand.Pop();
                if (exp.Type == TokenType.exp)
                {
                    return "{" +
                    "\"query\":" + ((SearchExpression)exp).expression +
                        "}";
                    
                }
                return "";
            }
            return "";
        }

        public string GetOperandExp()
        {
            if (Operand.Count > 0)
            {
                Expression exp = Operand.Pop();
                if (exp.Type == TokenType.exp)
                {
                    return ((SearchExpression)exp).expression;
                        

                }
                return "";
            }
            return "";
        }
    }

    public class ESBuilder
    {
        public  StringBuilder Terms = new StringBuilder();
        public SearchHelper searchHelper = new SearchHelper();
        public  void TermQuery(ITerminalNode node)
        {
            searchHelper.StackToken(node.Symbol.Text, node.Symbol);
        }

    }
}

