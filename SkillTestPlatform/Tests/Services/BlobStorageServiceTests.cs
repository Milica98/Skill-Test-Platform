using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using SkillTestPlatform.Constants;
using SkillTestPlatform.Services;

namespace SkillTestPlatform.Tests.Services
{
    [TestClass]
    public class BlobStorageServiceTests
    {
        private Mock<BlobContainerClient> _mockBlobClientContainerClient = null!;
        private Mock<BlobClient> _mockBlobClient = null!;
        private Mock<ILogger<BlobStorageService<TestEntity>>> _mockLogger = null!;

        private BlobStorageService<TestEntity> _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockBlobClientContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();
            _mockLogger = new Mock<ILogger<BlobStorageService<TestEntity>>>();

            _mockBlobClientContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>()))
                          .Returns(_mockBlobClient.Object);

            var mockServiceClient = new Mock<BlobServiceClient>();
            mockServiceClient.Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
                             .Returns(_mockBlobClientContainerClient.Object);

            _service = new BlobStorageService<TestEntity>(mockServiceClient.Object, "test-container", _mockLogger.Object);
        }

        [TestMethod]
        public async Task AddAsync_UploadsBlob()
        {
            var entity = new TestEntity { Name = "Name" };

            await _service.AddAsync(entity, entity.Id);

            _mockBlobClientContainerClient.Verify(b => b.GetBlobClient(BlobSettings.BlobName(entity.Id)), Times.Once);
            _mockBlobClient.Verify(b => b.UploadAsync(It.IsAny<Stream>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBlobDeleted_ReturnsTrue()
        {
            _mockBlobClient.Setup(b => b.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None, null, default))
                .ReturnsAsync(Response.FromValue(true, null!));

            var result = await _service.DeleteAsync("1");

            _mockBlobClientContainerClient.Verify(b => b.GetBlobClient(BlobSettings.BlobName("1")), Times.Once);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBlobNotFound_ReturnsFalse()
        {
            _mockBlobClient.Setup(b => b.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None, null, default))
                .ReturnsAsync(Response.FromValue(false, null!));

            var result = await _service.DeleteAsync("99");

            _mockBlobClientContainerClient.Verify(b => b.GetBlobClient(BlobSettings.BlobName("99")), Times.Once);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAsync_WhenBlobDoesNotExist_ReturnsNull()
        {
            _mockBlobClient.Setup(b => b.ExistsAsync(default))
                     .ReturnsAsync(Response.FromValue(false, null!));

            var result = await _service.GetAsync("10");

            _mockBlobClientContainerClient.Verify(b => b.GetBlobClient(BlobSettings.BlobName("10")), Times.Once);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_OverwritesBlob()
        {
            var entity = new TestEntity { Name = "Name2" };
            _mockBlobClient.Setup(b => b.UploadAsync(It.IsAny<Stream>(), true, default))
                     .ReturnsAsync(Mock.Of<Azure.Response<BlobContentInfo>>());

            await _service.UpdateAsync(entity, entity.Id);

            _mockBlobClientContainerClient.Verify(b => b.GetBlobClient(BlobSettings.BlobName(entity.Id)), Times.Once);
            _mockBlobClient.Verify(b => b.UploadAsync(It.IsAny<Stream>(), true, default), Times.Once);
        }
    }

    public class TestEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
    }
}