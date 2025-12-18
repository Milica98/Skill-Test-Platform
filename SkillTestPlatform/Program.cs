using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkillTestPlatform.Constants;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services;
using SkillTestPlatform.Services.Interfaces;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var azureStorageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
           ?? builder.Configuration["AzureStorage:ConnectionString"];


builder.Services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));

builder.Services.AddSingleton<IStorageService<Candidate>>(sp =>
    new BlobStorageService<Candidate>(sp.GetRequiredService<BlobServiceClient>(), BlobSettings.Candidates, sp.GetRequiredService<ILogger<BlobStorageService<Candidate>>>()));
builder.Services.AddSingleton<IStorageService<Skill>>(sp =>
    new BlobStorageService<Skill>(sp.GetRequiredService<BlobServiceClient>(), BlobSettings.Skills, sp.GetRequiredService<ILogger<BlobStorageService<Skill>>>()));
builder.Services.AddSingleton<IStorageService<TaskItem>>(sp =>
    new BlobStorageService<TaskItem>(sp.GetRequiredService<BlobServiceClient>(), BlobSettings.Tasks, sp.GetRequiredService<ILogger<BlobStorageService<TaskItem>>>()));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred";

        if (exception is Azure.RequestFailedException reqEx)
        {
            switch (reqEx.ErrorCode)
            {
                case "BlobNotFound":
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Blob not found";
                    break;

                case "AuthorizationFailure":
                case "AccountNotFound":
                    statusCode = StatusCodes.Status403Forbidden;
                    title = "Access denied to blob storage";
                    break;

                case "ContainerNotFound":
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Container not found";
                    break;

                case "ContainerBeingDeleted":
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Container is being deleted.";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = $"Blob storage error: {reqEx.ErrorCode}";
                    break;
            }
        }
        else if (exception is JsonException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Invalid blob content";
        }

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = exception?.Message,
            Status = statusCode,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
