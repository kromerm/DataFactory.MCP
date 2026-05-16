using DataFactory.MCP.Abstractions;
using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Extensions;
using DataFactory.MCP.Infrastructure.Http;
using DataFactory.MCP.Models.AirflowJob;
using DataFactory.MCP.Models.AirflowJob.Definition;
using DataFactory.MCP.Models.AirflowJob.Environment;
using DataFactory.MCP.Models.AirflowJob.Files;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace DataFactory.MCP.Services;

/// <summary>
/// Service for interacting with Microsoft Fabric Apache Airflow Jobs API
/// </summary>
public class FabricAirflowJobService : FabricServiceBase, IFabricAirflowJobService
{
    public FabricAirflowJobService(
        IHttpClientFactory httpClientFactory,
        ILogger<FabricAirflowJobService> logger,
        IValidationService validationService)
        : base(httpClientFactory, logger, validationService)
    {
    }

    public async Task<ListAirflowJobsResponse> ListAirflowJobsAsync(
        string workspaceId,
        string? continuationToken = null)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs")
                .BuildEndpoint();
            Logger.LogInformation("Fetching Apache Airflow Jobs from workspace {WorkspaceId}", workspaceId);

            var response = await GetAsync<ListAirflowJobsResponse>(endpoint, continuationToken);

            Logger.LogInformation("Successfully retrieved {Count} Apache Airflow Jobs from workspace {WorkspaceId}",
                response?.Value?.Count ?? 0, workspaceId);
            return response ?? new ListAirflowJobsResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching Apache Airflow Jobs from workspace {WorkspaceId}", workspaceId);
            throw;
        }
    }

    public async Task<AirflowJob> CreateAirflowJobAsync(
        string workspaceId,
        CreateAirflowJobRequest request)
    {
        try
        {
            ValidateGuids((workspaceId, nameof(workspaceId)));
            ValidationService.ValidateAndThrow(request, nameof(request));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs")
                .BuildEndpoint();
            Logger.LogInformation("Creating Apache Airflow Job '{DisplayName}' in workspace {WorkspaceId}",
                request.DisplayName, workspaceId);

            var created = await PostAsync<AirflowJob>(endpoint, request);

            Logger.LogInformation("Successfully created Apache Airflow Job '{DisplayName}' with ID {AirflowJobId} in workspace {WorkspaceId}",
                request.DisplayName, created?.Id, workspaceId);

            return created ?? new AirflowJob();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating Apache Airflow Job '{DisplayName}' in workspace {WorkspaceId}",
                request?.DisplayName, workspaceId);
            throw;
        }
    }

    public async Task<AirflowJob> GetAirflowJobAsync(
        string workspaceId,
        string airflowJobId)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}")
                .BuildEndpoint();
            Logger.LogInformation("Fetching Apache Airflow Job {AirflowJobId} from workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var airflowJob = await GetAsync<AirflowJob>(endpoint);

            Logger.LogInformation("Successfully retrieved Apache Airflow Job {AirflowJobId}", airflowJobId);
            return airflowJob ?? throw new InvalidOperationException($"Apache Airflow Job {airflowJobId} not found");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching Apache Airflow Job {AirflowJobId} from workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowJob> UpdateAirflowJobAsync(
        string workspaceId,
        string airflowJobId,
        UpdateAirflowJobRequest request)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}")
                .BuildEndpoint();
            Logger.LogInformation("Updating Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var updated = await PatchAsync<AirflowJob>(endpoint, request);

            Logger.LogInformation("Successfully updated Apache Airflow Job {AirflowJobId}", airflowJobId);
            return updated ?? throw new InvalidOperationException($"Failed to update Apache Airflow Job {airflowJobId}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task DeleteAirflowJobAsync(
        string workspaceId,
        string airflowJobId,
        bool hardDelete = false)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}")
                .WithQueryParam("hardDelete", hardDelete ? (bool?)true : null)
                .Build();
            Logger.LogInformation("Deleting Apache Airflow Job {AirflowJobId} from workspace {WorkspaceId} (hardDelete={HardDelete})",
                airflowJobId, workspaceId, hardDelete);

            var response = await HttpClient.DeleteAsync(url);
            await response.EnsureSuccessOrThrowAsync();

            Logger.LogInformation("Successfully deleted Apache Airflow Job {AirflowJobId}", airflowJobId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting Apache Airflow Job {AirflowJobId} from workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowJobDefinition> GetAirflowJobDefinitionAsync(
        string workspaceId,
        string airflowJobId,
        string? format = null)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/getDefinition")
                .WithQueryParam("format", format)
                .BuildEndpoint();
            Logger.LogInformation("Getting definition for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var emptyRequest = new { };
            var response = await PostAsync<GetAirflowJobDefinitionResponse>(endpoint, emptyRequest)
                           ?? throw new InvalidOperationException("Failed to get Apache Airflow Job definition response");

            Logger.LogInformation("Successfully retrieved definition for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return response.Definition;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting definition for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task UpdateAirflowJobDefinitionAsync(
        string workspaceId,
        string airflowJobId,
        AirflowJobDefinition definition,
        bool updateMetadata = false)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var endpoint = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/updateDefinition")
                .WithQueryParam("updateMetadata", updateMetadata ? (bool?)true : null)
                .BuildEndpoint();
            Logger.LogInformation("Updating definition for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var request = new UpdateAirflowJobDefinitionRequest { Definition = definition };
            var success = await PostNoContentAsync(endpoint, request);

            if (!success)
            {
                throw new HttpRequestException($"Failed to update definition for Apache Airflow Job {airflowJobId}");
            }

            Logger.LogInformation("Successfully updated definition for Apache Airflow Job {AirflowJobId}", airflowJobId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating definition for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowEnvironmentStatusResponse> GetAirflowJobEnvironmentStatusAsync(
        string workspaceId,
        string airflowJobId)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/environment")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Getting environment status for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            var result = await response.ReadAsJsonAsync<AirflowEnvironmentStatusResponse>(JsonOptions);

            Logger.LogInformation("Successfully retrieved environment status for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return result ?? new AirflowEnvironmentStatusResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting environment status for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowComputeResponse> GetAirflowJobComputeAsync(
        string workspaceId,
        string airflowJobId)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/environment/compute")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Getting compute configuration for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            var result = await response.ReadAsJsonAsync<AirflowComputeResponse>(JsonOptions);

            Logger.LogInformation("Successfully retrieved compute configuration for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return result ?? new AirflowComputeResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting compute configuration for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowEnvironmentSettingsResponse> GetAirflowJobSettingsAsync(
        string workspaceId,
        string airflowJobId)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/environment/settings")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Getting environment settings for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            var result = await response.ReadAsJsonAsync<AirflowEnvironmentSettingsResponse>(JsonOptions);

            Logger.LogInformation("Successfully retrieved environment settings for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return result ?? new AirflowEnvironmentSettingsResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting environment settings for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowLibrariesResponse> ListAirflowJobLibrariesAsync(
        string workspaceId,
        string airflowJobId,
        string? continuationToken = null)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/environment/libraries")
                .WithQueryParam("beta", (bool?)true)
                .WithContinuationToken(continuationToken)
                .Build();
            Logger.LogInformation("Listing libraries for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            var result = await response.ReadAsJsonAsync<AirflowLibrariesResponse>(JsonOptions);

            Logger.LogInformation("Successfully retrieved libraries for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return result ?? new AirflowLibrariesResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listing libraries for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<AirflowJobFilesResponse> ListAirflowJobFilesAsync(
        string workspaceId,
        string airflowJobId,
        string? rootPath = null,
        string? continuationToken = null)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/files")
                .WithQueryParam("beta", (bool?)true)
                .WithQueryParam("rootPath", rootPath)
                .WithContinuationToken(continuationToken)
                .Build();
            Logger.LogInformation("Listing files for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            var result = await response.ReadAsJsonAsync<AirflowJobFilesResponse>(JsonOptions);

            Logger.LogInformation("Successfully retrieved files for Apache Airflow Job {AirflowJobId}", airflowJobId);
            return result ?? new AirflowJobFilesResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listing files for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task<string> GetAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/files/{filePath}")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Getting file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);

            var response = await HttpClient.GetAsync(url);
            await response.EnsureSuccessOrThrowAsync();
            var content = await response.Content.ReadAsStringAsync();

            Logger.LogInformation("Successfully retrieved file '{FilePath}' for Apache Airflow Job {AirflowJobId}", filePath, airflowJobId);
            return content;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task UploadAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath,
        string fileContent)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/files/{filePath}")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Uploading file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);

            var response = await HttpClient.PutAsync(url, new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(fileContent)) { Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") } });
            await response.EnsureSuccessOrThrowAsync();

            Logger.LogInformation("Successfully uploaded file '{FilePath}' for Apache Airflow Job {AirflowJobId}", filePath, airflowJobId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);
            throw;
        }
    }

    public async Task DeleteAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath)
    {
        try
        {
            ValidateGuids(
                (workspaceId, nameof(workspaceId)),
                (airflowJobId, nameof(airflowJobId)));

            var url = FabricUrlBuilder.ForFabricApi()
                .WithLiteralPath($"workspaces/{workspaceId}/apacheAirflowJobs/{airflowJobId}/files/{filePath}")
                .WithQueryParam("beta", (bool?)true)
                .Build();
            Logger.LogInformation("Deleting file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);

            var response = await HttpClient.DeleteAsync(url);
            await response.EnsureSuccessOrThrowAsync();

            Logger.LogInformation("Successfully deleted file '{FilePath}' for Apache Airflow Job {AirflowJobId}", filePath, airflowJobId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting file '{FilePath}' for Apache Airflow Job {AirflowJobId} in workspace {WorkspaceId}",
                filePath, airflowJobId, workspaceId);
            throw;
        }
    }
}