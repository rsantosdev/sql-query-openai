using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Services;

public class DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
{
    public async Task<string> GetSchemaAsync()
    {
        try
        {
            await using var sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var result = await sqlConnection.QueryAsync(SchemaQuery);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get schema");
            return "Unable to get schema";
        }
    }

    public async Task<IEnumerable<dynamic>> ExecuteQuery(string query)
    {
        try
        {
            await using var sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var result = await sqlConnection.QueryAsync(query);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred at {nameof(ExecuteQuery)}");
            return null;
        }
    }

    private const string SchemaQuery = @"
        SELECT
            s.name as schema_name,      
            t.name as table_name,
            (
                SELECT
                    c.name as column_name,
                    TYPE_NAME(c.system_type_id) as data_type,   
                    cd.value as MS_Description
                FROM sys.columns AS c
                LEFT OUTER JOIN sys.extended_properties as cd 
                    ON cd.major_id = c.object_id
                    AND cd.minor_id = c.column_id
                    AND cd.name = 'MS_Description'
                WHERE c.object_id = t.object_id
                FOR JSON PATH
            ) as columns
        FROM sys.tables AS t
        INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
    ";
}