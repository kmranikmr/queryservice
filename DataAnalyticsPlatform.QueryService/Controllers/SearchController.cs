using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess;
using QueryService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QueryEngine;
using RestSharp;
using QueryEngine.Model;

namespace DataAnalyticsPlatform.QueryService.Controllers
{
    // [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class SearchController : ControllerBase
    {
        public ESNativeSearch<List<List<string>>> esNativeSearch { get; set; }
        public CustomGrammerQueryBuilder customQueryBuilder { get; set; }
        private IMemoryCache _cache;
        
        //private SearchResult<List<List<string>>> PreviousPage;
        //private SearchResult<List<List<string>>> NextPage;
        //private SearchResult<List<List<string>>> CurrentPage;
        // public SQLNativeSearch sqlNativeSearch { get; set; }
        public SearchController(ESNativeSearch<List<List<string>>> esNativeSearch, CustomGrammerQueryBuilder customBuilder, IMemoryCache Cache)//, SQLNativeSearch sqlNativeSearch )
        {
            this.esNativeSearch = esNativeSearch;
            this.customQueryBuilder = customBuilder;
            _cache = Cache;
            //PreviousPage = new SearchResult<List<List<string>>>();
          //  this.sqlNativeSearch = sqlNativeSearch;

        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
        //[HttpPost]//("/esnative")
        //public async Task<SearchResult<List<List<string>>>> PostQuery([FromBody]QueryRequest request)
        //{
        //    SearchResult<List<List<string>>> result = null;

        //    if (request.SearchDestination == DestinationType.RDBMS)
        //    {
        //        string pageOffset = "0";
        //        bool forward = true;
        //        result =  await esNativeSearch.SearchComplete(request.QueryString, request.Columns);//, request.PageSize, request.PageFilter, _cache, ref pageOffset, ref forward);

        //    }
        //    return result;
        //}
        [HttpPost("[action]")]
        public async Task<SearchResult<List<List<string>>>> TestPreview([FromBody]TestPreviewQuery request)
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            char[] sep = new char[] { ' ', '-', ':' };
            var inputidChanged = request.TaskId.Replace(sep, "_");
            var taskNameChanged = request.TaskName.Replace(sep, "_");
            string query = $"select * from {taskNameChanged}_{inputidChanged} limit {request.Limit}";
            string pageOffset = "0";
            Console.WriteLine(query);
            bool forward = true;
            var result = await esNativeSearch.SearchComplete2(query, new List<string> { }, 10, true);
            if (result != null)
            {
                return result;
            }
            return null;
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult GetTableInfo([FromBody]string[] TableName)
        {
            List<TableInfo> TableList = null;
            for (int i = 0; i < TableName.Length; i++)
            {
                string query = $@"select column_name, data_type from information_schema.columns where table_name = '{TableName[i]}'";
                if (TableList == null)
                {
                    TableList = esNativeSearch.GetTableData(query, TableName[i]);
                }
                else
                {
                    TableList.AddRange(esNativeSearch.GetTableData(query, TableName[i]));
                }
            }
            Console.WriteLine(TableList.ToString());
            return Ok(TableList.ToArray());
        }
        [HttpPost("[action]/{isWorkflow:bool=false}")]//("/esnative")
        public async Task<string> MappedQuery([FromBody]QueryData queryData, [FromHeader] string authorization, bool isWorkflow)
        {
            //request.isWorkflow = isWorkflow;
           
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (queryData.projectId != 0)
            {
                QueryRequest request = new QueryRequest { QueryString = queryData.query , ProjectId = queryData.projectId};
                request.isWorkflow = isWorkflow;
                var requestChanged = await QueryEngine.Helper.GetMappedTableName(request, authorization, userId,esNativeSearch.dataServiceServer);
                return requestChanged;
            }
            return null;
        }

        [HttpPost("[action]")]//("/esnative")
        public bool CreateView([FromBody] ViewData viewData )
        {
            try
            {
                string viewSql = $"Create or replace View \"{viewData.ViewName}\" AS ( {viewData.Query.Replace("\"", "")});";
                QueryEngine.Helper.ExecuteQuery(viewSql, esNativeSearch.GetConnectionString());
                Console.WriteLine(viewSql + " " +esNativeSearch.GetConnectionString());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        [HttpPost("{isWorkflow:bool=false}")]//("/esnative")
        public async Task<SearchResult<List<List<string>>>> Post( [FromBody]QueryRequest request, [FromHeader] string authorization, bool isWorkflow)
        {
            request.isWorkflow = isWorkflow;
            SearchResult<List<List<string>>> result = null;
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if ( request.ProjectId != 0 )
            {
                request.QueryString = await QueryEngine.Helper.GetMappedTableName(request, authorization, userId, esNativeSearch.dataServiceServer);
             /*   int startIndex = request.QueryString.ToLower().IndexOf("schema");
                List<int> sb = new List<int>();
                string query_with_join = "";
                string join_other_tablename = "";
                bool have_join = false;
                if (request.QueryString.ToLower().Contains("join"))
                {
                    have_join = true;
                    string[] tok = request.QueryString.ToLower().Split(' ');
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
                        var client = new RestClient($"http://localhost:6002/api/Projects/GetProjectIdByName/{userId}/{projectname[0]}");
                        var requestRest = new RestRequest(Method.GET);
                        requestRest.AddHeader("Authorization", authorization);

                        IRestResponse response = await client.ExecuteAsync(requestRest);

                        if (response != null && projectname.Length >= 2)
                        {
                            var rep = request.QueryString.ToLower().Replace(other_tablename, "XXX");
                            request.QueryString = rep;
                            query_with_join = projectname[1] + "_" + response.Content + "_" + userId;
                            join_other_tablename = other_tablename2;
                        }
                    }
                }

                int index = 0;
                do
                {
                    index = request.QueryString.ToLower().IndexOf("schema", index);
                    
                    if (index != -1)
                    {
                        sb.Add(index);
                        int endIndex = request.QueryString.ToLower().IndexOf(".", index);
                        string replacement = request.QueryString.ToLower().Substring(index, endIndex - index);
                        if (!replacement.Contains("_"))
                        {
                            string replace = Regex.Replace(request.QueryString.ToLower(), replacement, replacement + "_" + request.ProjectId + "_" + userId);
                            request.QueryString = replace;
                        }
                        index++;
                    }
                } while (index != -1);
                if ( have_join )
                {
                    var rep = request.QueryString.ToLower().Replace("xxx", query_with_join + "." + join_other_tablename);
                    request.QueryString = rep;
                    have_join = false;
                }

    */
                //if (startIndex != -1)
                {
                    //int endIndex = request.QueryString.ToLower().IndexOf(".", startIndex);
                    //string replacement = request.QueryString.ToLower().Substring(startIndex, endIndex - startIndex);
                    //string replace = Regex.Replace(request.QueryString.ToLower(), @"schema\d+", replacement + "_" + request.ProjectId + "_" + userId);
                    //request.QueryString = replace;// request.QueryString.Replace(replacement, replacement + "_" + request.ProjectId + "_" + userId);

                    //if (request.QueryString.ToLower().Contains("join"))
                    //{
                    //    string[] tok = request.QueryString.ToLower().Split(' ');
                    //    var otherTable = tok.Where(x => x.Contains(".schema") && x.IndexOf(".schema") != x.LastIndexOf('.'));
                    //    // endIndex = request.QueryString.ToLower().IndexOf(".schema", startIndex);
                    //    if (otherTable != null && otherTable.Count() > 0)
                    //    {
                    //        var other_tablename = otherTable.ElementAt(0);
                    //        var projectname = other_tablename.Split(".");
                    //        var client = new RestClient($"http://localhost:6002/api/Projects/GetProjectIdByName/{userId}/{projectname[0]}");
                    //        var requestRest = new RestRequest(Method.GET);
                    //        requestRest.AddHeader("Authorization", authorization);
                            
                    //        IRestResponse response = await client.ExecuteAsync(requestRest);
                         
                    //        if (response != null && projectname.Length >= 2)
                    //        {
                    //            var rep = request.QueryString.ToLower().Replace(other_tablename, projectname[1] + "_" + response.Content + "_" + userId);
                    //            request.QueryString = rep;
                    //        }
                    //    }
                    //}
                    Console.WriteLine("Query " + request.QueryString);
                }
                
            }
            //we need to check if project is good for this user
            if ( request.SearchDestination == DestinationType.RDBMS)
            {
                string pageOffset = "0";
                bool forward = true;
                if (request.AllRecords == false)
                {
                    result = esNativeSearch.SearchCurrentAsync(request.QueryString, request.PageIndex, request.PageSize, request.PageFilter, _cache, ref pageOffset, ref forward, request.PageFilterType);

                    var tempresult = await esNativeSearch.SearchAsync(request.QueryString, request.PageIndex, request.PageSize, request.PageFilter, _cache, pageOffset, forward, request.PageFilterType).ConfigureAwait(false);//, PreviousPage, NextPage);
                    bool remove = false;
                    if (result == null)
                    {
                        result = tempresult;
                        remove = true;
                    }
                    if (result != null)
                    {
                       // result.Header = result.Results[0];
                      //  if ( remove == true)
                       // result.Results.RemoveAt(0);
                    }
                    return result;
                }
                else
                {
                    result = await esNativeSearch.SearchComplete(request.QueryString, request.Columns, request.PageSize);
                    if(result != null)
                    {
                        //result.Header = result.Results[0];
                       
                       // result.Results.RemoveAt(0);
                    }
                    return result;
                }
            }
            else if (request.SearchDestination == DestinationType.ElasticSearch)
            {
                string queryEs = "";
                string indexName = "";
                if (request.QueryString.StartsWith("(") == true)
                {
                    queryEs = customQueryBuilder.GetESQuery(request.QueryString);
                    indexName = customQueryBuilder.IndexName;
                }
                else
                {
                    queryEs = request.QueryString;
                    indexName = QueryEngine.Helper.GetMappedTableName(request.SchemaName, request, authorization, userId).ToLower();
                    // queryEs = request.
                }

                result = await esNativeSearch.SearchAsync(queryEs, request.PageIndex, request.PageSize, indexName);
            }
            //var result = await esNativeSearch.SearchAsync(request.QueryString, request.PageIndex, request.PageSize);
           
            return result;
        }

        //public async Task<SearchResult<List<string>>>


    }
}
