using DataAccess;
using Elasticsearch.Net;
using Microsoft.Extensions.Caching.Memory;
using QueryEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueryEngine
{
    public class ESNativeSearch<T> : ISearchService<T>
    {
        public ElasticAccess<List<List<string>>> esAccess;
        public PostgresAccess<List<List<string>>> postgresAccess;
        public PostgresAccess<List<List<string>>> postgresDigdagAccess;
        private SearchResult<T> PreviousPage;
        private SearchResult<T> NextPage;
        private SearchResult<T> CurrentPage;
        private string ConnectionString;
        public string dataServiceServer;
        public ESNativeSearch(string connectionstring, string digdagConnectionString, string elastic, string dataservice)
        {
            esAccess = new ElasticAccess<List<List<string>>>(elastic);
            postgresAccess = new PostgresAccess<List<List<string>>>(connectionstring);
            postgresDigdagAccess = new PostgresAccess<List<List<string>>>(digdagConnectionString);
            ConnectionString = connectionstring;
            dataServiceServer = dataservice;
        }
        public string GetConnectionString()
        {
            return ConnectionString;
        }
        public List<TableInfo> GetTableData(string query, string tableName)
        {
            return QueryEngine.Helper.GetTableInfo(ConnectionString, query, tableName);
        }

        public async Task<SearchResult<T>> SearchAsync(string query, int page, int pageSize, string indexName)
        {
            return await Task.Factory.StartNew<SearchResult<T>>(()=>Search(query, page, pageSize, indexName));
        }
        public async Task<SearchResult<T>> SearchAsync(string query, int page, int pageSize)
        {
            return await Task.Factory.StartNew<SearchResult<T>>(() => Search(query, page, pageSize));
        }
        public async Task<SearchResult<T>> SearchComplete(string query, List<string>fields, int limit, bool useDigdag = false)//SearchResult<T> PreviousPage, SearchResult<T> NextPage, SearchResult<List<List<string>>> CurrentPage)
        {
            //T result = (T)(object)postgresAccess.LoadAllSQL(query, fields);// pageFilter, page.ToString(), pageSize.ToString(), ref next, ref prev, forward);
            // if (result == null) return null;
            List<HeaderData> HeaderList = new List<HeaderData>();
            List<List<string>> retDat;
            if (useDigdag)
            {
                retDat = await Task.Run<List<List<string>>>(() => postgresDigdagAccess.LoadAllSQL(query, fields, limit, ref HeaderList));
            }
            else
            {
                retDat = await Task.Run<List<List<string>>>(() => postgresAccess.LoadAllSQL(query, fields, limit, ref HeaderList));
            }
            return new SearchResult<T>
            {

                Results = (T)(object)retDat,
                Header = HeaderList
            };
        }
        public async Task<SearchResult<T>> SearchComplete2(string query, List<string> fields, int limit, bool useDigDag = false)//SearchResult<T> PreviousPage, SearchResult<T> NextPage, SearchResult<List<List<string>>> CurrentPage)
        {
            //T result = (T)(object)postgresAccess.LoadAllSQL(query, fields);// pageFilter, page.ToString(), pageSize.ToString(), ref next, ref prev, forward);
            // if (result == null) return null;
            List<string> HeaderList = new List<string>();
            List<List<string>> retDat;
            if (useDigDag)
            {
                retDat = await Task.Run<List<List<string>>>(() => postgresDigdagAccess.LoadAllSQL2(query, fields, limit, ref HeaderList));
            }
            else
            {
                retDat = await Task.Run<List<List<string>>>(() => postgresAccess.LoadAllSQL2(query, fields, limit, ref HeaderList));
            }
            return new SearchResult<T>
            {

                Results = (T)(object)retDat,
                Header2 = HeaderList
            };
        }
        public SearchResult<T> SearchCurrentAsync(string query, int page, int pageSize, string pageFilter, IMemoryCache cache, ref string pageOffset, ref bool forward, string PageFilterType = "Double")//SearchResult<T> PreviousPage, SearchResult<T> NextPage, SearchResult<List<List<string>>> CurrentPage)
        {
            if (page == 0)
            {
                cache.Remove("PreviousPage");
                cache.Remove("NextPage");
                cache.Remove("CurrentPage");
            }
            cache.TryGetValue("PreviousPage", out PreviousPage);
            cache.TryGetValue("NextPage", out NextPage);
            cache.TryGetValue("CurrentPage", out CurrentPage);
           
            forward = true;
            string pageMarker = "";
            pageOffset = "0";
            if (CurrentPage != null)
            {
                if (page > CurrentPage.Page)
                {
                    //forward motion of scroll pages

                    //use first nextpage as current
                    //get new next page...
                    if (NextPage != null)
                    {
                        PreviousPage = CurrentPage;
                        CurrentPage = NextPage;
                        pageOffset = CurrentPage.NextStart;
                        return CurrentPage;
                    }

                }
                else if (page < CurrentPage.Page)
                {
                    forward = false;
                    //scrollback;
                    if (PreviousPage != null)
                    {
                        //going back..
                        //set current page to previous..and get new previous page
                        NextPage = CurrentPage;
                        CurrentPage = PreviousPage;
                        pageOffset = CurrentPage.NextStart;
                        return CurrentPage;
                    }

                }
                else if (page == CurrentPage.Page)
                {
                    return CurrentPage;
                }
            }
            return null;
        }
        public async Task<SearchResult<T>> SearchAsync(string query, int page, int pageSize, string pageFilter, IMemoryCache cache, string pageOffset, bool forward, string PageFilterType = "Double")//SearchResult<T> PreviousPage, SearchResult<T> NextPage, SearchResult<List<List<string>>> CurrentPage)
        {
            //cache.TryGetValue("PreviousPage", out PreviousPage);
            //cache.TryGetValue("NextPage", out NextPage);
            //cache.TryGetValue("CurrentPage", out CurrentPage);
            //forward = true;
            //string pageMarker = "";
            //pageOffset = "0";
            //if (CurrentPage != null)
            //{
            //    if (page >= CurrentPage.Page)
            //    {
            //        //forward motion of scroll pages

            //        //use first nextpage as current
            //        //get new next page...
            //        if (NextPage != null)
            //        {
            //            PreviousPage = CurrentPage;
            //            CurrentPage = NextPage;
            //            pageOffset = CurrentPage.NextStart;
            //        }

            //    }
            //    else if (page <= PreviousPage.Page)
            //    {
            //        forward = false;
            //        //scrollback;
            //        if (PreviousPage != null)
            //        {
            //            //going back..
            //            //set current page to previous..and get new previous page
            //            NextPage = CurrentPage;
            //            CurrentPage = PreviousPage;
            //            pageOffset = PreviousPage.PreviousStart;
            //        }

            //    }
            //}

           
            var ret = await Task.Run<SearchResult<T>>(() => Search(query, pageOffset, pageSize, pageFilter, forward, PageFilterType)).ConfigureAwait(false);
            var retObj = ret as SearchResult<List<List<string>>>;
            //  retObj.Header = retObj.Results[0];
           //   retObj.Results.RemoveAt(0);
           
            if (CurrentPage != null)
            {
                if (forward)
                    ret.Page = page + 1;
                else
                    ret.Page = page - 1;
            }
            if (forward && CurrentPage == null)
                CurrentPage = ret;
            else if (forward)
                NextPage = ret;
            else if (!forward && CurrentPage == null)
                CurrentPage = ret;
            else if (!forward && CurrentPage != null)
                PreviousPage = ret;
                

            if ( forward && NextPage == null)
            {
                var nextRet = await Task.Run<SearchResult<T>>(() => Search(query, CurrentPage.NextStart, pageSize, pageFilter, forward, PageFilterType));
                var retNextObj = nextRet as SearchResult<List<List<string>>>;
                //retNextObj.Header = retNextObj.Results[0];
                //retNextObj.Results.RemoveAt(0);
                nextRet.Page += 1;
                NextPage = nextRet;
            }
            cache.Set("PreviousPage", PreviousPage);
            cache.Set("CurrentPage", CurrentPage);
            cache.Set("NextPage", NextPage);
            return CurrentPage;
        }
        public SearchResult<T> Search(string query, int page, int pageSize, string indexName = "")
        {
            List<HeaderData> HeaderList = new List<HeaderData>();
            T result = (T)(object)esAccess.PostQuery(query, page, pageSize, ref HeaderList, indexName);
            if (result == null) return null;
            return new SearchResult<T>
            {

                Results = (T)result,
                Header = HeaderList,
                Header2 = HeaderList.Select(x => x.Header).ToList()
        };
        }

        public SearchResult<T> Search(string query, int page, int pageSize)
        {
            return null;
        }

            public SearchResult<T> Search(string query, string page, int pageSize, string pageFilter, bool forward, string PageFilterType)
        {
            string prev = "";
            string next = "";
            List<HeaderData> HeaderList = new List<HeaderData>();
            T result = (T)(object)postgresAccess.LoadSQL(query, pageFilter, page.ToString(), pageSize.ToString(), ref next, ref prev, forward, ref HeaderList, PageFilterType);
            if (result == null) return null;

            var ret = new SearchResult<T>();
            ret.Results = (T)result;
            ret.Header = HeaderList;
            ret.Header2 = HeaderList.Select(x => x.Header).ToList();
            ret.PreviousStart = prev;
            ret.NextStart = next;
           

            return ret;
           
            
        }

        public SearchResult<T> Search2(string query, string page, int pageSize, string pageFilter, bool forward, string PageFilterType)
        {
            string prev = "";
            string next = "";
            List<string> HeaderList = new List<string>();
            T result = (T)(object)postgresAccess.LoadSQL2(query, pageFilter, page.ToString(), pageSize.ToString(), ref next, ref prev, forward, ref HeaderList, PageFilterType);
            if (result == null) return null;

            var ret = new SearchResult<T>();
            ret.Results = (T)result;
            ret.Header2 = HeaderList;
            ret.PreviousStart = prev;
            ret.NextStart = next;


            return ret;


        }
        public SearchResult<Post> SearchByFilter(string query, IEnumerable<string> tags, int page, int pageSize)
        {
            return null;
        }
        public IEnumerable<string> Autocomplete(string query, int count)
        {
            return null;
        }

    }
}
