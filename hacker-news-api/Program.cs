using hacker_news_wrapper;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "HackerNewsAPI",
        Description = "Hacker News API",
        Version = "v0"
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddServiceProvider();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();


[ExcludeFromCodeCoverage]
public partial class Program
{
}