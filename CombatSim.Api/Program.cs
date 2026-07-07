using CombatSim.Core.Features.Simulator.Models;
using CombatSim.Core.Features.Simulator.Services;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your services. This is essentially registering "recipes" with builder.Services. It's sort of 
// like saying "if anyone ever asks for an ISimulatorService, here's how to make one: construct a 
// SimulatorService." Nothing gets built yet at this point — you're just registering recipes.
builder.Services.AddHttpClient(); // This just registers IHttpClientFactory itself into the container
builder.Services.AddSingleton<IDictionary<string, ParseDamageResponse>>(
    provider => new ConcurrentDictionary<string, ParseDamageResponse>() // Use ConcurrentDictionary for thread-safe access
    );
builder.Services.AddSingleton<ISimulatorService>(provider =>
    {
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        var cache = provider.GetRequiredService<IDictionary<string, ParseDamageResponse>>();
        return new SimulatorService(httpClient, cache);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();