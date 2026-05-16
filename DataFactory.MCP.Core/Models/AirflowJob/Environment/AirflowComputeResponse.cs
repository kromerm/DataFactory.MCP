using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.AirflowJob.Environment;

public class AirflowComputeResponse
{
    [JsonPropertyName("poolTemplateId")]
    public string? PoolTemplateId { get; set; }
    [JsonPropertyName("poolTemplateName")]
    public string? PoolTemplateName { get; set; }
    [JsonPropertyName("nodeSize")]
    public string? NodeSize { get; set; }  // Small or Large
    [JsonPropertyName("computeScalability")]
    public AirflowComputeScalability? ComputeScalability { get; set; }
    [JsonPropertyName("apacheAirflowJobVersion")]
    public string? ApacheAirflowJobVersion { get; set; }
    [JsonPropertyName("apacheAirflowJobVersionDetails")]
    public AirflowVersionDetails? ApacheAirflowJobVersionDetails { get; set; }
    [JsonPropertyName("shutdownPolicy")]
    public string? ShutdownPolicy { get; set; }  // AlwaysOn or OneHourInactivity
}

public class AirflowComputeScalability
{
    [JsonPropertyName("minNodeCount")]
    public int MinNodeCount { get; set; }
    [JsonPropertyName("maxNodeCount")]
    public int MaxNodeCount { get; set; }
}

public class AirflowVersionDetails
{
    [JsonPropertyName("apacheAirflowVersion")]
    public string? ApacheAirflowVersion { get; set; }
    [JsonPropertyName("pythonVersion")]
    public string? PythonVersion { get; set; }
}
