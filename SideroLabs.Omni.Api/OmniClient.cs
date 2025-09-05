using Grpc.Core;
using Grpc.Net.Client;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Models.Responses;

namespace SideroLabs.Omni.Api;

/// <summary>
/// An Omni Client for interacting with the Sidero Labs Omni Management API
/// </summary>
/// <param name="omniClientOptions"></param>
public class OmniClient(OmniClientOptions omniClientOptions) : IDisposable
{
	private readonly OmniClientOptions _options = omniClientOptions ?? throw new ArgumentNullException(nameof(omniClientOptions));
	private readonly GrpcChannel _channel = CreateChannel(omniClientOptions);
	private bool _disposed;

	/// <summary>
	/// Gets the configured endpoint
	/// </summary>
	public string Endpoint => _options.Endpoint;

	/// <summary>
	/// Gets whether TLS is enabled
	/// </summary>
	public bool UseTls => _options.UseTls;

	/// <summary>
	/// Creates a gRPC channel with the specified options
	/// </summary>
	private static GrpcChannel CreateChannel(OmniClientOptions options)
	{
		var channelOptions = new GrpcChannelOptions
		{
			HttpHandler = CreateHttpHandler(options)
		};

		return GrpcChannel.ForAddress(options.Endpoint, channelOptions);
	}

	/// <summary>
	/// Creates an HTTP handler with the specified security options
	/// </summary>
	private static HttpMessageHandler CreateHttpHandler(OmniClientOptions options)
	{
		var handler = new HttpClientHandler();

		if (!options.ValidateCertificate)
		{
			handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
		}

		return handler;
	}

	/// <summary>
	/// Creates call options with authentication and timeout
	/// </summary>
	private CallOptions CreateCallOptions()
	{
		var headers = new Metadata();

		if (!string.IsNullOrEmpty(_options.AuthToken))
		{
			headers.Add("Authorization", $"Bearer {_options.AuthToken}");
		}

		var deadline = DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);
		return new CallOptions(headers: headers, deadline: deadline);
	}

	#region Cluster Management

	/// <summary>
	/// Lists all clusters
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<ListClustersResponse> ListClustersAsync(CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call once proto-generated client is available
		// For now, return a placeholder response that simulates gRPC behavior
		await Task.Delay(10, cancellationToken);
		return new ListClustersResponse
		{
			Clusters = new List<Cluster>
			{
				new()
				{
					Id = "cluster-1",
					Name = "production-cluster",
					Spec = new ClusterSpec
					{
						KubernetesVersion = "v1.28.0",
						TalosVersion = "v1.5.0",
						Features = new List<string> { "embedded-discovery-service" }
					},
					Status = new ClusterStatus
					{
						Phase = "Running",
						Ready = true,
						Conditions = new List<string> { "Healthy" }
					},
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
					UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				}
			}
		};
	}

	/// <summary>
	/// Creates a new cluster
	/// </summary>
	/// <param name="name">Name of the cluster to create</param>
	/// <param name="spec">Specification for the new cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<CreateClusterResponse> CreateClusterAsync(string name, ClusterSpec spec, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new CreateClusterResponse
		{
			Cluster = new Cluster
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				Spec = spec,
				Status = new ClusterStatus
				{
					Phase = "Provisioning",
					Ready = false,
					Conditions = new List<string> { "Creating" }
				},
				CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <summary>
	/// Gets a cluster by ID
	/// </summary>
	/// <param name="id">ID of the cluster to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<GetClusterResponse> GetClusterAsync(string id, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new GetClusterResponse
		{
			Cluster = new Cluster
			{
				Id = id,
				Name = $"cluster-{id}",
				Spec = new ClusterSpec
				{
					KubernetesVersion = "v1.28.0",
					TalosVersion = "v1.5.0"
				},
				Status = new ClusterStatus
				{
					Phase = "Running",
					Ready = true
				}
			}
		};
	}

	/// <summary>
	/// Updates a cluster
	/// </summary>
	/// <param name="id">ID of the cluster to update</param>
	/// <param name="spec">Updated specification for the cluster</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<UpdateClusterResponse> UpdateClusterAsync(string id, ClusterSpec spec, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new UpdateClusterResponse
		{
			Cluster = new Cluster
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <summary>
	/// Deletes a cluster
	/// </summary>
	/// <param name="id">ID of the cluster to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<DeleteClusterResponse> DeleteClusterAsync(string id, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new DeleteClusterResponse();
	}

	#endregion

	#region Machine Management

	/// <summary>
	/// Lists machines in a cluster
	/// </summary>
	/// <param name="clusterId">ID of the cluster to list machines for</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<ListMachinesResponse> ListMachinesAsync(string clusterId, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new ListMachinesResponse
		{
			Machines = new List<Machine>
			{
				new()
				{
					Id = "machine-1",
					Name = "control-plane-1",
					ClusterId = clusterId,
					Spec = new MachineSpec
					{
						Role = "controlplane",
						Labels = new Dictionary<string, string>
						{
							{ "node-role.kubernetes.io/control-plane", "" },
							{ "environment", "production" }
						}
					},
					Status = new MachineStatus
					{
						Phase = "Running",
						Ready = true,
						Address = "10.0.1.10"
					},
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
					UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				}
			}
		};
	}

	/// <summary>
	/// Gets a machine by ID
	/// </summary>
	/// <param name="id">ID of the machine to retrieve</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<GetMachineResponse> GetMachineAsync(string id, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new GetMachineResponse
		{
			Machine = new Machine
			{
				Id = id,
				Name = $"machine-{id}",
				Spec = new MachineSpec { Role = "worker" },
				Status = new MachineStatus
				{
					Phase = "Running",
					Ready = true,
					Address = "10.0.1.20"
				}
			}
		};
	}

	/// <summary>
	/// Updates a machine
	/// </summary>
	/// <param name="id">ID of the machine to update</param>
	/// <param name="spec">Updated specification for the machine</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<UpdateMachineResponse> UpdateMachineAsync(string id, MachineSpec spec, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new UpdateMachineResponse
		{
			Machine = new Machine
			{
				Id = id,
				Spec = spec,
				UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};
	}

	/// <summary>
	/// Deletes a machine
	/// </summary>
	/// <param name="id">ID of the machine to delete</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<DeleteMachineResponse> DeleteMachineAsync(string id, CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new DeleteMachineResponse();
	}

	#endregion

	#region Status

	/// <summary>
	/// Gets the status of the Omni service
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	public async Task<GetStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
	{
		// TODO: Replace with actual gRPC call
		await Task.Delay(10, cancellationToken);
		return new GetStatusResponse
		{
			Version = "1.0.0",
			Ready = true
		};
	}

	#endregion

	#region IDisposable

	/// <summary>
	/// Disposes the client and releases resources
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_channel?.Dispose();
			_disposed = true;
		}

		GC.SuppressFinalize(this);
	}

	#endregion
}
