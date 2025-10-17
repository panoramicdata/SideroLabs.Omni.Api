using SideroLabs.Omni.Api.Interfaces;

namespace SideroLabs.Omni.Api.Services;

internal class UserManagement : IUserManagement
{
	public Task<object> CreateAsync(
		string email,
		string? role,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(
		string email,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<object> GetInfoAsync(
		string email,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<List<object>> ListAsync(
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task SetRoleAsync(
		string email,
		string role,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
