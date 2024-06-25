using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QueryEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.QueryService.Service
{
    public static class QueryServices
    {
        public static void AddQueryServices(this IServiceCollection service , string p, string digdagConnection, string elastic, string dataservice, string mongoConnection)// IOptions<ConnectionStringsConfig> optionsAccessor)
        {
            IConfiguration confiuration;
           // var postgresConnectionString = confiuration.GetConnectionString("postgresdb");
            //service.Configure<ConnectionStringsConfig>(option => option.PostgresConnection = p);  
           //"HOST=raja.db.elephantsql.com;PORT=5432;Username=aniwbjgk;Password=esypNF7dCv9kKReCSNvM48LsPoJX_IvG;Database=aniwbjgk;Search Path=public
            service.AddSingleton<ESNativeSearch<List<List<string>>>>(s => new ESNativeSearch<List<List<string>>>(p, digdagConnection, elastic, dataservice,mongoConnection));// "HOST=raja.db.elephantsql.com;PORT=5432;Username=aniwbjgk;Password=esypNF7dCv9kKReCSNvM48LsPoJX_IvG;Database=aniwbjgk;Search Path=public"));// "HOST=localhost;PORT=5433;Username=dev;Password=nwdidb19;Database=nwdi_ts;Search Path=nwdi"));//"HOST =raja.db.elephantsql.com;PORT=5432;Username=aniwbjgk;Password=esypNF7dCv9kKReCSNvM48LsPoJX_IvG;Database=aniwbjgk;Search Path=public"));//"HOST=localhost;PORT=5433;Username=dev;Password=nwdidb19;Database=nwdi_ts;Search Path=nwdi"
            service.AddSingleton<CustomGrammerQueryBuilder>();
           // service.AddSingleton<SQLNativeSearch>();
        }
    }
}
