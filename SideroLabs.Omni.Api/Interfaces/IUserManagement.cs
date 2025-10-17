namespace SideroLabs.Omni.Api.Interfaces;

public interface IUserManagement
{
	Task<object> CreateAsync(
		string email,
		string? role,
		CancellationToken cancellationToken);

	Task DeleteAsync(
		string email,
		CancellationToken cancellationToken);

	Task<List<object>> ListAsync(
		CancellationToken cancellationToken);

	Task SetRoleAsync(
		string email,
		string role,
		CancellationToken cancellationToken);

	Task<object> GetInfoAsync(
		string email,
		CancellationToken cancellationToken);
}
