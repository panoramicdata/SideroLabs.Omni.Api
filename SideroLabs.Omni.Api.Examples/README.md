# SideroLabs Omni API Examples

This project contains comprehensive examples demonstrating how to use the SideroLabs Omni API client library.

## Getting Started

### Prerequisites

- .NET 9.0 or later
- Access to a SideroLabs Omni instance
- PGP key pair for authentication

### Configuration

1. Copy `appsettings.example.json` to `appsettings.json`
2. Update the configuration with your Omni instance details:
   - `Endpoint`: Your Omni instance URL
   - `Identity`: Your username
   - `PgpPrivateKey`: Your PGP private key (in armored format)

### Running Examples

The examples can be run from the command line:

```bash
# Build and run a specific example
dotnet run basic

# Run with verbose error output
dotnet run readonly --verbose

# Run all examples
dotnet run all
```

## Available Examples

### Basic Examples

- **`basic`** - Basic usage example with configuration management
- **`structured`** - Structured basic usage example using the example framework

### Advanced Examples

- **`streaming`** - Advanced streaming operations and machine management
- **`service-accounts`** - Service account lifecycle management
- **`provisioning`** - Machine provisioning with schematics
- **`comprehensive`** - All available ManagementService operations

### Safety Examples

- **`readonly`** - Read-only mode demonstration

### Run All

- **`all`** - Run all examples sequentially with user prompts

## Example Categories

### Configuration Management

Examples showing how to:
- Get kubeconfig for cluster access
- Get talosconfig for Talos cluster access
- Get omniconfig for omnictl
- Validate configuration files

### Service Account Management

Examples showing how to:
- Create service accounts
- List existing service accounts
- Renew service account credentials
- Destroy service accounts

### Machine Operations

Examples showing how to:
- Stream machine logs in real-time
- Check Kubernetes upgrade readiness
- Create schematics for machine provisioning

### Streaming Operations

Examples showing how to:
- Stream machine logs with filtering
- Stream Kubernetes manifest synchronization
- Handle real-time data streams

### Read-Only Mode

Examples showing how to:
- Enable read-only mode for safety
- Handle read-only mode exceptions
- Distinguish between read and write operations

## Project Structure

```
SideroLabs.Omni.Api.Examples/
├── Infrastructure/
│   ├── ExampleBase.cs              # Base class for structured examples
│   ├── ExampleConfigurationFactory.cs # Configuration helpers
│   ├── IExampleOutput.cs           # Output interface
│   └── ConsoleExampleOutput.cs     # Console output implementation
├── Scenarios/
│   └── BasicUsageExample.cs        # Structured basic usage example
├── OmniClientExample.cs            # Static example methods
├── Program.cs                      # Main entry point
├── appsettings.json                # Configuration file
└── appsettings.example.json        # Example configuration
```

## Example Framework

The project includes a framework for creating structured examples:

- **`ExampleBase`** - Base class with error handling and cancellation support
- **`IExampleOutput`** - Interface for output formatting
- **`ConsoleExampleOutput`** - Console implementation with colored output
- **`ExampleConfigurationFactory`** - Helper for creating common configurations

## Error Handling

All examples include comprehensive error handling for:

- Network connectivity issues
- Authentication failures
- Read-only mode violations
- Operation cancellation
- gRPC errors

## Configuration Options

The examples support various configuration scenarios:

- **Standard options** - Basic configuration for most examples
- **Read-only options** - Safe configuration that prevents write operations
- **Streaming options** - Configuration with longer timeouts for streaming operations

## Contributing

When adding new examples:

1. Consider using the structured approach with `ExampleBase`
2. Include comprehensive error handling
3. Add appropriate documentation
4. Update this README with the new example
5. Test with both valid and invalid configurations

## Notes

- Examples use placeholder values that need to be replaced with real credentials
- Some examples may require specific permissions or cluster configurations
- Network timeouts are intentionally short for demonstration purposes
- Read-only mode examples are safe to run in production environments
