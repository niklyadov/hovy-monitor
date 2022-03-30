using HovyMonitor.Api.Data;
using HovyMonitor.Api.Data.Repository;
using HovyMonitor.Api.Entity;
using HovyMonitor.Api.Services;
using HovyMonitor.Api.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace HovyMonitor.Api
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
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.Configure<Configuration>(Configuration.GetSection("AppConfiguration"));
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddFile(Configuration.GetSection("Logging")));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HovyMonitor.Api", Version = "v1" });
            });

            #region DbConnection

            var dbConnection = Configuration.GetConnectionString("DefaultConnection");
            var dbVersion = Configuration.GetValue<string>("ConnectionMysqlMariaDbVersion");
            services.AddDbContext<ApplicationContext>(options =>
                options.UseMySql(dbConnection,
                    new MariaDbServerVersion(dbVersion)));

            #endregion

            services.AddScoped<SensorDetectionsRepository>();

            services.AddSingleton<SensorDetectionsService>();

            services.AddHostedService<SerialMonitorWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HovyMonitor.Api v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}