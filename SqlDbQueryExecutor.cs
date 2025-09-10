using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Systems_One_SanitizeBarcodes_Service;

public class SqlDbQueryExecutor : IDbQueryExecutor
{
    private readonly DatabaseOptions _dbOptions;
    private readonly QueryExecutionOptions _queryOptions;
    private readonly ILogger<SqlDbQueryExecutor> _logger;

    public SqlDbQueryExecutor(IOptions<DatabaseOptions> dbOptions, IOptions<QueryExecutionOptions> queryOptions, ILogger<SqlDbQueryExecutor> logger)
    {
        _dbOptions = dbOptions.Value;
        _queryOptions = queryOptions.Value;
        _logger = logger;
    }

    public async Task<int> ExecuteQueryFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Query file not found: {file}", filePath);
            return 0;
        }

        string sql = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(sql))
        {
            _logger.LogWarning("Query file is empty: {file}", filePath);
            return 0;
        }

        sql = ApplyPlaceholders(sql);

        // Split on lines that contain only GO (case-insensitive) - GO is a batch separator used by tools, not T-SQL.
        string[] batches = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToArray();

        int totalRows = 0;

        try
        {
            await using var connection = new SqlConnection(_dbOptions.BuildConnectionString());
            await connection.OpenAsync(cancellationToken);

            for (int i = 0; i < batches.Length; i++)
            {
                string batch = batches[i];
                try
                {
                    await using var cmd = new SqlCommand(batch, connection) { CommandType = System.Data.CommandType.Text };
                    int rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    totalRows += rows;
                    _logger.LogInformation("Executed batch {index}/{count} from {file}. Rows affected: {rows}", i + 1, batches.Length, filePath, rows);
                }
                catch (Exception exBatch)
                {
                    _logger.LogError(exBatch, "Error executing batch {index}/{count} from {file}", i + 1, batches.Length, filePath);
                    // Continue with next batch
                }
            }

            _logger.LogInformation("Finished executing script {file}. Total rows affected (sum): {rows}", filePath, totalRows);
            return totalRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query script {file}", filePath);
            return totalRows;
        }
    }

    private string ApplyPlaceholders(string sql)
    {
        return sql
            .Replace("{{DatabaseName}}", _dbOptions.DatabaseName)
            .Replace("{{TableName}}", _queryOptions.TableName)
            .Replace("{{ReplacementChar}}", EscapeSqlLiteral(_queryOptions.ReplacementChar));
    }

    private static string EscapeSqlLiteral(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("'", "''");
    }
}
