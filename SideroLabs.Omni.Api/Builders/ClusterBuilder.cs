using SideroLabs.Omni.Api.Resources;

namespace SideroLabs.Omni.Api.Builders;

/// <summary>
/// Builder for creating Cluster resources with a fluent API
/// </summary>
public class ClusterBuilder
{
	private readonly Cluster _cluster;

	/// <summary>
	/// Initializes a new instance of the ClusterBuilder class
	/// </summary>
	/// <param name="clusterName">The name/ID of the cluster</param>
	public ClusterBuilder(string clusterName)
	{
		if (string.IsNullOrWhiteSpace(clusterName))
		{
			throw new ArgumentException("Cluster name cannot be null or empty", nameof(clusterName));
		}

		_cluster = new Cluster
		{
			Metadata = new ResourceMetadata
			{
				Id = clusterName,
				Namespace = "default"
			}
		};
	}

	/// <summary>
	/// Sets the Kubernetes version for the cluster
	/// </summary>
	public ClusterBuilder WithKubernetesVersion(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new ArgumentException("Kubernetes version cannot be null or empty", nameof(version));
		}

		_cluster.Spec.KubernetesVersion = version;
		return this;
	}

	/// <summary>
	/// Sets the Talos version for the cluster
	/// </summary>
	public ClusterBuilder WithTalosVersion(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new ArgumentException("Talos version cannot be null or empty", nameof(version));
		}

		_cluster.Spec.TalosVersion = version;
		return this;
	}

	/// <summary>
	/// Configures the cluster network settings
	/// </summary>
	public ClusterBuilder WithNetwork(NetworkConfig network)
	{
		_cluster.Spec.Network = network ?? throw new ArgumentNullException(nameof(network));
		return this;
	}

	/// <summary>
	/// Sets the namespace for the cluster
	/// </summary>
	public ClusterBuilder InNamespace(string @namespace)
	{
		if (string.IsNullOrWhiteSpace(@namespace))
		{
			throw new ArgumentException("Namespace cannot be null or empty", nameof(@namespace));
		}

		_cluster.Metadata.Namespace = @namespace;
		return this;
	}

	/// <summary>
	/// Adds a label to the cluster metadata
	/// </summary>
	public ClusterBuilder WithLabel(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Label key cannot be null or empty", nameof(key));
		}

		_cluster.Metadata.Labels ??= [];
		_cluster.Metadata.Labels[key] = value ?? string.Empty;
		return this;
	}

	/// <summary>
	/// Adds multiple labels to the cluster metadata
	/// </summary>
	public ClusterBuilder WithLabels(Dictionary<string, string> labels)
	{
		ArgumentNullException.ThrowIfNull(labels);

		_cluster.Metadata.Labels ??= [];
		foreach (var (key, value) in labels)
		{
			_cluster.Metadata.Labels[key] = value;
		}

		return this;
	}

	/// <summary>
	/// Adds an annotation to the cluster metadata
	/// </summary>
	public ClusterBuilder WithAnnotation(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Annotation key cannot be null or empty", nameof(key));
		}

		_cluster.Metadata.Annotations ??= [];
		_cluster.Metadata.Annotations[key] = value ?? string.Empty;
		return this;
	}

	/// <summary>
	/// Builds and returns the configured Cluster resource
	/// </summary>
	public Cluster Build()
	{
		// Validate required fields
		if (string.IsNullOrWhiteSpace(_cluster.Spec.KubernetesVersion))
		{
			throw new InvalidOperationException("Kubernetes version must be set before building the cluster");
		}

		if (string.IsNullOrWhiteSpace(_cluster.Spec.TalosVersion))
		{
			throw new InvalidOperationException("Talos version must be set before building the cluster");
		}

		return _cluster;
	}

	/// <summary>
	/// Implicitly converts the builder to a Cluster
	/// </summary>
	public static implicit operator Cluster(ClusterBuilder builder) => builder.Build();
}
