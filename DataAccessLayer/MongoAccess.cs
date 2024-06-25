using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class MongoAccess<T>
    {
        MongoClient _client = null;

        IMongoDatabase _database = null;

        public MongoAccess(string connectionString)
        {
            Console.WriteLine("connectionString" + connectionString);
            var settings = MongoClientSettings.FromConnectionString
                (connectionString);
            //mongodb://deephouseio:Idap3336@cluster0-shard-00-00.aka48.mongodb.net:27017,cluster0-shard-00-01.aka48.mongodb.net:27017,cluster0-shard-00-02.aka48.mongodb.net:27017/dap?ssl=true&replicaSet=atlas-14dq3v-shard-0&authSource=admin&retryWrites=true&w=majority");
            _client = new MongoClient(settings);

            _database = _client.GetDatabase("dap");//_database.GetCollection<Entity>("table");

        
        }
        public string GetDataTypeFromMongo(BsonType FieldType)
        {
            if (FieldType == BsonType.DateTime)
                return "string";
            else if (FieldType == BsonType.String)
                return "string";
            else if (FieldType == BsonType.Symbol)
                return "string";
            else if (FieldType == BsonType.Array)
                return "string";
            else if (FieldType == BsonType.Document)
                return "string";
            else
                return "Double";
        }

        public List<List<string>> PostQuery(string query, int page, int pageSize, string collectionName, ref List<HeaderData> HeaderList,  bool isFind = true)
        {
            List<List<string>> searchResult = new List<List<string>>(); ;
            if (collectionName == string.Empty) return null;

            //IMongoCollection<object> col = _database.GetCollection<object>(collectionName);
            if (isFind)
            {
                BsonDocument document = BsonSerializer.Deserialize<BsonDocument>(query);
                var col = _database.GetCollection<object>(collectionName).FindSync<BsonDocument>(document);
                var list = col.ToList();
              
                for (int i = 0; i < list.Count; i++)
                {
                    var eleList = list[i].Elements.GetEnumerator();
                    List<string> inner = new List<string>();
                    for (int j = 0; j < list[i].ElementCount; j++)
                    {
                        
                        eleList.MoveNext();
                        var ele = eleList.Current;
                        if (searchResult.Count == 0)
                        {
                            if (!ele.Name.Contains("Props") && !ele.Name.Contains("Values"))
                            HeaderList.Add(new HeaderData { Header = ele.Name, DataType = GetDataTypeFromMongo(ele.Value.BsonType)});

                        }
                        if (!ele.Name.Contains("Props") && !ele.Name.Contains("Values"))
                            inner.Add(ele.Value.ToString());
                    }
                    searchResult.Add(inner);
                }
            }
            return searchResult;
        }
    }
}

