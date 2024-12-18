using System.Text.Json;
using OpenAI.Chat;

namespace WebApplication1.Services;

public class UserResponseService(ChatClient chatClient, ILogger<UserResponseService> logger)
{
    public async Task<string> GenerateUserResponseAsync(string question, dynamic data)
    {
        try
        {
            ChatCompletion completion = await chatClient.CompleteChatAsync(string.Format(Prompt, question, JsonSerializer.Serialize(data)));
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to generate response to user");
            return "Unable to generate response";
        }
    }

    private const string Prompt =
        @"
            **System:**
            You need to asnwer to user question using a json of data received as input.
            In case you are not able to asnwer to the question just say it to the user.
            You can ask to have more dettail or make suggestions.

            Print the output as simple text without any markdown.

            User Question:  
            {0}

            Data:  
            {1}
        ";
}