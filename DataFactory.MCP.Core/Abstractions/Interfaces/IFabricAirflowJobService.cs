using DataFactory.MCP.Models.AirflowJob;
using DataFactory.MCP.Models.AirflowJob.Definition;
using DataFactory.MCP.Models.AirflowJob.Environment;
using DataFactory.MCP.Models.AirflowJob.Files;

namespace DataFactory.MCP.Abstractions.Interfaces;

/// <summary>
/// Service for interacting with Microsoft Fabric Apache Airflow Jobs API
/// </summary>
public interface IFabricAirflowJobService
{
    /// <summary>
    /// Lists all Apache Airflow Jobs from the specified workspace
    /// </summary>
    Task<ListAirflowJobsResponse> ListAirflowJobsAsync(
        string workspaceId,
        string? continuationToken = null);

    /// <summary>
    /// Creates a new Apache Airflow Job in the specified workspace
    /// </summary>
    Task<AirflowJob> CreateAirflowJobAsync(
        string workspaceId,
        CreateAirflowJobRequest request);

    /// <summary>
    /// Gets Apache Airflow Job metadata by ID
    /// </summary>
    Task<AirflowJob> GetAirflowJobAsync(
        string workspaceId,
        string airflowJobId);

    /// <summary>
    /// Updates Apache Airflow Job metadata (displayName, description)
    /// </summary>
    Task<AirflowJob> UpdateAirflowJobAsync(
        string workspaceId,
        string airflowJobId,
        UpdateAirflowJobRequest request);

    /// <summary>
    /// Deletes an Apache Airflow Job
    /// </summary>
    Task DeleteAirflowJobAsync(
        string workspaceId,
        string airflowJobId,
        bool hardDelete = false);

    /// <summary>
    /// Gets the definition of an Apache Airflow Job
    /// </summary>
    Task<AirflowJobDefinition> GetAirflowJobDefinitionAsync(
        string workspaceId,
        string airflowJobId,
        string? format = null);

    /// <summary>
    /// Updates the definition of an Apache Airflow Job
    /// </summary>
    Task UpdateAirflowJobDefinitionAsync(
        string workspaceId,
        string airflowJobId,
        AirflowJobDefinition definition,
        bool updateMetadata = false);

    /// <summary>
    /// Gets the environment status of an Apache Airflow Job cluster
    /// </summary>
    Task<AirflowEnvironmentStatusResponse> GetAirflowJobEnvironmentStatusAsync(
        string workspaceId,
        string airflowJobId);

    /// <summary>
    /// Gets the compute configuration of an Apache Airflow Job
    /// </summary>
    Task<AirflowComputeResponse> GetAirflowJobComputeAsync(
        string workspaceId,
        string airflowJobId);

    /// <summary>
    /// Gets the environment settings of an Apache Airflow Job
    /// </summary>
    Task<AirflowEnvironmentSettingsResponse> GetAirflowJobSettingsAsync(
        string workspaceId,
        string airflowJobId);

    /// <summary>
    /// Lists installed Python libraries in the Apache Airflow Job environment
    /// </summary>
    Task<AirflowLibrariesResponse> ListAirflowJobLibrariesAsync(
        string workspaceId,
        string airflowJobId,
        string? continuationToken = null);

    /// <summary>
    /// Lists DAG and plugin files in an Apache Airflow Job
    /// </summary>
    Task<AirflowJobFilesResponse> ListAirflowJobFilesAsync(
        string workspaceId,
        string airflowJobId,
        string? rootPath = null,
        string? continuationToken = null);

    /// <summary>
    /// Gets the content of a DAG or plugin file
    /// </summary>
    Task<string> GetAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath);

    /// <summary>
    /// Uploads (creates or updates) a DAG or plugin file in an Apache Airflow Job
    /// </summary>
    Task UploadAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath,
        string fileContent);

    /// <summary>
    /// Deletes a DAG or plugin file from an Apache Airflow Job
    /// </summary>
    Task DeleteAirflowJobFileAsync(
        string workspaceId,
        string airflowJobId,
        string filePath);
}
