using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSingleton<PersistenceContext>();

builder.Services.AddSingleton<IFleetRepository>(provider =>
    provider.GetRequiredService<PersistenceContext>().Fleets);

builder.Services.AddSingleton<ICommandRepository>(provider =>
    provider.GetRequiredService<PersistenceContext>().Commands);

builder.Services.AddSingleton<IResourcePoolRepository>(provider =>
    provider.GetRequiredService<PersistenceContext>().ResourcePools);

builder.Services.AddSingleton<IBackgroundCommandQueue, InMemoryBackgroundCommandQueue>();
builder.Services.AddScoped<ICommandProcessor, CommandProcessor>();

builder.Services.AddScoped<FleetService>();
builder.Services.AddScoped<CommandService>();

builder.Services.AddHostedService<CommandBackgroundWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public partial class Program { }