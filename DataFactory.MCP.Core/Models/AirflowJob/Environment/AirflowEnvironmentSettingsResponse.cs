using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.AirflowJob.Environment;

public class AirflowEnvironmentSettingsResponse
{
    [JsonPropertyName("environmentVariables")]
    public List<AirflowNameValuePair> EnvironmentVariables { get; set; } = new();
    [JsonPropertyName("airflowConfigurationOverrides")]
    public List<AirflowNameValuePair> AirflowConfigurationOverrides { get; set; } = new();
    [JsonPropertyName("triggerers")]
    public string? Triggerers { get; set; }  // Enabled or Disabled
}

public class AirflowNameValuePair
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
