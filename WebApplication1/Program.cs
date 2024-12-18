using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<AzureOpenAIClient>(new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAi:Endpoint"]),
    new AzureKeyCredential(builder.Configuration["AzureOpenAi:Key"])));

builder.Services.AddSingleton<ChatClient>(sp =>
{
    var azureAiClient = sp.GetRequiredService<AzureOpenAIClient>();
    return azureAiClient.GetChatClient(builder.Configuration["AzureOpenAi:Model"]);
});

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<QueryGenerationService>();
builder.Services.AddSingleton<UserResponseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/query", async (
    [FromBody] QueryRequest question, 
    [FromServices] DatabaseService databaseService,
    [FromServices] QueryGenerationService queryGenerationService,
    [FromServices] UserResponseService userResponseService) =>
    {
        var schema = await databaseService.GetSchemaAsync();
        var query = await queryGenerationService.GenerateQueryAsync(question.Question, schema);

        var data = await databaseService.ExecuteQuery(query);
        var result = await userResponseService.GenerateUserResponseAsync(question.Question, data);
        return result;
    });

app.Run();

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
}