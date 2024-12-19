using OpenAI.Chat;

namespace WebApplication1.Services;

public partial class QueryGenerationService(ChatClient chatClient, ILogger<QueryGenerationService> logger)
{
    private string _question = string.Empty;
    private string _databaseSchema = string.Empty;
    private readonly List<Func<Func<string, Task<(bool HasError, string Message)>>, Func<string, Task<(bool HasError, string Message)>>>> _middlewares = new();

    public QueryGenerationService CreateProcess(string question, string databaseSchema)
    {
        _question = question;
        _databaseSchema = databaseSchema;
        return this;
    }

    public async Task<(bool HasError, string Message)> ExecuteAsync()
    {
        Func<string, Task<(bool HasError, string Message)>> next = async (question) => await GenerateQueryAsync(question, _databaseSchema);

        foreach (var middleware in _middlewares.AsEnumerable().Reverse())
        {
            next = middleware(next);
        }

        var result = await next(_question);

        return result;
    }

    private async Task<(bool HasError, string Message)> GenerateQueryAsync(string question, string databaseSchema)
    {
        try
        {
            ChatCompletion completion = await chatClient.CompleteChatAsync(string.Format(Prompt, question, databaseSchema));
            return (false, completion.Content[0].Text);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to generate query");
            return (true, "Unable to generate query");
        }
    }

    private const string Prompt =
        @"
            You are an assistant database developer and you must respond ONLY using T-SQL language based on the Azure SQL database.
            You must rely on the database schema defined in input schema section.

            Your response must always start with a SELECT or a WITH, you must never add other words before or after.
            You must always specify the names of the columns and tables in square brackets [].
            Your response must always consist of 1 query only.
            Answer in plain text without markdown or HTML.

            If asked to delete data, do not answer.
            Do not add any comment or reply that is not in T-SQL.

            Input Question:
            {0}

            Input tables schema:
            {1}
        ";
}