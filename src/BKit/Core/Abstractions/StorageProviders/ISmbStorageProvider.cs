namespace BKit.Core.Abstractions.StorageProviders
{
    /// <summary>
    /// Defines the contract for a storage provider that interacts with SMB (Server Message Block) file shares.
    /// </summary>
    /// <remarks>Implementations of this interface provide methods for accessing and managing files and
    /// directories on SMB network shares. This interface extends the general storage provider contract to support
    /// SMB-specific operations.</remarks>
    internal interface ISmbStorageProvider : IStorageProvider
    {
    }
}
