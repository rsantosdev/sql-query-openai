namespace WebApplication1.Services;

public partial class QueryGenerationService
{
    public QueryGenerationService WithDbLogging()
    {
        _middlewares.Add(next => async question =>
        {
            logger.LogInformation("LoggingMiddleware initializing");
        
            //todo: Save the question in DB
        
            var result = await next(question);

            //todo: Save the result in DB
        
            logger.LogInformation("LoggingMiddleware done");

            return result;
        });

        return this;
    }
}