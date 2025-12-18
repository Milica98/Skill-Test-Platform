using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SkillTestPlatform.Services;

namespace SkillTestPlatform.Tests.Integration
{
    [TestClass]
    public class BlobStorageServiceIntegrationTests
    {
        private BlobServiceClient _serviceClient = null!;
        private BlobStorageService<TestEntity> _storageService = null!;
        private ILogger<BlobStorageService<TestEntity>> _logger = null!;
        [TestInitialize]
        public void Setup()
        {
            string connectionString = "UseDevelopmentStorage=true"; // azurite
            _serviceClient = new BlobServiceClient(connectionString);
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BlobStorageService<TestEntity>>();
            _storageService = new BlobStorageService<TestEntity>(_serviceClient, "test-entities", _logger);
        }

        [TestMethod]
        public async Task AddAsync_ThenGetById_ReturnsEntity()
        {
            var entity = new TestEntity { Name = "Name1" };

            await _storageService.AddAsync(entity, entity.Id);
            var retrieved = await _storageService.GetAsync(entity.Id);

            Assert.IsNotNull(retrieved);
            Assert.AreEqual(entity.Id, retrieved!.Id);
            Assert.AreEqual(entity.Name, retrieved.Name);
        }

        [TestMethod]
        public async Task GetById_WhenBlobDoesNotExist_ReturnsNull()
        {
            var retrieved = await _storageService.GetAsync("not-existing");
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public async Task GetAll_ReturnsMultipleEntities()
        {
            var entity1 = new TestEntity { Name = "One" };
            var entity2 = new TestEntity { Name = "Two" };

            await _storageService.AddAsync(entity1, entity1.Id);
            await _storageService.AddAsync(entity2, entity2.Id);

            var all = await _storageService.GetAsync();

            Assert.IsGreaterThanOrEqualTo(2, all.Count);
        }

        [TestMethod]
        public async Task UpdateAsync_OverwritesBlob()
        {
            var entity = new TestEntity { Name = "OldName" };
            await _storageService.AddAsync(entity, entity.Id);

            entity.Name = "NewName";
            await _storageService.UpdateAsync(entity, entity.Id);

            var retrieved = await _storageService.GetAsync(entity.Id);
            Assert.AreEqual("NewName", retrieved!.Name);
        }

        [TestMethod]
        public async Task DeleteAsync_RemovesBlob()
        {
            var entity = new TestEntity { Name = "Name1" };
            await _storageService.AddAsync(entity, entity.Id);

            var deleted = await _storageService.DeleteAsync(entity.Id);
            Assert.IsTrue(deleted);

            var retrieved = await _storageService.GetAsync(entity.Id);
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBlobNotExists_ReturnsFalse()
        {
            var deleted = await _storageService.DeleteAsync("someId");
            Assert.IsFalse(deleted);
        }

        [TestMethod]
        public async Task AddAsync_SetsAccessTierToCool()
        {
            var entity = new TestEntity { Name = "Name1" };
            await _storageService.AddAsync(entity, entity.Id);

            var blobClient = new BlobContainerClient("UseDevelopmentStorage=true", "test-entities")
                                .GetBlobClient($"{entity.Id}.json");
            var properties = await blobClient.GetPropertiesAsync();

            Assert.AreEqual(AccessTier.Cool.ToString(), properties.Value.AccessTier);
        }
    }

    public class TestEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
    }
}