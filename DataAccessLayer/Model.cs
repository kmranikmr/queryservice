using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess
{

    public class Post
    {
        public string Id { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? Score { get; set; }
        public int? AnswerCount { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Tags { get; set; }

        [Completion]
        public IEnumerable<string> Suggest { get; set; }
    }

    public class HeaderData
    {
        public string Header { get; set; }
        public string DataType { get; set; }
    }
    public class SearchResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public T Results { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public string PreviousStart { get; set; }
        public string NextStart { get; set; }
        public List<HeaderData> Header { get; set; }
        public List<string> Header2 { get; set; }
    }
    public class PaginatedData<T>
    {
        public int CurrentPage { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int PageSize { get; set; }

    }
    public class PaginatedList<T>
    {
        public int CurrentPage { get; private set; }
        public int From { get; private set; }
        public List<T> Items { get; private set; }
        public int PageSize { get; private set; }
        public int To { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int currentPage, int pageSize)
        {
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            PageSize = pageSize;
            From = ((currentPage - 1) * pageSize) + 1;
            To = (From + pageSize) - 1;

            Items = items;
        }

        public bool HasPreviousPage
        {
            get
            {
                return (CurrentPage > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }
    }


    public class FieldInfo
        {
            public string Name { get; set; }

            public DataType DataType { get; set; }

            public int Length { get; set; }

            public string Map { get; set; }

            public List<FieldInfo> InnerFields { get; set; }

            public override string ToString()
            {
                return $@"{Name} - {DataType} ({Length})";
            }

            public FieldInfo()
            {

            }

            public FieldInfo(string name, DataType t) : this()
            {
                DataType = t;

                Name = name;

                Map = name;

                if (t == DataType.Object || t == DataType.ObjectArray)
                {
                    InnerFields = new List<FieldInfo>();
                }
            }

            public void AddField(FieldInfo f)
            {
                if (DataType == DataType.Object || DataType == DataType.ObjectArray)
                {
                    if (InnerFields == null) InnerFields = new List<FieldInfo>();

                    f.Map = Map + "." + f.Name;

                    InnerFields.Add(f);
                }
                else
                {
                    throw new Exception("Fields can be added only on object data type.");
                }
            }


            public FieldInfo(FieldInfo d)
            {
                Name = d.Name;
                DataType = d.DataType;
                Length = d.Length;
                Map = d.Map;
                if (d.InnerFields != null && d.InnerFields.Count > 0)
                {
                    InnerFields = new List<FieldInfo>();
                    foreach (var item in d.InnerFields)
                    {
                        InnerFields.Add(new FieldInfo(item));
                    }
                }
            }

            public void Update(FieldInfo d)
            {
                Name = d.Name;
                DataType = d.DataType;
                Length = d.Length;
                Map = d.Map;
                if (d.InnerFields != null && d.InnerFields.Count > 0)
                {
                    InnerFields = new List<FieldInfo>();
                    foreach (var item in d.InnerFields)
                    {
                        InnerFields.Add(new FieldInfo(item));
                    }
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FieldInfo))
                    return false;

                var other = obj as FieldInfo;

                if ((DataType != other.DataType) || (Length != other.Length) || (Map != other.Map) || (Name != other.Name))
                    return false;

                return true;
            }

            public static bool operator ==(FieldInfo x, FieldInfo y)
            {
                return x.Equals(y);
            }
            public static bool operator !=(FieldInfo x, FieldInfo y)
            {
                return !(x == y);
            }

            internal static DataType GetFieldType(string dataType)
            {
                switch (dataType)
                {
                    case "DateTime":
                        return DataType.DateTime;
                    case "int":
                        return DataType.Int;
                    case "double":
                        return DataType.Double;
                    case "string":
                        return DataType.String;

                }

                return DataType.Object;
            }
        }

        public class TypeConfig
        {
            public List<FieldInfo> BaseClassFields { get; set; }

            public List<ModelInfo> ModelInfoList { get; set; }

            public TypeConfig()
            {
                BaseClassFields = new List<FieldInfo>();
                ModelInfoList = new List<ModelInfo>();
            }
            public TypeConfig(TypeConfig type)
            {
                BaseClassFields = type.BaseClassFields;
                ModelInfoList = type.ModelInfoList;
            }
        }

        public class ModelInfo
        {
            public string ModelName { get; set; }

            public List<FieldInfo> ModelFields { get; set; }

            public ModelInfo()
            {
                ModelFields = new List<FieldInfo>();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ModelInfo))
                    return false;

                var other = obj as ModelInfo;

                if ((ModelName != other.ModelName) || (!ModelFields.All(item => other.ModelFields.Contains(item))))
                    return false;

                return true;
            }

            public static bool operator ==(ModelInfo x, ModelInfo y)
            {
                return x.Equals(y);
            }
            public static bool operator !=(ModelInfo x, ModelInfo y)
            {
                return !(x == y);
            }
        }

        public enum DataType
        {
            String,
            Int,
            Double,
            Boolean,
            Long,
            Char,
            DateTime,
            Object,
            ObjectArray,
            StringArray,
            IntArray,
            FloatArray
        }
    }



