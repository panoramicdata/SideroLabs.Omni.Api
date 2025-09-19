using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api.Interfaces;

/// <summary>
/// Interface for configuration template management
/// </summary>
public interface IConfigurationTemplateManagement
{
	/// <summary>
	/// Lists all configuration templates
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListConfigTemplatesResponse> ListConfigTemplatesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Lists configuration templates filtered by type
	/// </summary>
	/// <param name="type">Type filter for templates</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<ListConfigTemplatesResponse> ListConfigTemplatesAsync(ConfigTemplateType type, CancellationToken cancellationToken);

	/// <summary>
	/// Gets a configuration template by ID
	/// </summary>
	/// <param name="id">ID of the template to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<GetConfigTemplateResponse> GetConfigTemplateAsync(string id, CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new configuration template
	/// </summary>
	/// <param name="name">Name of the template</param>
	/// <param name="type">Type of the template</param>
	/// <param name="spec">Template specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<CreateConfigTemplateResponse> CreateConfigTemplateAsync(string name, ConfigTemplateType type, ConfigTemplateSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a configuration template
	/// </summary>
	/// <param name="id">ID of the template to update</param>
	/// <param name="spec">Updated template specification</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<UpdateConfigTemplateResponse> UpdateConfigTemplateAsync(string id, ConfigTemplateSpec spec, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a configuration template
	/// </summary>
	/// <param name="id">ID of the template to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	Task<DeleteConfigTemplateResponse> DeleteConfigTemplateAsync(string id, CancellationToken cancellationToken);
}
