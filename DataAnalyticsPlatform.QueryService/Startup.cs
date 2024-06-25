using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataAnalyticsPlatform.QueryService.Service;
using Microsoft.EntityFrameworkCore;
namespace DataAnalyticsPlatform.QueryService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureAuth(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var postgresConnectionString = Configuration.GetConnectionString("postgresdb");
            var digdagPostgresConnectionString = Configuration.GetConnectionString("digdagdb");
            var elasticConnectionString = Configuration.GetConnectionString("elastic");
            var dataService = Configuration.GetConnectionString("dataservice");//var dataService = Configuration.GetConnectionString("dataservice");
            var mongoConnection = Configuration.GetConnectionString("mongodb");
            //    services.ConfigureAuth(Configuration);
            // services.Configure<ConnectionStringsConfig>(option => option.PostgresConnection = postgresConnectionString);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddQueryServices(postgresConnectionString, digdagPostgresConnectionString, elasticConnectionString, dataService, mongoConnection);
            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           app.UseAuthentication();
            app.UseCors(
                     builder =>
                     {
                         builder.AllowAnyOrigin()
                         .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                     }
                     );
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
