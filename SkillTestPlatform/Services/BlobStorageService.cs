using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SkillTestPlatform.Constants;
using SkillTestPlatform.Services.Interfaces;
using System.Text.Json;

namespace SkillTestPlatform.Services
{
    public class BlobStorageService<T> : IStorageService<T>
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<BlobStorageService<T>> _logger;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public BlobStorageService(BlobServiceClient serviceClient, string containerName, ILogger<BlobStorageService<T>> logger)
        {
            _container = serviceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists();
            _logger = logger;
        }

        public async Task AddAsync(T entity, string id)
        {
            try
            {
                var blob = _container.GetBlobClient(BlobSettings.BlobName(id));
                var bytes = JsonSerializer.SerializeToUtf8Bytes(entity, _jsonOptions);
                using var ms = new MemoryStream(bytes);
                await blob.UploadAsync(ms);
                await blob.SetAccessTierAsync(AccessTier.Cool);
                _logger.LogInformation("Blob {Id} added successfully", id);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Request failed while adding blob {Id}", id);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Serialization failed for blob {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding blob {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var blob = _container.GetBlobClient(BlobSettings.BlobName(id));
                var response = await blob.DeleteIfExistsAsync();
                if (!response.Value)
                    _logger.LogWarning("Blob {Id} not found for deletion", id);

                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Request failed while deleting blob {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting blob {Id}", id);
                throw;
            }
        }

        public async Task<T?> GetAsync(string id)
        {
            try
            {
                var blob = _container.GetBlobClient(BlobSettings.BlobName(id));
                if (!await blob.ExistsAsync())
                {
                    _logger.LogWarning("Blob {Id} does not exist", id);
                    return default;
                }

                var download = await blob.DownloadContentAsync();
                _logger.LogInformation("Blob {Id} fetched successfully", id);
                return download.Value.Content.ToObjectFromJson<T>(_jsonOptions);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Request failed while fetching blob {Id}", id);
                return default;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Deserialization failed for blob {Id}", id);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching blob {Id}", id);
                return default;
            }
        }

        public async Task<IReadOnlyList<T>> GetAsync()
        {
            try
            {
                var results = new List<T>();
                await foreach (var item in _container.GetBlobsAsync())
                {
                    var blob = _container.GetBlobClient(item.Name);
                    var content = await blob.DownloadContentAsync();
                    var obj = content.Value.Content.ToObjectFromJson<T>(_jsonOptions);
                    if (obj is not null) results.Add(obj);
                }
                _logger.LogInformation("Fetched {Count} blobs successfully", results.Count);
                return results;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Request failed while fetching blobs");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Deserialization failed while fetching blobs");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching blobs");
                throw;
            }
        }

        public async Task UpdateAsync(T entity, string id)
        {
            try
            {
                var blob = _container.GetBlobClient(BlobSettings.BlobName(id));
                var bytes = JsonSerializer.SerializeToUtf8Bytes(entity, _jsonOptions);
                using var ms = new MemoryStream(bytes);
                await blob.UploadAsync(ms, overwrite: true);
                await blob.SetAccessTierAsync(AccessTier.Cool);
                _logger.LogInformation("Blob {Id} updated successfully", id);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Request failed while updating blob {Id}", id);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Serialization failed for blob {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating blob {Id}", id);
                throw;
            }
        }
    }
}
