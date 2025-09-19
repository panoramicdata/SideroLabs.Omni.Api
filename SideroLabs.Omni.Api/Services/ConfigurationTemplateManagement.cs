using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Implementation of configuration template management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the ConfigurationTemplateManagement class
/// </remarks>
/// <param name="options">Client options</param>
/// <param name="channel">gRPC channel</param>
/// <param name="authenticator">Authentication provider</param>
internal class ConfigurationTemplateManagement(
	OmniClientOptions options,
	GrpcChannel channel,
	OmniAuthenticator? authenticator) : OmniServiceBase(options, channel, authenticator), IConfigurationTemplateManagement
{

	/// <inheritdoc />
	public async Task<ListConfigTemplatesResponse> ListConfigTemplatesAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Listing all configuration templates...");
		await Task.Delay(10, cancellationToken);

		var templates = new List<ConfigTemplate>
		{
			new()
			{
				Id = "template-1",
				Name = "Basic Cluster Template",
				Type = ConfigTemplateType.Cluster,
				Spec = new ConfigTemplateSpec { Version = "1.0.0" }
			}
		};

		return new ListConfigTemplatesResponse { Templates = templates };
	}

	/// <inheritdoc />
	public async Task<ListConfigTemplatesResponse> ListConfigTemplatesAsync(ConfigTemplateType type, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Listing configuration templates of type: {Type}", type);
		await Task.Delay(10, cancellationToken);

		var allTemplates = await ListConfigTemplatesAsync(cancellationToken);
		var filteredTemplates = allTemplates.Templates.Where(t => t.Type == type).ToList();

		return new ListConfigTemplatesResponse { Templates = filteredTemplates };
	}

	/// <inheritdoc />
	public async Task<GetConfigTemplateResponse> GetConfigTemplateAsync(string id, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Getting configuration template: {Id}", id);
		await Task.Delay(10, cancellationToken);

		return new GetConfigTemplateResponse
		{
			Template = new ConfigTemplate
			{
				Id = id,
				Name = $"template-{id}",
				Type = ConfigTemplateType.Cluster
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Create, Description = "Creates a new configuration template")]
	public async Task<CreateConfigTemplateResponse> CreateConfigTemplateAsync(string name, ConfigTemplateType type, ConfigTemplateSpec spec, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Creating configuration template: {Name}", name);
		EnsureWriteActionAllowed("Configuration template");
		await Task.Delay(10, cancellationToken);

		return new CreateConfigTemplateResponse
		{
			Template = new ConfigTemplate
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Type = type,
				Spec = spec
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Update, Description = "Updates a configuration template")]
	public async Task<UpdateConfigTemplateResponse> UpdateConfigTemplateAsync(string id, ConfigTemplateSpec spec, CancellationToken cancellationToken)
	{
		Logger.LogInformation("Updating configuration template: {Id}", id);
		EnsureWriteActionAllowed("Configuration template");
		await Task.Delay(10, cancellationToken);

		return new UpdateConfigTemplateResponse
		{
			Template = new ConfigTemplate
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <inheritdoc />
	[IsWriteAction(WriteActionType.Delete, Description = "Deletes a configuration template")]
	public async Task<DeleteConfigTemplateResponse> DeleteConfigTemplateAsync(string id, CancellationToken cancellationToken)
	{
		Logger.LogWarning("Deleting configuration template: {Id}", id);
		EnsureWriteActionAllowed("Configuration template");
		await Task.Delay(10, cancellationToken);

		return new DeleteConfigTemplateResponse { Success = true };
	}
}
