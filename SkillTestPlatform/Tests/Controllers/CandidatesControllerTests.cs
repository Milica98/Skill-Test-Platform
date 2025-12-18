using Microsoft.AspNetCore.Mvc;
using Moq;
using SkillTestPlatform.Constants;
using SkillTestPlatform.Controllers;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Tests.Controllers
{
    [TestClass]
    public class CandidatesControllerTests
    {
        private CandidatesController _controller = null!;

        private Mock<IStorageService<Candidate>> _mockStorageService = null!;
        private Mock<ILogger<CandidatesController>> _mockLogger = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockStorageService = new Mock<IStorageService<Candidate>>();
            _mockLogger = new Mock<ILogger<CandidatesController>>();
            _controller = new CandidatesController(_mockStorageService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task Get_ReturnsOkWithCandidates()
        {
            var candidates = new List<Candidate>
            {
                GetCandidate("Name1", "Surname1", "1234", Seniority.Senior),
                GetCandidate("Name2", "Surname2", "5678", Seniority.Mid)
            };
            _mockStorageService.Setup(s => s.GetAsync()).ReturnsAsync(candidates);

            var result = await _controller.Get();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedCandidates = okResult.Value as IEnumerable<Candidate>;
            Assert.IsNotNull(returnedCandidates);
            Assert.HasCount(2, returnedCandidates.ToList());
        }

        [TestMethod]
        public async Task Get_ById_WhenCandidateExists_ReturnsOk()
        {
            var candidate = GetCandidate("Name2", "Surname2", "5678", Seniority.Mid);
            _mockStorageService.Setup(s => s.GetAsync(candidate.Id)).ReturnsAsync(candidate);

            var result = await _controller.Get(candidate.Id);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(candidate, okResult.Value);
        }

        [TestMethod]
        public async Task Get_ById_WhenCandidateDoesNotExist_ReturnsNotFound()
        {
            _mockStorageService.Setup(s => s.GetAsync("99")).ReturnsAsync((Candidate?)null);

            var result = await _controller.Get("99");

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var candidate = GetCandidateCreateModel("Name3", "Surname3", "12345", Seniority.Junior);
            _mockStorageService.Setup(s => s.AddAsync(It.IsAny<Candidate>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Create(candidate);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("Get", createdResult.ActionName);
            Assert.IsInstanceOfType(createdResult.Value, typeof(Candidate));
        }

        [TestMethod]
        public async Task Update_ReturnsNoContent()
        {
            var candidate = GetCandidate("Name4", "Surname4", "4321", Seniority.Mid);
            _mockStorageService.Setup(s => s.UpdateAsync(candidate, candidate.Id))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Update(candidate);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _mockStorageService.Setup(s => s.DeleteAsync("1")).ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenNotDeleted()
        {
            _mockStorageService.Setup(s => s.DeleteAsync("99")).ReturnsAsync(false);

            var result = await _controller.Delete("99");

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        private static Candidate GetCandidate(string name, string surname, string phone, string seniority)
        {
            return new Candidate
            {
                Name = name,
                Surname = surname,
                PhoneNumber = phone,
                Seniority = seniority
            };
        }

        private static CandidateCreateModel GetCandidateCreateModel(string name, string surname, string phone, string seniority)
        {
            return new CandidateCreateModel
            {
                Name = name,
                Surname = surname,
                PhoneNumber = phone,
                Seniority = seniority
            };
        }
    }
}