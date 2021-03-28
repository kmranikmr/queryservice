using DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEngine
{
    public interface ISearchService<T> 
    {
        SearchResult<T> Search(string query, int page, int pageSize);
        SearchResult<T> Search(string query, int page, int pageSize , string indexName );
        SearchResult<Post> SearchByFilter(string query, IEnumerable<string> tags, int page, int pageSize);
        IEnumerable<string> Autocomplete(string query, int count);
    }
}
