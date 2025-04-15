using API.Controllers;
using API.Models;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectTests
{
    [TestClass]
    public class ObjectControllerTests
    {
        private readonly Mock<IObject2DRepository> _mockObjectRepo;
        private readonly Mock<IEnvironment2DRepository> _mockEnvRepo;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly ObjectController _controller;

        public ObjectControllerTests()
        {
            _mockObjectRepo = new Mock<IObject2DRepository>();
            _mockEnvRepo = new Mock<IEnvironment2DRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();

            _controller = new ObjectController(
                _mockObjectRepo.Object,
                _mockEnvRepo.Object,
                _mockAuthService.Object
            );
        }

        [TestMethod]
        public async Task GetObjects_ReturnsOkResult_WithObjectList()
        {
            // Arrange
            var objects = new List<Object2D> {
                new Object2D { Id = Guid.NewGuid(), EnvironmentId = Guid.NewGuid(), PrefabId = "Prefab1", PositionX = 1, PositionY = 2, ScaleX = 1, ScaleY = 1, RotationZ = 0, SortingLayer = 1 },
                new Object2D { Id = Guid.NewGuid(), EnvironmentId = Guid.NewGuid(), PrefabId = "Prefab2", PositionX = 3, PositionY = 4, ScaleX = 1, ScaleY = 1, RotationZ = 90, SortingLayer = 2 }
            };

            _mockObjectRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(objects);

            // Act
            var result = await _controller.GetObjects();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnValue = okResult.Value as IEnumerable<Object2D>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, ((List<Object2D>)returnValue).Count);
        }

        [TestMethod]
        public async Task GetObject_ReturnsNotFound_WhenObjectDoesNotExist()
        {
            // Arrange
            var objectId = Guid.NewGuid();
            _mockObjectRepo.Setup(repo => repo.GetByIdAsync(objectId)).ReturnsAsync((Object2D)null);

            // Act
            var result = await _controller.GetObject(objectId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task AddObject_ReturnsOkResult_WhenObjectIsAddedSuccessfully()
        {
            // Arrange
            var postObject = new PostObject2D
            {
                EnvironmentId = Guid.NewGuid().ToString(),
                PrefabId = "Prefab1",
                PositionX = 1,
                PositionY = 2,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 1
            };

            var object2D = new Object2D
            {
                Id = Guid.NewGuid(),
                EnvironmentId = Guid.Parse(postObject.EnvironmentId),
                PrefabId = postObject.PrefabId,
                PositionX = postObject.PositionX,
                PositionY = postObject.PositionY,
                ScaleX = postObject.ScaleX,
                ScaleY = postObject.ScaleY,
                RotationZ = postObject.RotationZ,
                SortingLayer = postObject.SortingLayer
            };

            _mockEnvRepo.Setup(repo => repo.GetByIdAsync(object2D.EnvironmentId)).ReturnsAsync(new Environment2D { OwnerUserId = "user123" });
            _mockAuthService.Setup(service => service.GetCurrentAuthenticatedUserId()).Returns("user123");

            _mockObjectRepo.Setup(repo => repo.InsertAsync(It.IsAny<Object2D>())).ReturnsAsync(object2D.Id);

            // Act
            var result = await _controller.AddObject(postObject);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(postObject, okResult.Value);
        }

        [TestMethod]
        public async Task AddObject_ReturnsUnauthorized_WhenUserDoesNotOwnEnvironment()
        {
            // Arrange
            var postObject = new PostObject2D
            {
                EnvironmentId = Guid.NewGuid().ToString(),
                PrefabId = "Prefab1",
                PositionX = 1,
                PositionY = 2,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 1
            };

            var object2D = new Object2D
            {
                Id = Guid.NewGuid(),
                EnvironmentId = Guid.Parse(postObject.EnvironmentId),
                PrefabId = postObject.PrefabId,
                PositionX = postObject.PositionX,
                PositionY = postObject.PositionY,
                ScaleX = postObject.ScaleX,
                ScaleY = postObject.ScaleY,
                RotationZ = postObject.RotationZ,
                SortingLayer = postObject.SortingLayer
            };

            _mockEnvRepo.Setup(repo => repo.GetByIdAsync(object2D.EnvironmentId)).ReturnsAsync(new Environment2D { OwnerUserId = "user123" });
            _mockAuthService.Setup(service => service.GetCurrentAuthenticatedUserId()).Returns("user456");

            // Act
            var result = await _controller.AddObject(postObject);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual("You do not own this environment", unauthorizedResult.Value);
        }

        // Additional test cases can follow for other actions (PUT, DELETE, etc.)
    }
}
