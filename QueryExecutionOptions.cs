namespace Systems_One_SanitizeBarcodes_Service;

public class QueryExecutionOptions
{
    public int IntervalMinutes { get; set; } = 5;
    public string QueryFile { get; set; } = string.Empty; // Relative path
    public string TableName { get; set; } = "YourTable"; // Configurable table name used in template
    public string ReplacementChar { get; set; } = string.Empty; // Character used to replace invalid chars
}
