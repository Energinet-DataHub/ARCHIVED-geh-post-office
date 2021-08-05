using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Domain.Repositories
{
    /// <summary>
    /// Repository for DataAvailable aggregates.
    /// </summary>
    public interface IDataAvailableRepository
    {
        /// <summary>
        /// Get UUIDs from Cosmos database
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns>A collection with all UUIDs for the specified recipient</returns>
        public Task<RequestData> GetDataAvailableUuidsAsync(string recipient);

        /// <summary>
        /// Save a document.
        /// </summary>
        /// <param name="document">The document to save.</param>
        Task<bool> SaveDocumentAsync(DataAvailable document);
    }
}
