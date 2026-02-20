using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Service = "SupplyConsensus API",
        Version = "1.0.0"
    });
})
.WithName("HealthCheck")
.WithOpenApi();

// API root
app.MapGet("/api", () =>
{
    return Results.Ok(new
    {
        Name = "SupplyConsensus API",
        Version = "1.0.0",
        Description = "Supply chain consensus platform API"
    });
})
.WithName("GetApiInfo")
.WithOpenApi();

app.Run();
