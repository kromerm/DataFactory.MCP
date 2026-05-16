using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.AirflowJob.Environment;

public class AirflowLibrariesResponse
{
    [JsonPropertyName("value")]
    public List<AirflowLibrary> Value { get; set; } = new();
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
    [JsonPropertyName("continuationUri")]
    public string? ContinuationUri { get; set; }
}

public class AirflowLibrary
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("libraryType")]
    public string? LibraryType { get; set; }  // Public or Private
    [JsonPropertyName("source")]
    public string? Source { get; set; }  // PyPI or Internal
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
