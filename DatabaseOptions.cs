namespace Systems_One_SanitizeBarcodes_Service;

public class DatabaseOptions
{
    public string Server { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty; // Used to replace {{DatabaseName}} in SQL templates
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string BuildConnectionString()
        => $"Server={Server};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;";
}
