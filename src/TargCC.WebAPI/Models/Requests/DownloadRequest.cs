namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for downloading generated files as ZIP
/// </summary>
public class DownloadRequest
{
    /// <summary>
    /// Gets or sets the list of file paths to include in the ZIP
    /// </summary>
    public List<string> FilePaths { get; set; } = new();
}
