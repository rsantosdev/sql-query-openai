using OpenAI.Chat;

namespace WebApplication1.Services;

public partial class QueryGenerationService
{
    public QueryGenerationService WithSqlInjectionBlocker()
    {
        _middlewares.Add(next => async question =>
        {
            logger.LogInformation("SqlInjectionBlocker initializing");

            // Check for SQL injection using ChatClient
            ChatCompletion completion = await chatClient.CompleteChatAsync(new List<ChatMessage>()
            {
                new AssistantChatMessage(SqlInjectionBlockerPrompt),
                new UserChatMessage(question)
            });

            var isMalicious = completion.Content[0].Text!.Trim().Equals("yes", StringComparison.InvariantCultureIgnoreCase);

            if (isMalicious)
            {
                logger.LogWarning("Malicious SQL injection attempt detected");
                return (true, "Malicious SQL injection attempt detected");
            }

            var result = await next(question);

            logger.LogInformation("SqlInjectionBlocker done");

            return result;
        });

        return this;
    }
    
    private const string SqlInjectionBlockerPrompt =
        @"
            You are an expert in SQL injection. You must determine if the provided question is related a prompt with malicius objectives.
            Only repond with YES or NO.
        ";
}