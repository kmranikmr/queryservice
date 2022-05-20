using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RestSharp;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using QueryService.Model;
using QueryEngine.Model;
using Npgsql;
using System.Data;
using Newtonsoft.Json;

namespace QueryEngine
{
    public static class Helper
    {
        public static bool ExecuteQuery(string query, string connectionString)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {

                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))

                    {
                        var ret = command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static List<TableInfo> GetTableInfo(string connectionString, string query, string tableName)
        {
            List<TableInfo> result = new List<TableInfo>();
            try
            {
                
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))

                    {
                        using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            dataAdapter.Fill(dt);

                            foreach (DataRow row in dt.Rows)
                            {
                                var tableInfo = new TableInfo
                                {
                                    TableName = tableName,
                                    ColumnName = row[0].ToString(),
                                    DataType = row[1].ToString()
                                };
                                result.Add(tableInfo);
                            }
                        }

                    }
                }

            }catch(Exception ex)
            {
                return null;
            }
            return result;
        }

        public static string[] GetTableNames(string query)
        {
            List<string> tableNames = new List<string>();
            string[] toks = query.ToLower().Split(' ');
            bool getTable = false;
            for (int i = 0; i < toks.Length; i++)
            {
                if (getTable)
                {
                    if (toks[i] != " " && toks[i] != "")
                    {
                        Console.WriteLine(toks[i]);
                        tableNames.Add(toks[i]);
                        getTable = false;
                    }
                    else
                        continue;
                }
                if (toks[i] == "from" || toks[i] == "join")
                {
                    if ( i <=  toks.Length - 1)
                    {
                        getTable = true;
                    }
                }
               
            }
            
            return tableNames.ToArray();
        }
        public static string GetMappedTableName(string schemaName, QueryRequest queryRequest, string authorization, int userId)
        {
            string schema = schemaName;

            if (!queryRequest.isWorkflow)
            {
                List<int> sb = new List<int>();
                int index = 0;
                do
                {
                    index = schemaName.ToLower().IndexOf("schema", index);

                    if (index != -1)
                    {
                        sb.Add(index);
                        int endIndex = schema.ToLower().IndexOf(".", index);
                        string replacement = schema.Substring(index, endIndex - index);
                        if (!replacement.Contains("_"))
                        {
                            string replace = "";
                            if (queryRequest.SearchDestination == DestinationType.ElasticSearch)
                            {

                                replace = Regex.Replace(schema, replacement, replacement + "_" + queryRequest.ProjectId + "_" + userId);
                            }
                            else
                            {
                                replace = Regex.Replace(schema.ToLower(), replacement, replacement + "_" + queryRequest.ProjectId + "_" + userId);
                            }
                            schema = replace;
                        }
                        index++;
                    }
                } while (index != -1);
            }
            else
            {

            }
                return schema;
        }
        public static async Task<string> GetMappedTableName(QueryRequest queryRequest , string authorization, int userId , string dataservice)
        {
            Console.WriteLine("Queryrequest isWorkflow:" + queryRequest.isWorkflow);
            if (!queryRequest.isWorkflow)
            {
                int startIndex = queryRequest.QueryString.ToLower().IndexOf("schema");
                List<int> sb = new List<int>();
                string query_with_join = "";
                string join_other_tablename = "";
                bool have_join = false;
                if (queryRequest.QueryString.ToLower().Contains("join"))
                {
                    have_join = true;
                    string[] tok = queryRequest.QueryString.ToLower().Split(' ');
                    var otherTable = tok.Where(x => x.Contains(".schema") && x.IndexOf(".schema") != x.LastIndexOf('.'));
                    // endIndex = request.QueryString.ToLower().IndexOf(".schema", startIndex);
                    if (otherTable != null && otherTable.Count() > 0)
                    {
                        var other_tablename = otherTable.ElementAt(0);
                        var splitted = other_tablename.Split(".");
                        string other_tablename2 = "";
                        if (splitted.Count() == 3)
                        {
                            other_tablename2 = splitted[2];
                        }
                        var projectname = other_tablename.Split(".");
                        var client = new RestClient($"{dataservice}:6002/api/Projects/GetProjectIdByName/{userId}/{projectname[0]}");
                        var requestRest = new RestRequest(Method.GET);
                        requestRest.AddHeader("Authorization", authorization);

                        IRestResponse response = await client.ExecuteAsync(requestRest);

                        if (response != null && projectname.Length >= 2)
                        {
                            var rep = queryRequest.QueryString.ToLower().Replace(other_tablename, "XXX");
                            queryRequest.QueryString = rep;
                            query_with_join = projectname[1] + "_" + response.Content + "_" + userId;
                            join_other_tablename = other_tablename2;
                        }
                    }
                }

                int index = 0;
                do
                {
                    index = queryRequest.QueryString.ToLower().IndexOf("schema", index);

                    if (index != -1)
                    {
                        sb.Add(index);
                        int endIndex = queryRequest.QueryString.ToLower().IndexOf(".", index);
                        string replacement = queryRequest.QueryString.ToLower().Substring(index, endIndex - index);
                        if (!replacement.Contains("_"))
                        {
                            string replace = "";
                            if (queryRequest.SearchDestination == DestinationType.ElasticSearch ||
                                queryRequest.SearchDestination == DestinationType.Mongo)
                            {

                                replace = Regex.Replace(queryRequest.QueryString, replacement, replacement + "_" + queryRequest.ProjectId + "_" + userId);
                            }
                            else
                            {
                                replace = Regex.Replace(queryRequest.QueryString.ToLower(), replacement, replacement + "_" + queryRequest.ProjectId + "_" + userId);
                            }
                            queryRequest.QueryString = replace;
                        }
                        index++;
                    }
                } while (index != -1);
                if (have_join)
                {
                    var rep = queryRequest.QueryString.ToLower().Replace("xxx", query_with_join + "." + join_other_tablename);
                    queryRequest.QueryString = rep;
                    have_join = false;
                }

                return queryRequest.QueryString;
            }
            else
            {
                //return null;
                Console.WriteLine("is Workflow");
                string[] tableNames = GetTableNames(queryRequest.QueryString);
                foreach (var t in tableNames)
                {
                    Console.WriteLine(t);
 
                }
                Console.WriteLine(dataservice);
                var client = new RestClient($"{dataservice}:6002/api/workflowprojects/workflowmodelnames/{queryRequest.ProjectId}/{queryRequest.SchemaVersionId}");
                var requestRest = new RestRequest(Method.POST);
                requestRest.AddHeader("Authorization", authorization);
                requestRest.AddHeader("Accept", "application/json");
                
                var json = JsonConvert.SerializeObject(tableNames);
                requestRest.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(requestRest);

                 
                if ( response != null)
                {
                    Console.WriteLine("have response");
                    Console.WriteLine(response.Content);
                    string[] tables = JsonConvert.DeserializeObject<string[]>(response.Content);

                    for ( int i = 0; i < tables.Length; i++)
                    {
                        if (tableNames.Length > i)
                        {
                            Console.WriteLine($"replace{tableNames[i]} with {tables[i]}");
                            queryRequest.QueryString = queryRequest.QueryString.ToLower().Replace(tableNames[i], tables[i]);
                        }
                    }
                }

                return queryRequest.QueryString;
                //var requestRest = new RestRequest(Method.GET);
                // requestRest.AddHeader("Authorization", authorization);

                //  IRestResponse response = await client.ExecuteAsync(requestRest);
            }
        }

    }
}

