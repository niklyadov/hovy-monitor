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

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var configuration = builder.Configuration;

var appConfiguration = configuration.GetSection("AppConfiguration");
builder.Services.Configure<Configuration>(appConfiguration);

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.Configure<Configuration>(configuration.GetSection("AppConfiguration"));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HovyMonitor.Api", Version = "v1" });
});

#region DbConnection

var dbConnection = configuration.GetConnectionString("DefaultConnection");
var dbVersion = configuration.GetValue<string>("ConnectionMysqlMariaDbVersion");
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(dbConnection,
        new MariaDbServerVersion(dbVersion)));

#endregion

builder.Services.AddScoped<SensorDetectionsRepository>();

builder.Services.AddSingleton<SensorDetectionsService>();

builder.Services.AddHostedService<SerialMonitorWorker>();

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Host.UseWindowsService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.Run();