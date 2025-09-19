namespace SideroLabs.Omni.Api.Exceptions;

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
/// <param name="message">The exception message</param>
/// <param name="innerException">The inner exception</param>
public class OmniAuthenticationException(string message, Exception? innerException = null) : OmniException("authentication", message, null, innerException)
{
}
