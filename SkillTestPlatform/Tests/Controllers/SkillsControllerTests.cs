using Microsoft.AspNetCore.Mvc;
using Moq;
using SkillTestPlatform.Controllers;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Tests.Controllers
{
    [TestClass]
    public class SkillsControllerTests
    {
        private SkillsController _controller = null!;

        private Mock<IStorageService<Skill>> _mockStorageService = null!;
        private Mock<ILogger<SkillsController>> _mockLogger = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockStorageService = new Mock<IStorageService<Skill>>();
            _mockLogger = new Mock<ILogger<SkillsController>>();
            _controller = new SkillsController(_mockStorageService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task List_ReturnsOkWithSkills()
        {
            var skills = new List<Skill>
            {
                new() { Name = ".NET" },
                new() { Name = "Python" }
            };
            _mockStorageService.Setup(s => s.GetAsync()).ReturnsAsync(skills);

            var result = await _controller.Get();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedSkills = okResult.Value as IEnumerable<Skill>;
            Assert.IsNotNull(returnedSkills);
            Assert.HasCount(2, returnedSkills.ToList());
        }

        [TestMethod]
        public async Task Get_ById_WhenSkillExists_ReturnsOk()
        {
            var skill = new Skill { Name = "Java" };
            _mockStorageService.Setup(s => s.GetAsync(skill.Id)).ReturnsAsync(skill);

            var result = await _controller.Get(skill.Id);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(skill, okResult.Value);
        }

        [TestMethod]
        public async Task Get_ById_WhenSkillDoesNotExist_ReturnsNotFound()
        {
            _mockStorageService.Setup(s => s.GetAsync("99")).ReturnsAsync((Skill?)null);

            var result = await _controller.Get("99");

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Add_ReturnsCreatedAtAction()
        {
            var skill = new SkillCreateModel { Name = "Python" };
            _mockStorageService.Setup(s => s.AddAsync(It.IsAny<Skill>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Add(skill);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("Get", createdResult.ActionName);
            Assert.IsInstanceOfType(createdResult.Value, typeof(Skill));
        }

        [TestMethod]
        public async Task Update_ReturnsNoContent()
        {
            var skill = new Skill { Name = "React" };
            _mockStorageService.Setup(s => s.UpdateAsync(skill, skill.Id))
                        .Returns(Task.CompletedTask);

            var result = await _controller.Update(skill);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_WhenDeleted_ReturnsNoContent()
        {
            _mockStorageService.Setup(s => s.DeleteAsync("1")).ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_WhenNotDeleted_ReturnsNotFound()
        {
            _mockStorageService.Setup(s => s.DeleteAsync("99")).ReturnsAsync(false);

            var result = await _controller.Delete("99");

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}