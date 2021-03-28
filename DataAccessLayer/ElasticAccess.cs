using System;
using System.Collections.Generic;
using System.Text;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataAccess
{
    public class ElasticAccess<T>
    {
        protected readonly IElasticClient Client;
        public ElasticAccess(string connectionString)
        {
            Client = CreateClient(connectionString);
        }
        public IElasticClient CreateClient(string connectionString)
        {
            var node = new Uri(connectionString);
            var connectionPool = new SingleNodeConnectionPool(node);
            var connectionSettings = new ConnectionSettings(connectionPool,
           sourceSerializer: (builtin, setting) => new JsonNetSerializer(
            builtin, setting, () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }
          )).DisableDirectStreaming();
            return new ElasticClient(connectionSettings);
        }
        public string GetDataTypeFromES(JTokenType FieldType)
        {
            if (FieldType == JTokenType.Date)
                return "string";
            else if (FieldType == JTokenType.String)
                return "string";
            else if (FieldType == JTokenType.Object)
                return "string";
            else if (FieldType == JTokenType.Array)
                return "string";
            else if (FieldType == JTokenType.Raw)
                return "string";
            else
                return "Double";
        }
        public List<List<string>> PostQuery(string queryStr, int page , int size, ref List<HeaderData> HeaderList, string indexName = "")//List<List<string>>
        {
            List<List<string>> searchResult = new List<List<string>>(); ;
            //var indexName = "";// "schema1_4248_2.model1";
            ISearchResponse<dynamic> test = null;
            if (indexName == "")
            {
                test = Client.Search<dynamic>(s => s
                //.Index(indexName).
                .AllIndices()
                 .From(page)
                 .Size(size)
                 .Query(q => q
                 .Raw(queryStr)
                   )
                );
            }
            else
            {
                test = Client.Search<dynamic>(s => s
               .Index(indexName)
              //.AllIndices()
                .From(page)
                .Size(size)
                .Query(q => q
                .Raw(queryStr)
                  )
               );
            }

            foreach ( var hit in test.Hits)
            {
                List<string> inner = new List<string>();
                foreach (var keyValuePair in hit.Source)
                {

                    inner.Add(((JProperty)keyValuePair).Name + ":" + ((JProperty)keyValuePair).Value);
                    if (searchResult.Count == 0)
                    {

                        HeaderList.Add(new HeaderData { Header = ((JProperty)keyValuePair).Name, DataType = GetDataTypeFromES(((JProperty)keyValuePair).Value.Type) });

                    }
                }
                
                searchResult.Add(inner);
                //    var source = (IDictionary<string, Newtonsoft.Json.Linq.JToken>)hit.Source;
                int g = 0;
            }
            //var stringResponse = Client.LowLevel.DoRequest<StringResponse>(
            //    HttpMethod.POST,
            //    "_search",
            //    PostData.Serializable(new
            //    {
            //        query =  queryStr  
            //    }));            //string query = @"{""bool"": {""should"": [{""match"":{""cooling"":""air * ""}},{""bool"": {""must"": [{""match"":{""mediaCategory"":""photo""}},{""match"":{""className"":""resi * ""}}],""filter"": [{""terms"": {""_index"": [""model2""]}}]}}],""filter"": [{""terms"": {""_index"": [""model2""]}}]}}";
            //var stringResponse = Client.LowLevel.Search<StringResponse>(query);

            //       var test = Client.Search<dynamic>(s => s
            //       .Index(""_all"")
            //    .From(0)
            //    .Size(100)
            //    .Query(q => q
            //        .Raw(queryStr)
            //    )
            //);
            //var stringResponse = Client.LowLevel.DoRequest<StringResponse>(
            //HttpMethod.POST,
            //""_search",
            //PostData.Serializable(new
            //{
            //    // query = queryStr,
            //    query = new { match_all = new { } }


            //}));
            //   From = page,
            ///Size = size,
            return (List<List<string>>)(searchResult);
        }

    }
}
