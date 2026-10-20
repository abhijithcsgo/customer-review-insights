using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CustomerReviewInsights.Api.Configuration;
using CustomerReviewInsights.Api.Data;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Exceptions;
using CustomerReviewInsights.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            var response = new ApiErrorResponse
            {
                Error = "Validation failed.",
                Detail = "One or more fields are invalid.",
                ValidationErrors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Customer Review Insights API",
        Version = "v1",
        Description = "AI-powered customer review analysis API"
    });
});

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpClient<IReviewAnalysisService, GeminiReviewAnalysisService>((sp, client) =>
{
    var gemini = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(gemini.TimeoutSeconds, 5, 180));
});

builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Catch unhandled exceptions from the rest of the pipeline (must be early).
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ClientApp");
app.MapControllers();

app.Run();

public partial class Program;
