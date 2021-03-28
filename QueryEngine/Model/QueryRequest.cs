using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueryService.Model
{
    public enum DestinationType
    {
        None,
        RDBMS,
        Mongo,
        ElasticSearch,
        csv
        
    }
    public class QueryRequest
    {
        public string QueryString { get; set; }
        public DestinationType SearchDestination { get; set; }
        public List<string> ModelNames { get; set; }
        public int  PageIndex { get; set; }
        public int PageSize { get; set; }
        public string PageFilter { get; set; }

        public string PageFilterType { get; set; }
        public string PageOffset { get; set; }
        public bool AllRecords { get; set; }
        public List<string> Columns { set; get; }

        public int ProjectId { get; set; }
        public int SchemaVersionId { get; set; }
        public int ModelAttemptId { get; set; }
        public bool isWorkflow { get; set; }
        public string SchemaName { get; set; }
        public string ModelName { get; set; }
    }


    public class TestPreviewQuery
    {
        public TestPreviewQuery()
        {
            Limit = 10;
        }
        public string query { get; set; }
        public string TaskName { get; set; }
        public string TaskId { get; set; }
        public int workfowProjectId { get; set; }
        public int workflowVersionId { get; set; }
        public int WorkflowTestId { get; set; }
        public int Limit { get; set; }
    }


    public class ViewData
    {
        public string Query { get; set; }
        public string ViewName { get; set; }
    }

    public class QueryData
    {
        public string query { get; set; }
        public int projectId { get; set; }
    }
  
}

