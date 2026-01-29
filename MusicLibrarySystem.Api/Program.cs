using Microsoft.OpenApi;
using MusicLibrarySystem.Data.Ambient;
using MusicLibrarySystem.Data.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

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

builder.Services.AddScoped<ReportConnectionProvider>();

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