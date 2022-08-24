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
using Microsoft.OpenApi.Models;
using Serilog;

CreateHostBuilder(args)
    .Build()
    .Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => {
            webBuilder.UseStartup<Startup>();
        })
        .UseWindowsService()
        .UseSerilog((hostingContext, loggerConfiguration)
            => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));


class Startup
{
    private readonly IWebHostEnvironment _currentEnvironment;
    public readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _currentEnvironment = env;
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<Configuration>(_configuration.GetSection("AppConfiguration"));
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        #region DbConnection

        var dbConnection = _configuration.GetConnectionString("DefaultConnection");
        var dbVersion = _configuration.GetValue<string>("ConnectionMysqlMariaDbVersion");
        services.AddDbContext<ApplicationContext>(options =>
            options.UseMySql(dbConnection,
                new MariaDbServerVersion(dbVersion)));

        #endregion

        services.AddScoped<SensorDetectionsRepository>();
        services.AddSingleton<SerialMonitor>();
        services.AddSingleton<SensorDetectionsService>();
        services.AddHostedService<SerialMonitorWorker>();

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "HovyMonitor.Api", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        if (_currentEnvironment.IsDevelopment())
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