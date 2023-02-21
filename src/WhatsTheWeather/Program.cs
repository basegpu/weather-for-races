using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.SecretSauce;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.WriteIndented = true;
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddRazorPages();
builder.Services.AddEndpointDefinitions(typeof(Race),typeof(OpenApiInfo));

Log.Information(builder.Configuration.GetDebugView());

var app = builder.Build();

app.UseEndpointDefinitions();
app.MapRazorPages();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
