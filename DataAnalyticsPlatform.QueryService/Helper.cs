//using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using Newtonsoft.Json.Schema;
//using Newtonsoft.Json.Schema.Generation;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyticsPlatform.QueryService
{
    public static class ExtensionMethods
    {
        public static string Replace(this string s, char[] separators, string newVal)
        {
            string[] temp;

            temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(newVal, temp);
        }
    }
    public static class Helper
    {
        public static string GetJson(object record)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);

            return json;
        }


        public static string GetJsonSchema(Type type)
        {
            return "";
            //JSchemaGenerator jsonSchemaGenerator = new JSchemaGenerator();
            //JSchema schema = jsonSchemaGenerator.Generate(type);
            //schema.Title = type.Name;

            //return schema.ToString();
        }

        public static T DeserializeFromXmlString<T>(string xml) where T : new()
        {
            T xmlObject = new T();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml);
            xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
