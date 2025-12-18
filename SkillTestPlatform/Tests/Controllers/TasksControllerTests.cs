using Microsoft.AspNetCore.Mvc;
using Moq;
using SkillTestPlatform.Constants;
using SkillTestPlatform.Controllers;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Tests.Controllers
{
    [TestClass]
    public class TasksControllerTests
    {
        private TasksController _controller = null!;

        private Mock<IStorageService<TaskItem>> _mockStorageService = null!;
        private Mock<ILogger<TasksController>> _mockLogger = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockStorageService = new Mock<IStorageService<TaskItem>>();
            _mockLogger = new Mock<ILogger<TasksController>>();
            _controller = new TasksController(_mockStorageService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task List_ReturnsOkWithTasks()
        {
            var tasks = new List<TaskItem>
            {
                GetTask("Title 1", Seniority.Senior, "Description 1", "Code1" ),
                GetTask("Title 2", Seniority.Junior, "Description 2", "Code2" )
            };
            _mockStorageService.Setup(r => r.GetAsync()).ReturnsAsync(tasks);

            var result = await _controller.Get();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedTasks = okResult.Value as IEnumerable<TaskItem>;
            Assert.IsNotNull(returnedTasks);
            Assert.HasCount(2, returnedTasks.ToList());
        }

        [TestMethod]
        public async Task Get_ById_WhenTaskExists_ReturnsOk()
        {
            var task = GetTask("Title 3", Seniority.Mid, "Description 3", "Code3");

            _mockStorageService.Setup(r => r.GetAsync(task.Id)).ReturnsAsync(task);

            var result = await _controller.Get(task.Id);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(task, okResult.Value);
        }

        [TestMethod]
        public async Task Get_ById_WhenTaskDoesNotExist_ReturnsNotFound()
        {
            _mockStorageService.Setup(r => r.GetAsync("99")).ReturnsAsync((TaskItem?)null);

            var result = await _controller.Get("99");

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Add_ReturnsCreatedAtAction()
        {
            var task = GetTaskCreateModel("Title 5", Seniority.Senior, "Description 5", "Code5");

            _mockStorageService.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.Add(task);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("Get", createdResult.ActionName);
            Assert.IsInstanceOfType(createdResult.Value, typeof(TaskItem));
        }

        [TestMethod]
        public async Task Update_ReturnsNoContent()
        {
            var task = GetTask("Title 6", Seniority.Mid, "Description 6", "Code6");

            _mockStorageService.Setup(r => r.UpdateAsync(task, task.Id))
                     .Returns(Task.CompletedTask);

            var result = await _controller.Update(task);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_WhenDeleted_ReturnsNoContent()
        {
            _mockStorageService.Setup(r => r.DeleteAsync("1")).ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_WhenNotDeleted_ReturnsNotFound()
        {
            _mockStorageService.Setup(r => r.DeleteAsync("99")).ReturnsAsync(false);

            var result = await _controller.Delete("99");

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        private static TaskItem GetTask(string title, string level, string description, string code)
        {
            return new TaskItem
            {
                Title = title,
                Level = level,
                Description = description,
                Code = code
            };
        }

        private static TaskItemCreateModel GetTaskCreateModel(string title, string level, string description, string code)
        {
            return new TaskItemCreateModel
            {
                Title = title,
                Level = level,
                Description = description,
                Code = code
            };
        }
    }
}