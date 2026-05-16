using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.AirflowJob.Files;

public class AirflowJobFilesResponse
{
    [JsonPropertyName("value")]
    public List<AirflowJobFileMetadata> Value { get; set; } = new();
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
    [JsonPropertyName("continuationUri")]
    public string? ContinuationUri { get; set; }
}

public class AirflowJobFileMetadata
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;
    [JsonPropertyName("sizeInBytes")]
    public long SizeInBytes { get; set; }
}
