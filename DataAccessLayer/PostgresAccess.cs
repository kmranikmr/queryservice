using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
namespace DataAccess
{
    public class PostgresAccess<T>
    {
        public string _connectionString { get; set; }
        public PostgresAccess(string connString)
        {
            _connectionString = connString;
        }
        public string GetSql(string sql, string filter, string offset, string pageSize, bool forward, string PageFilterType)
        {
            string stmt = sql;
            string quotes = string.Empty;
            if ( string.IsNullOrEmpty(offset) )
            {
                offset = "0";
            }
            string[] joinkeywords = { "inner", "join", "outer", "left", "right" };
            if (!string.IsNullOrEmpty(PageFilterType) && PageFilterType.ToLower() == "string")
                quotes = "'";
            bool hasRowInformation = false;
            if (forward)
            {
                stmt = "select * from "+ "(" + stmt + ")A ";
                Console.WriteLine(" pos 1 " + stmt);
                // if (sql.ToLower().Contains("where"))
                //  {
                //     stmt += " and " + filter + " > " +  offset +  " order by " + filter + " " + " limit " + pageSize;
                //  }
                // else
                if (sql.Contains("join"))
                {
                    stmt = "";
                    string[] tok = sql.Split(" ");
                    int pos = 0;
                    string tablename = "";
                    string aliasPresent = "";
                    bool SelectAll = false;
                    
                    foreach ( string str in tok)
                    {
                        if (str.ToLower().Trim() == "*")
                        {
                            SelectAll = true;
                        }
                        if ((tablename == "") && ( SelectAll || str.Contains("rowid") ) )
                        {
                            hasRowInformation = true;
                        }
                        if ( str.ToLower().Trim() == "from" )
                        {
                            tablename = tok[pos + 1];
                            if (tok[pos + 2].ToLower() == "as")
                            {
                                aliasPresent = tok[pos + 3].ToLower();
                            }
                            else
                            {
                                aliasPresent = tok[pos + 2].ToLower();
                                if (aliasPresent.Contains("(") || aliasPresent.Contains("select"))
                                {
                                    aliasPresent = "";//hack;;
                                }
                            }
                            break;
                        }
                        pos++;
                    }
                    if (SelectAll)//select all
                    {
                        bool isKeyword = joinkeywords.Any(x => x == aliasPresent);
                        if (isKeyword && filter == "rowid")
                        {
                            sql = sql.Replace("*", $"*, {tablename}.rowid AS rowid1 ");
                            stmt = "select * from " + "(" + sql + ")A ";
                            stmt = stmt + " where " + "rowid1 " + " > " + offset + " order by rowid1" + " limit " + pageSize;
                            Console.WriteLine(" pos key " + stmt);
                        }
                        else if ( !isKeyword && filter == "rowid")
                        {
                            sql = sql.Replace("*", $"*, {aliasPresent}.rowid AS rowid1 ");
                            stmt = "select * from " + "(" + sql + ")A ";
                            stmt = stmt + " where " + "rowid1 " + " > " + offset + " order by rowid1" + " limit " + pageSize;
                            Console.WriteLine(" pos not key " + stmt);
                        }
                    }
                    else//let's assume only one rowid will be present
                    {
                        if (hasRowInformation)
                        {
                            if (stmt == "")
                            {
                                stmt = "select * from " + "(" + sql + ")A ";
                            }
                            stmt += " where " + filter + " > " + offset + " order by " + filter + " " + " limit " + pageSize;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    if ( sql.Contains("*") || sql.Contains("rowid") )
                    {
                       
                        stmt += " where " + filter + " > " + offset + " order by " + filter + " " + " limit " + pageSize;
                        Console.WriteLine(" pos 2 " + stmt);
                    }
                    else
                    {

                    }

                }
            }
            else
            {
               // if (sql.ToLower().Contains("where"))
               // {
               //     stmt += " and " + filter + " <= " +  offset +  " order by " + filter + " " + " limit " + pageSize;
               // }
               /// else
                {
                    stmt += " where " + filter + " <= " +  offset +  " order by " + filter + " " + " limit " + pageSize;

                }
            }
            Console.WriteLine(" pos 3 " + stmt);
            return stmt;
        }
        public string GetDataTypeFromPostGres(Type FieldType)
        {
            if (FieldType == typeof(string))
                return "string";
            else if (FieldType == typeof(System.String))
                return "string";
            else if (FieldType == typeof(System.DateTime))
                return "string";
            else
                return "Double";
        }
        public List<List<string>> LoadAllSQL(String query , List<string> fields, int limit, ref List<HeaderData> HeaderList)
        {
            
            List<List<string>> dataList = new List<List<string>>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))

            {
                if ( !query.ToLower().Contains("limit"))
                {
                    query += " limit " + limit;
                }
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))

                {
                    connection.Open();
                    List<int> filterIndex = new List<int>();
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (dataList.Count == 0)
                            {
                                List<string> columns = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string field = reader.GetName(i);
                                    Type fieldType = reader.GetFieldType(i);
                                    if (fields.Contains(field))
                                    {
                                        HeaderList.Add(new HeaderData { Header = field, DataType = GetDataTypeFromPostGres(fieldType)});
                                        filterIndex.Add(i);
                                    }

                                }
                                //dataList.Add(columns);
                            }

                            var l = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (filterIndex.Contains(i))
                                {
                                    var v = Convert.ToString(reader.GetValue(i));
                                    l.Add(v);
                                }
                            }
                            dataList.Add(l);
                            
                        }
                    }


                }
                return dataList;
            }
         
        }

        public List<List<string>> LoadAllSQL2(String query, List<string> fields, int limit, ref List<string> HeaderList)
        {

            List<List<string>> dataList = new List<List<string>>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))

            {
                if (!query.ToLower().Contains("limit"))
                {
                    query += " limit " + limit;
                }
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))

                {
                    connection.Open();
                    List<int> filterIndex = new List<int>();
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (dataList.Count == 0)
                            {
                                List<string> columns = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string field = reader.GetName(i);
                                    Type fieldType = reader.GetFieldType(i);
                                    if (fields.Count > 0)
                                    {
                                        if (fields.Contains(field))
                                        {
                                            HeaderList.Add(field);
                                            filterIndex.Add(i);
                                        }
                                    }
                                    else
                                    {
                                        HeaderList.Add(field);
                                        filterIndex.Add(i);
                                    }

                                }
                                //dataList.Add(columns);
                            }

                            var l = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (filterIndex.Contains(i))
                                {
                                    var v = Convert.ToString(reader.GetValue(i));
                                    l.Add(v);
                                }
                            }
                            dataList.Add(l);

                        }
                    }


                }
                return dataList;
            }

        }

        public List<List<string>> LoadSQL2(String query, string filter, string offset, string pageSize, ref string next, ref string prev, bool forward, ref List<string> HeaderList, string PageFilterType)
        {
            string sql_stmt = GetSql(query, filter, offset, pageSize, forward, PageFilterType);
            List<List<string>> dataList = new List<List<string>>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql_stmt, connection))

                {
                    connection.Open();
                    int filterIndex = 0;
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (dataList.Count == 0)
                            {
                                List<string> columns = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string field = reader.GetName(i);
                                    Type fieldType = reader.GetFieldType(i);
                                    columns.Add(field);
                                    HeaderList.Add( field );

                                    if (field == filter)
                                    {
                                        //  HeaderList.Add(new HeaderData { Header = field, DataType = GetDataTypeFromPostGres(fieldType) });
                                        filterIndex = i;
                                    }
                                }
                                // dataList.Add(columns);
                            }

                            var l = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var v = Convert.ToString(reader.GetValue(i));
                                l.Add(v);
                                if (i == filterIndex)
                                {
                                    if (prev == "")
                                        prev = v;

                                    next = v;
                                }
                            }
                            dataList.Add(l);
                        }
                    }
                }

                return dataList;

            }
        }
        public List<List<string>> LoadSQL(String query, string filter, string offset, string pageSize, ref string next, ref string prev, bool forward, ref List<HeaderData> HeaderList, string PageFilterType)
        {
            string sql_stmt = GetSql(query, filter, offset, pageSize, forward, PageFilterType);
            List<List<string>> dataList = new List<List<string>>();

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql_stmt, connection))

                {
                    connection.Open();
                    int filterIndex = 0;
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (dataList.Count == 0)
                            {
                                List<string> columns = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string field = reader.GetName(i);
                                    Type fieldType = reader.GetFieldType(i);
                                    columns.Add(field);
                                    HeaderList.Add(new HeaderData { Header = field, DataType = GetDataTypeFromPostGres(fieldType) });

                                    if ( field == filter)
                                    {
                                      //  HeaderList.Add(new HeaderData { Header = field, DataType = GetDataTypeFromPostGres(fieldType) });
                                        filterIndex = i;
                                    }
                                }
                               // dataList.Add(columns);
                            }
                            
                            var l = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var v = Convert.ToString(reader.GetValue(i));
                                l.Add(v);
                                if ( i == filterIndex)
                                {
                                    if (prev == "")
                                        prev = v;
                                    
                                        next = v;
                                }
                            }
                            dataList.Add(l);
                        }
                    }
                }
                
                return dataList;

            }
        }

        public List<string> GetColumnNames(string query)
        {
            try
            {
                List<string> Columns = new List<string>();
                HashSet<string> UniqueNames = new HashSet<string>();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    DataTable dt = new DataTable();
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                    {

                        adapter.Fill(dt);
                        for( int i = 0; i < dt.Columns.Count; i++ )
                        {
                            DataColumn col = dt.Columns[i];
                            if (UniqueNames.Add(col.ColumnName))
                            {
                                Columns.Add( col.ColumnName);
                            }
                            else
                            {
                                Columns.Add(col.ColumnName + i);
                            }
                        }
                    }
                    return Columns;
                }
               
            }
            catch (Exception ex)
            {

            }
           return null;
        }
        public List<List<string>> GetResultTable(String query, string filter, string offset, string pageSize, ref string next, ref string prev, bool forward)
        {


            string sql_stmt = GetSql(query, filter, offset, pageSize, forward, "");
            List<List<string>> dataList = new List<List<string>>();
            DataTable dt = new DataTable();
            string filterName = "";
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql_stmt, connection))
                    {
                       
                        adapter.Fill(dt);
                        
                        foreach ( DataRow row in dt.Rows)
                        {
                            var l = new List<string>();
                            
                            foreach ( DataColumn col in dt.Columns)
                            {
                                if (dataList.Count == 0)
                                {
                                    l.Add(col.ColumnName);
                                }
                                else
                                {
                                    string v = row[col].ToString();
                                    l.Add(v);
                                    if (col.ColumnName == filter)
                                    {
                                        if (prev == "")
                                            prev = v;

                                        next = v;
                                    }
                                }
                            }
                            dataList.Add(l);
                        }
                    }
                }

                //NLogger.Debug($"GetResultTable took {stopwatch?.Elapsed.TotalSeconds} seconds - {commandText}");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
               
            }

            return dataList;

        }
    }
}
