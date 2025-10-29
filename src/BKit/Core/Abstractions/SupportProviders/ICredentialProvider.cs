namespace BKit.Core.Abstractions.SupportProviders
{
    /// <summary>
    /// Defines a contract for providers that support getting credentials from various sources.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be implemented by classes that provide mechanisms for retrieving 
    /// credentials from various sources, such as secure vaults, environment variables, or configuration files.
    /// </remarks>
    internal interface ICredentialProvider : ISupportProvider
    {
    }
}
