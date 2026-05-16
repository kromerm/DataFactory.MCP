using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.AirflowJob.Environment;

public class AirflowEnvironmentStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    // Values: Initial, Starting, Started, Stopping, Stopped, Upgrading, Failed
}
