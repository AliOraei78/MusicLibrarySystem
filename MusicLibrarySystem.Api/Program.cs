using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MusicLibrarySystem.Data.Ambient;
using MusicLibrarySystem.Data.Context;
using MusicLibrarySystem.Data.Repositories;
using MusicLibrarySystem.Data.TypeHandlers;
using System;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64;
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Music Library System API",
        Version = "v1",
        Description = "Music Library System built b .NET10",
        Contact = new OpenApiContact
        {
            Name = "Ali Jenabi",
            Email = "a.jenabi78@gmail.com"
        }
    });

    // Optional: Include XML comments
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AlbumRepository>();
builder.Services.AddScoped<ReportRepository>();

builder.Services.AddScoped<DapperAmbientContext>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connString = config.GetConnectionString("DefaultConnection")!;
    return new DapperAmbientContext(connString);
});

// EF Core (for full ORM capabilities)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Raw Dapper (for fast queries)
builder.Services.AddScoped<IDapperContext, DapperContext>();

// Hybrid or separate repository
builder.Services.AddScoped<AlbumHybridRepository>();

builder.Services.AddScoped<ReportConnectionProvider>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/dapper-app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

SqlMapper.AddTypeHandler(new GenreTypeHandler());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Management System API V1");

        // Serve Swagger UI at root URL (/)
        c.RoutePrefix = string.Empty;

        // Set the HTML page title
        c.DocumentTitle = "Movie Management API";

        // Hide the models/schema section by default
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();