using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Management;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Constants;
using SideroLabs.Omni.Api.Enums;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Models;
using SideroLabs.Omni.Api.Security;

namespace SideroLabs.Omni.Api.Services;

/// <summary>
/// Service for validation operations
/// </summary>
internal class ValidationService(ManagementConnection connection) : ManagementServiceBase(connection), IValidationService
{
	public async Task ValidateConfigAsync(string config, CancellationToken cancellationToken = default)
	{
		var request = new ValidateConfigRequest
		{
			Config = config
		};

		await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ValidateConfigAsync,
			GrpcMethods.ValidateConfig,
			"configuration validation",
			cancellationToken);
	}

	public async Task<ValidateJsonSchemaResult> ValidateJsonSchemaAsync(
		string data,
		string schema,
		CancellationToken cancellationToken = default)
	{
		var request = new ValidateJsonSchemaRequest
		{
			Data = data,
			Schema = schema
		};

		var response = await CallHelper.ExecuteCallAsync(
			request,
			GrpcClient.ValidateJSONSchemaAsync,
			GrpcMethods.ValidateJsonSchema,
			"JSON schema validation",
			cancellationToken);

		var result = new ValidateJsonSchemaResult
		{
			Errors = ConvertProtoErrors(response.Errors)
		};

		if (!result.IsValid)
		{
			Logger.LogWarning("JSON schema validation failed with {ErrorCount} error(s)", result.TotalErrorCount);
		}

		return result;
	}

	private static List<ValidateJsonSchemaError> ConvertProtoErrors(
		Google.Protobuf.Collections.RepeatedField<ValidateJsonSchemaResponse.Types.Error> protoErrors)
	{
		var errors = new List<ValidateJsonSchemaError>();
		foreach (var protoError in protoErrors)
		{
			errors.Add(ConvertProtoError(protoError));
		}

		return errors;
	}

	private static ValidateJsonSchemaError ConvertProtoError(ValidateJsonSchemaResponse.Types.Error protoError)
	{
		var error = new ValidateJsonSchemaError
		{
			SchemaPath = protoError.SchemaPath ?? "",
			DataPath = protoError.DataPath ?? "",
			Cause = protoError.Cause ?? ""
		};

		if (protoError.Errors != null)
		{
			foreach (var nestedProtoError in protoError.Errors)
			{
				error.Errors.Add(ConvertProtoError(nestedProtoError));
			}
		}

		return error;
	}
}
