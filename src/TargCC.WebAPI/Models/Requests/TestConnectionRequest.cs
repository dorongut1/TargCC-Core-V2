namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request to test a database connection.
/// </summary>
public class TestConnectionRequest
{
    /// <summary>
    /// Gets or sets the connection string to test.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
