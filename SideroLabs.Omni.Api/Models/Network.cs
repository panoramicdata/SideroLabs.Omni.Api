namespace SideroLabs.Omni.Api.Models;

/// <summary>
/// Represents a network configuration in the Omni system
/// </summary>
public class NetworkConfig
{
	/// <summary>
	/// Unique identifier for the network configuration
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Display name of the network configuration
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Description of the network configuration
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Network configuration specification
	/// </summary>
	public NetworkConfigSpec Spec { get; set; } = new();

	/// <summary>
	/// Current status of the network configuration
	/// </summary>
	public NetworkConfigStatus Status { get; set; } = new();

	/// <summary>
	/// Timestamp when the network config was created (Unix timestamp)
	/// </summary>
	public long CreatedAt { get; set; }

	/// <summary>
	/// Timestamp when the network config was last updated (Unix timestamp)
	/// </summary>
	public long UpdatedAt { get; set; }
}

/// <summary>
/// Network configuration specification
/// </summary>
public class NetworkConfigSpec
{
	/// <summary>
	/// CIDR blocks for the network
	/// </summary>
	public List<string> CidrBlocks { get; set; } = new();

	/// <summary>
	/// DNS configuration
	/// </summary>
	public DnsConfig DnsConfig { get; set; } = new();

	/// <summary>
	/// Load balancer configuration
	/// </summary>
	public LoadBalancerConfig LoadBalancer { get; set; } = new();

	/// <summary>
	/// Ingress configuration
	/// </summary>
	public IngressConfig Ingress { get; set; } = new();

	/// <summary>
	/// Service mesh configuration
	/// </summary>
	public ServiceMeshConfig? ServiceMesh { get; set; }

	/// <summary>
	/// Network policies
	/// </summary>
	public List<NetworkPolicy> Policies { get; set; } = new();

	/// <summary>
	/// CNI (Container Network Interface) plugin
	/// </summary>
	public CniPlugin CniPlugin { get; set; } = new();
}

/// <summary>
/// DNS configuration
/// </summary>
public class DnsConfig
{
	/// <summary>
	/// DNS servers
	/// </summary>
	public List<string> Servers { get; set; } = new();

	/// <summary>
	/// Search domains
	/// </summary>
	public List<string> SearchDomains { get; set; } = new();

	/// <summary>
	/// DNS options
	/// </summary>
	public Dictionary<string, string> Options { get; set; } = new();

	/// <summary>
	/// CoreDNS configuration
	/// </summary>
	public string? CoreDnsConfig { get; set; }
}

/// <summary>
/// Load balancer configuration
/// </summary>
public class LoadBalancerConfig
{
	/// <summary>
	/// Load balancer type
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// IP address pools
	/// </summary>
	public List<IpAddressPool> IpPools { get; set; } = new();

	/// <summary>
	/// Load balancer class
	/// </summary>
	public string? LoadBalancerClass { get; set; }

	/// <summary>
	/// External traffic policy
	/// </summary>
	public string ExternalTrafficPolicy { get; set; } = "Cluster";
}

/// <summary>
/// IP address pool configuration
/// </summary>
public class IpAddressPool
{
	/// <summary>
	/// Name of the IP pool
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// CIDR blocks in the pool
	/// </summary>
	public List<string> CidrBlocks { get; set; } = new();

	/// <summary>
	/// Whether this pool is the default
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// Protocols supported by this pool
	/// </summary>
	public List<string> Protocols { get; set; } = new();
}

/// <summary>
/// CNI plugin configuration
/// </summary>
public class CniPlugin
{
	/// <summary>
	/// CNI plugin type (calico, flannel, cilium, etc.)
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// CNI plugin version
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// CNI configuration parameters
	/// </summary>
	public Dictionary<string, object> Config { get; set; } = new();

	/// <summary>
	/// MTU size
	/// </summary>
	public int? Mtu { get; set; }

	/// <summary>
	/// IPAM (IP Address Management) configuration
	/// </summary>
	public IpamConfig IpamConfig { get; set; } = new();
}

/// <summary>
/// IPAM configuration
/// </summary>
public class IpamConfig
{
	/// <summary>
	/// IPAM type
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Subnet configuration
	/// </summary>
	public List<SubnetConfig> Subnets { get; set; } = new();

	/// <summary>
	/// Routes configuration
	/// </summary>
	public List<RouteConfig> Routes { get; set; } = new();
}

/// <summary>
/// Ingress configuration
/// </summary>
public class IngressConfig
{
	/// <summary>
	/// Ingress controller type
	/// </summary>
	public string ControllerType { get; set; } = string.Empty;

	/// <summary>
	/// Default ingress class
	/// </summary>
	public string? DefaultIngressClass { get; set; }

	/// <summary>
	/// SSL/TLS configuration
	/// </summary>
	public TlsConfig TlsConfig { get; set; } = new();

	/// <summary>
	/// Default backend service
	/// </summary>
	public string? DefaultBackend { get; set; }
}

/// <summary>
/// TLS configuration
/// </summary>
public class TlsConfig
{
	/// <summary>
	/// Certificate issuer
	/// </summary>
	public string? CertIssuer { get; set; }

	/// <summary>
	/// Certificate manager type
	/// </summary>
	public string? CertManager { get; set; }

	/// <summary>
	/// Default TLS certificate secret
	/// </summary>
	public string? DefaultCertSecret { get; set; }

	/// <summary>
	/// TLS minimum version
	/// </summary>
	public string? MinVersion { get; set; }

	/// <summary>
	/// TLS cipher suites
	/// </summary>
	public List<string> CipherSuites { get; set; } = new();
}

/// <summary>
/// Service mesh configuration
/// </summary>
public class ServiceMeshConfig
{
	/// <summary>
	/// Service mesh type (istio, linkerd, etc.)
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Version of the service mesh
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// Service mesh configuration parameters
	/// </summary>
	public Dictionary<string, object> Config { get; set; } = new();

	/// <summary>
	/// Whether mTLS is enabled
	/// </summary>
	public bool MutualTlsEnabled { get; set; }

	/// <summary>
	/// Observability configuration
	/// </summary>
	public ObservabilityConfig Observability { get; set; } = new();
}

/// <summary>
/// Observability configuration for service mesh
/// </summary>
public class ObservabilityConfig
{
	/// <summary>
	/// Whether tracing is enabled
	/// </summary>
	public bool TracingEnabled { get; set; }

	/// <summary>
	/// Tracing provider
	/// </summary>
	public string? TracingProvider { get; set; }

	/// <summary>
	/// Whether metrics are enabled
	/// </summary>
	public bool MetricsEnabled { get; set; }

	/// <summary>
	/// Metrics provider
	/// </summary>
	public string? MetricsProvider { get; set; }

	/// <summary>
	/// Whether access logging is enabled
	/// </summary>
	public bool AccessLogsEnabled { get; set; }
}

/// <summary>
/// Network policy definition
/// </summary>
public class NetworkPolicy
{
	/// <summary>
	/// Name of the network policy
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Namespace for the policy
	/// </summary>
	public string Namespace { get; set; } = string.Empty;

	/// <summary>
	/// Pod selector for the policy
	/// </summary>
	public Dictionary<string, string> PodSelector { get; set; } = new();

	/// <summary>
	/// Ingress rules
	/// </summary>
	public List<NetworkPolicyRule> IngressRules { get; set; } = new();

	/// <summary>
	/// Egress rules
	/// </summary>
	public List<NetworkPolicyRule> EgressRules { get; set; } = new();
}

/// <summary>
/// Network policy rule
/// </summary>
public class NetworkPolicyRule
{
	/// <summary>
	/// Allowed ports
	/// </summary>
	public List<NetworkPolicyPort> Ports { get; set; } = new();

	/// <summary>
	/// Source/destination selectors
	/// </summary>
	public NetworkPolicyPeer From { get; set; } = new();

	/// <summary>
	/// Destination selectors (for egress)
	/// </summary>
	public NetworkPolicyPeer To { get; set; } = new();
}

/// <summary>
/// Network policy port specification
/// </summary>
public class NetworkPolicyPort
{
	/// <summary>
	/// Protocol (TCP, UDP, SCTP)
	/// </summary>
	public string Protocol { get; set; } = "TCP";

	/// <summary>
	/// Port number or name
	/// </summary>
	public string Port { get; set; } = string.Empty;

	/// <summary>
	/// End port for port ranges
	/// </summary>
	public int? EndPort { get; set; }
}

/// <summary>
/// Network policy peer specification
/// </summary>
public class NetworkPolicyPeer
{
	/// <summary>
	/// Pod selector
	/// </summary>
	public Dictionary<string, string>? PodSelector { get; set; }

	/// <summary>
	/// Namespace selector
	/// </summary>
	public Dictionary<string, string>? NamespaceSelector { get; set; }

	/// <summary>
	/// IP blocks
	/// </summary>
	public List<IpBlock> IpBlocks { get; set; } = new();
}

/// <summary>
/// IP block specification
/// </summary>
public class IpBlock
{
	/// <summary>
	/// CIDR block
	/// </summary>
	public string Cidr { get; set; } = string.Empty;

	/// <summary>
	/// Excluded CIDR blocks
	/// </summary>
	public List<string> Except { get; set; } = new();
}

/// <summary>
/// Current status of a network configuration
/// </summary>
public class NetworkConfigStatus
{
	/// <summary>
	/// Current phase of the network configuration
	/// </summary>
	public string Phase { get; set; } = string.Empty;

	/// <summary>
	/// Whether the network configuration is ready
	/// </summary>
	public bool Ready { get; set; }

	/// <summary>
	/// List of applied configurations
	/// </summary>
	public List<string> AppliedConfigs { get; set; } = new();

	/// <summary>
	/// Current conditions
	/// </summary>
	public List<string> Conditions { get; set; } = new();

	/// <summary>
	/// Error message if configuration failed
	/// </summary>
	public string? ErrorMessage { get; set; }
}
