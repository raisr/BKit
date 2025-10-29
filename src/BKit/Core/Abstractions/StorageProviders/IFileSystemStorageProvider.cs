namespace BKit.Core.Abstractions.StorageProviders
{
    /// <summary>
    /// Defines an interface for storage providers that interact with a file system.
    /// </summary>
    /// <remarks>Implementations of this interface provide file system-based storage operations. This
    /// interface extends the functionality of IStorageProvider to support scenarios where storage is backed by a file
    /// system.</remarks>
    internal interface IFileSystemStorageProvider : IStorageProvider
    {
    }
}
