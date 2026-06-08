using System.Reflection;
using BookStorage.Api.Modules;
using BookStorage.Core;
using BookStorage.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "BookStorage API";
        document.Info.Version = "v1";
        document.Info.Description = "API for managing book storage";
        return Task.CompletedTask;
    });
});
builder.Services.AddCore();
builder.Services.AddInfrastructure();

var config = TypeAdapterConfig.GlobalSettings;

config.Scan(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.Services.UseInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.MapBooks();
app.MapPersons();
app.MapCategories();
app.MapBookFiles();

app.Run();
