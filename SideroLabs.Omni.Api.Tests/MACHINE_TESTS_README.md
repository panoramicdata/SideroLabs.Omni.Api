# Machine Testing Guide

This guide explains how to configure and run tests for machine operations with your Omni instance.

## Overview

The integration tests include comprehensive tests for machine log streaming operations. These tests verify that you can:

- Stream logs from configured machines
- Use follow mode for real-time log streaming
- Control the number of tail lines returned
- Handle non-existent machines gracefully
- Validate input parameters

## Prerequisites

1. **Configured Omni Instance**: You need access to a running Omni instance with at least one machine configured
2. **Valid Credentials**: Your `appsettings.json` must have valid Omni credentials
3. **Machine Access**: The credentials must have permission to retrieve talosconfig and access machine logs

## How Machine Discovery Works

The tests automatically discover machines from your Omni instance:

1. **Automatic Discovery**: Tests call `GetTalosConfigAsync()` to retrieve the talosconfig
2. **Endpoint Extraction**: The talosconfig contains machine endpoints under the `contexts` section
3. **First Machine Selection**: Tests automatically use the first available machine endpoint
4. **No Manual Configuration Required**: No need to set environment variables or hardcode machine IDs!

### Talosconfig Structure Example

```yaml
contexts:
  my-cluster:
    endpoints:
      - 192.168.1.100  # First machine - automatically used by tests
      - 192.168.1.101  # Second machine
      - 192.168.1.102  # Third machine
```

## Running the Tests

### Run All Integration Tests
```bash
dotnet test
```

### Run Only Machine-Related Tests
```bash
dotnet test --filter "FullyQualifiedName~Machine"
```

### Run Specific Machine Tests
```bash
# Test basic machine log streaming
dotnet test --filter "RealWorld_StreamMachineLogs_WithConfiguredMachines"

# Test follow mode
dotnet test --filter "RealWorld_MachineLogStreaming_SupportsFollowMode"

# Test tail lines parameter
dotnet test --filter "RealWorld_MachineLogStreaming_SupportsTailLines"

# Test error handling
dotnet test --filter "RealWorld_MachineLogStreaming_HandlesNonExistentMachine"
```

## Test Descriptions

### `RealWorld_StreamMachineLogs_WithConfiguredMachines`
- **Purpose**: Verify you can stream logs from a real machine
- **Behavior**: 
  - Retrieves talosconfig from Omni
  - Extracts the first machine endpoint
  - Fetches and displays the last 10 log entries from that machine
- **Fallback**: If no machines are found, test provides helpful guidance

### `RealWorld_MachineLogStreaming_SupportsFollowMode`
- **Purpose**: Test real-time log streaming with follow mode
- **Behavior**: 
  - Automatically discovers first machine
  - Streams logs in follow mode for 5 seconds or 3 log entries, whichever comes first

### `RealWorld_MachineLogStreaming_SupportsTailLines`
- **Purpose**: Verify the tail lines parameter works correctly
- **Behavior**: 
  - Automatically discovers first machine
  - Requests 20 tail lines and verifies we don't exceed that limit

### `RealWorld_MachineLogStreaming_ValidatesInput`
- **Purpose**: Test API input validation
- **Requires**: None (uses empty machine ID)
- **Behavior**: Verifies the API properly validates input parameters

### `RealWorld_MachineLogStreaming_HandlesNonExistentMachine`
- **Purpose**: Test error handling for non-existent machines
- **Requires**: None (uses generated UUID)
- **Behavior**: Verifies the API returns NotFound for non-existent machines

## Expected Test Outcomes

### With Configured Machines
- ✅ Tests automatically discover machines from talosconfig
- ✅ Tests successfully stream log entries from the first machine
- ✅ Log entries are displayed in the test output
- ✅ Follow mode works for real-time streaming
- ✅ Tail lines parameter limits the number of logs returned

### Without Configured Machines
- ℹ️ Tests will detect no machines are available and skip gracefully
- ℹ️ Error handling tests will still pass (they don't require real machines)
- ℹ️ Tests will provide instructions about ensuring machines are configured in Omni

### With Reader Role Permissions
- 🔒 Tests may show `PermissionDenied` if your role doesn't allow machine log access
- ✅ This is expected and confirms authentication is working
- 💡 You may need elevated permissions to access machine logs

## Example Test Output

**Successful Test with Automatic Discovery:**
```
🚀 Testing machine log streaming with configured machines
🔍 Retrieving talosconfig to extract machine IDs...
✅ Found machine endpoint: 192.168.1.100
Attempting to stream logs from machine: 192.168.1.100
📄 Log entry 1: [2025-01-19 10:30:15] kernel: CPU0: Thread Overtemperature interrupt
📄 Log entry 2: [2025-01-19 10:30:16] systemd[1]: Started Machine Health Monitor
📄 Log entry 3: [2025-01-19 10:30:17] kubelet: Node ready
✅ Successfully streamed 3 log entries from machine 192.168.1.100
```

**Without Machines:**
```
🔍 Retrieving talosconfig to extract machine IDs...
⚠️ No machine endpoints found in talosconfig
ℹ️ No machines found in talosconfig - skipping test
💡 Tip: Ensure your Omni instance has at least one machine configured
```

**Permission Denied:**
```
🔒 Permission denied for machine log access - this is expected with Reader role
✅ gRPC authentication is working correctly (permission check succeeded)
```

## How It Works

The machine discovery process:

1. **Retrieve Configuration**: Tests call `client.Management.GetTalosConfigAsync()` to get the talosconfig
2. **Parse YAML**: Uses YamlDotNet to parse the talosconfig structure
3. **Extract Endpoints**: Navigates to `contexts.[context-name].endpoints` to find machine endpoints
4. **Select First Machine**: Uses the first endpoint in the list for testing
5. **Stream Logs**: Calls `StreamMachineLogsAsync()` with the discovered machine endpoint

### Why This Approach?

- **✅ Zero Configuration**: No need to manually configure TEST_MACHINE_ID
- **✅ Always Up-to-Date**: Uses actual machines from your Omni instance
- **✅ Cluster-Aware**: Automatically adapts to your cluster configuration
- **✅ Reliable**: If machines exist in your cluster, tests will find them
- **✅ Simple**: Just configure your Omni credentials and run tests

## Troubleshooting

### "No machines found" Message
- **Solution**: Ensure your Omni instance has at least one machine provisioned
- **Check**: Verify machines are visible in Omni UI
- **Verify**: Your credentials have permission to retrieve talosconfig
- **Tip**: Check that your cluster has been properly set up with machines

### "Permission denied" Error
- **Solution**: Check your user role in Omni
- **Required**: Your role must have permission to access talosconfig and machine logs
- **Alternative**: Contact your Omni administrator for elevated permissions
- **Note**: Reader role typically cannot access machine logs

### "Operation timed out" Error
- **Solution**: Increase timeout in test configuration
- **Check**: Network connectivity to Omni instance
- **Verify**: Machine is online and responsive
- **Debug**: Try accessing the machine through Omni UI first

### No Log Entries Returned
- **Possible Causes**:
  - Machine has no recent logs  
  - Machine is not generating logs
  - Tail lines parameter is too low
  - Machine is offline or in an error state
- **Solution**: Check machine status in Omni UI first
- **Verify**: Machine is actively running workloads

### Talosconfig Parsing Errors
- **Possible Causes**:
  - Unexpected talosconfig format
  - Missing contexts section
  - Empty endpoints list
- **Solution**: Verify talosconfig structure manually
- **Debug**: Check test output for parsing error details

## CI/CD Integration

No special configuration needed! Just ensure your CI/CD pipeline has valid Omni credentials configured in `appsettings.json` or through user secrets.

**GitHub Actions Example:**
```yaml
- name: Run Integration Tests
  run: dotnet test
  env:
    # Only Omni credentials needed - machines auto-discovered
    OMNI__ENDPOINT: ${{ secrets.OMNI_ENDPOINT }}
    OMNI__AUTHTOKEN: ${{ secrets.OMNI_AUTH_TOKEN }}
```

## Additional Resources

- [Omni Documentation](https://omni.siderolabs.com/)
- [Talos Documentation](https://www.talos.dev/)
- [gRPC Documentation](https://grpc.io/docs/)
- [omnictl CLI Documentation](https://omni.siderolabs.com/docs/reference/cli/omnictl/)

## Support

If you encounter issues:
1. Check the test output logs for detailed error messages
2. Verify your Omni instance is accessible
3. Confirm your credentials have the necessary permissions
4. Verify machines are configured in your Omni instance
5. Try accessing machines through the Omni web UI first
6. Review the Omni API documentation

## Contributing

If you'd like to improve the machine testing:
1. Check the test implementation in `IntegrationTests.cs`
2. Review the `GetFirstMachineIdAsync()` method for machine discovery logic
3. Submit improvements or bug fixes via pull request
4. Include tests and documentation updates
