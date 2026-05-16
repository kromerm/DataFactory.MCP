using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Extensions;
using DataFactory.MCP.Models.AirflowJob;
using DataFactory.MCP.Models.AirflowJob.Definition;
using DataFactory.MCP.Models.AirflowJob.Environment;
using DataFactory.MCP.Models.AirflowJob.Files;

namespace DataFactory.MCP.Tools.AirflowJob;

/// <summary>
/// MCP Tool for managing Microsoft Fabric Apache Airflow Jobs.
/// Handles CRUD operations and definition management.
/// </summary>
[McpServerToolType]
public class AirflowJobTool
{
    private readonly IFabricAirflowJobService _airflowJobService;
    private readonly IValidationService _validationService;

    public AirflowJobTool(
        IFabricAirflowJobService airflowJobService,
        IValidationService validationService)
    {
        _airflowJobService = airflowJobService;
        _validationService = validationService;
    }

    [McpServerTool, Description(@"Returns a list of Apache Airflow Jobs from the specified workspace. This API supports pagination.")]
    public async Task<string> ListAirflowJobsAsync(
        [Description("The workspace ID to list Apache Airflow Jobs from (required)")] string workspaceId,
        [Description("A token for retrieving the next page of results (optional)")] string? continuationToken = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));

            var response = await _airflowJobService.ListAirflowJobsAsync(workspaceId, continuationToken);

            if (!response.Value.Any())
            {
                return $"No Apache Airflow Jobs found in workspace '{workspaceId}'.";
            }

            var result = new
            {
                WorkspaceId = workspaceId,
                AirflowJobCount = response.Value.Count,
                ContinuationToken = response.ContinuationToken,
                ContinuationUri = response.ContinuationUri,
                HasMoreResults = !string.IsNullOrEmpty(response.ContinuationToken),
                AirflowJobs = response.Value.Select(j => j.ToFormattedInfo())
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("listing Apache Airflow Jobs").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Creates an Apache Airflow Job in the specified workspace.")]
    public async Task<string> CreateAirflowJobAsync(
        [Description("The workspace ID where the Apache Airflow Job will be created (required)")] string workspaceId,
        [Description("The Apache Airflow Job display name (required)")] string displayName,
        [Description("The Apache Airflow Job description (optional, max 256 characters)")] string? description = null,
        [Description("The folder ID where the job will be created (optional, defaults to workspace root)")] string? folderId = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(displayName, nameof(displayName));

            var request = new CreateAirflowJobRequest
            {
                DisplayName = displayName,
                Description = description,
                FolderId = folderId
            };

            var created = await _airflowJobService.CreateAirflowJobAsync(workspaceId, request);

            var result = new
            {
                Success = true,
                Message = $"Apache Airflow Job '{displayName}' created successfully",
                AirflowJobId = created.Id,
                DisplayName = created.DisplayName,
                Description = created.Description,
                Type = created.Type,
                WorkspaceId = created.WorkspaceId,
                FolderId = created.FolderId,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("creating Apache Airflow Job").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the metadata of an Apache Airflow Job by ID.")]
    public async Task<string> GetAirflowJobAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID to retrieve (required)")] string airflowJobId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var airflowJob = await _airflowJobService.GetAirflowJobAsync(workspaceId, airflowJobId);

            return airflowJob.ToFormattedInfo().ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Updates the metadata (displayName and/or description) of an Apache Airflow Job.")]
    public async Task<string> UpdateAirflowJobAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID to update (required)")] string airflowJobId,
        [Description("The new display name (optional)")] string? displayName = null,
        [Description("The new description (optional)")] string? description = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            if (string.IsNullOrEmpty(displayName) && description == null)
            {
                throw new ArgumentException("At least one of displayName or description must be provided");
            }

            var request = new UpdateAirflowJobRequest
            {
                DisplayName = displayName,
                Description = description
            };

            var updated = await _airflowJobService.UpdateAirflowJobAsync(workspaceId, airflowJobId, request);

            var result = new
            {
                Success = true,
                Message = $"Apache Airflow Job '{updated.DisplayName}' updated successfully",
                AirflowJob = updated.ToFormattedInfo()
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("updating Apache Airflow Job").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Deletes an Apache Airflow Job. Use hardDelete=true to permanently delete the item, bypassing the recycle bin.")]
    public async Task<string> DeleteAirflowJobAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID to delete (required)")] string airflowJobId,
        [Description("When true, permanently deletes the item without going through the recycle bin (optional, default false)")] bool hardDelete = false)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            await _airflowJobService.DeleteAirflowJobAsync(workspaceId, airflowJobId, hardDelete);

            var result = new
            {
                Success = true,
                Message = $"Apache Airflow Job '{airflowJobId}' deleted successfully",
                AirflowJobId = airflowJobId,
                WorkspaceId = workspaceId,
                HardDelete = hardDelete
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("deleting Apache Airflow Job").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the definition of an Apache Airflow Job. The definition contains the job configuration with base64-encoded parts.")]
    public async Task<string> GetAirflowJobDefinitionAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID to get the definition for (required)")] string airflowJobId,
        [Description("The definition format to return (optional)")] string? format = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var definition = await _airflowJobService.GetAirflowJobDefinitionAsync(workspaceId, airflowJobId, format);

            var result = new
            {
                Success = true,
                AirflowJobId = airflowJobId,
                WorkspaceId = workspaceId,
                Format = definition.Format,
                PartsCount = definition.Parts.Count,
                Parts = definition.Parts.Select(p => new
                {
                    Path = p.Path,
                    PayloadType = p.PayloadType,
                    DecodedPayload = TryDecodeBase64(p.Payload)
                })
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job definition").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Updates the definition of an Apache Airflow Job with the provided JSON content. The JSON will be base64-encoded and sent to the API.")]
    public async Task<string> UpdateAirflowJobDefinitionAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID to update (required)")] string airflowJobId,
        [Description("The Airflow Job definition JSON content (required)")] string definitionJson,
        [Description("When true, updates the item's metadata (e.g. sensitivity label) as part of the definition update (optional, default false)")] bool updateMetadata = false)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));
            _validationService.ValidateRequiredString(definitionJson, nameof(definitionJson));

            // Validate JSON format
            try
            {
                JsonDocument.Parse(definitionJson);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}");
            }

            // Encode the JSON as base64
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(definitionJson));

            var definition = new AirflowJobDefinition
            {
                Parts = new List<AirflowJobDefinitionPart>
                {
                    new AirflowJobDefinitionPart
                    {
                        Path = "airflowjob-content.json",
                        Payload = base64Payload,
                        PayloadType = "InlineBase64"
                    }
                }
            };

            await _airflowJobService.UpdateAirflowJobDefinitionAsync(workspaceId, airflowJobId, definition, updateMetadata);

            var result = new
            {
                Success = true,
                AirflowJobId = airflowJobId,
                WorkspaceId = workspaceId,
                UpdateMetadata = updateMetadata,
                Message = "Apache Airflow Job definition updated successfully"
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("updating Apache Airflow Job definition").ToMcpJson();
        }
    }

    private static string TryDecodeBase64(string? payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return string.Empty;
        }

        try
        {
            var bytes = Convert.FromBase64String(payload);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return payload;
        }
    }

    [McpServerTool, Description(@"Gets the environment status (Starting/Started/Stopped/etc) of an Apache Airflow Job cluster")]
    public async Task<string> GetAirflowJobEnvironmentStatusAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var result = await _airflowJobService.GetAirflowJobEnvironmentStatusAsync(workspaceId, airflowJobId);

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job environment status").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the compute configuration (node size, pool template, Airflow version) of an Apache Airflow Job")]
    public async Task<string> GetAirflowJobComputeAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var result = await _airflowJobService.GetAirflowJobComputeAsync(workspaceId, airflowJobId);

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job compute configuration").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the environment settings (environment variables, config overrides, triggerers) of an Apache Airflow Job")]
    public async Task<string> GetAirflowJobSettingsAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var result = await _airflowJobService.GetAirflowJobSettingsAsync(workspaceId, airflowJobId);

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job environment settings").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Lists installed Python libraries in the Apache Airflow Job environment")]
    public async Task<string> ListAirflowJobLibrariesAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId,
        [Description("A token for retrieving the next page of results (optional)")] string? continuationToken = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var response = await _airflowJobService.ListAirflowJobLibrariesAsync(workspaceId, airflowJobId, continuationToken);

            var result = new
            {
                WorkspaceId = workspaceId,
                AirflowJobId = airflowJobId,
                LibraryCount = response.Value.Count,
                ContinuationToken = response.ContinuationToken,
                HasMoreResults = !string.IsNullOrEmpty(response.ContinuationToken),
                Libraries = response.Value
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("listing Apache Airflow Job libraries").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Lists DAG and plugin files in an Apache Airflow Job. Use rootPath to filter by 'dags/' or 'plugins/'")]
    public async Task<string> ListAirflowJobFilesAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId,
        [Description("Filter by root path, e.g. 'dags/' or 'plugins/' (optional)")] string? rootPath = null,
        [Description("A token for retrieving the next page of results (optional)")] string? continuationToken = null)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));

            var response = await _airflowJobService.ListAirflowJobFilesAsync(workspaceId, airflowJobId, rootPath, continuationToken);

            var result = new
            {
                WorkspaceId = workspaceId,
                AirflowJobId = airflowJobId,
                RootPath = rootPath,
                FileCount = response.Value.Count,
                ContinuationToken = response.ContinuationToken,
                HasMoreResults = !string.IsNullOrEmpty(response.ContinuationToken),
                Files = response.Value
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("listing Apache Airflow Job files").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Gets the content of a DAG or plugin file. filePath should start with 'dags/' or 'plugins/'")]
    public async Task<string> GetAirflowJobFileAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId,
        [Description("The file path, e.g. 'dags/example_dag.py' (required)")] string filePath)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));
            _validationService.ValidateRequiredString(filePath, nameof(filePath));

            var content = await _airflowJobService.GetAirflowJobFileAsync(workspaceId, airflowJobId, filePath);

            var result = new
            {
                WorkspaceId = workspaceId,
                AirflowJobId = airflowJobId,
                FilePath = filePath,
                Content = content
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("getting Apache Airflow Job file").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Creates or updates a DAG or plugin file in an Apache Airflow Job. filePath must start with 'dags/' or 'plugins/'. Supports Python, YAML, JSON, SQL and other text files.")]
    public async Task<string> UploadAirflowJobFileAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId,
        [Description("The file path, e.g. 'dags/my_dag.py' (required)")] string filePath,
        [Description("The file content to upload (required)")] string fileContent)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));
            _validationService.ValidateRequiredString(filePath, nameof(filePath));
            _validationService.ValidateRequiredString(fileContent, nameof(fileContent));

            await _airflowJobService.UploadAirflowJobFileAsync(workspaceId, airflowJobId, filePath, fileContent);

            var result = new
            {
                Success = true,
                Message = $"File '{filePath}' uploaded successfully",
                WorkspaceId = workspaceId,
                AirflowJobId = airflowJobId,
                FilePath = filePath
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("uploading Apache Airflow Job file").ToMcpJson();
        }
    }

    [McpServerTool, Description(@"Deletes a DAG or plugin file from an Apache Airflow Job. filePath must start with 'dags/' or 'plugins/'")]
    public async Task<string> DeleteAirflowJobFileAsync(
        [Description("The workspace ID containing the Apache Airflow Job (required)")] string workspaceId,
        [Description("The Apache Airflow Job ID (required)")] string airflowJobId,
        [Description("The file path to delete, e.g. 'dags/example_dag.py' (required)")] string filePath)
    {
        try
        {
            _validationService.ValidateRequiredString(workspaceId, nameof(workspaceId));
            _validationService.ValidateRequiredString(airflowJobId, nameof(airflowJobId));
            _validationService.ValidateRequiredString(filePath, nameof(filePath));

            await _airflowJobService.DeleteAirflowJobFileAsync(workspaceId, airflowJobId, filePath);

            var result = new
            {
                Success = true,
                Message = $"File '{filePath}' deleted successfully",
                WorkspaceId = workspaceId,
                AirflowJobId = airflowJobId,
                FilePath = filePath
            };

            return result.ToMcpJson();
        }
        catch (ArgumentException ex)
        {
            return ex.ToValidationError().ToMcpJson();
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToAuthenticationError().ToMcpJson();
        }
        catch (HttpRequestException ex)
        {
            return ex.ToHttpError().ToMcpJson();
        }
        catch (Exception ex)
        {
            return ex.ToOperationError("deleting Apache Airflow Job file").ToMcpJson();
        }
    }
}