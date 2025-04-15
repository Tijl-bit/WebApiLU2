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

namespace EnvironmentTests
{
    [TestClass]
    public class EnvironmentControllerTests
    {
        private Mock<IAuthenticationService> _authenticationServiceMock;
        private Mock<IEnvironment2DRepository> _environment2DRepositoryMock;
        private EnvironmentController _controller;

        public EnvironmentControllerTests()
        {
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _environment2DRepositoryMock = new Mock<IEnvironment2DRepository>();
            _controller = new EnvironmentController(_authenticationServiceMock.Object, _environment2DRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsOkResult_WithListOfEnvironments()
        {
            // Arrange
            var userId = "user123";
            var environments = new List<Environment2D>
            {
                new Environment2D { Id = Guid.NewGuid(), Name = "Env1", OwnerUserId = userId, MaxLength = 10, MaxHeight = 10 },
                new Environment2D { Id = Guid.NewGuid(), Name = "Env2", OwnerUserId = userId, MaxLength = 10, MaxHeight = 10 }
            };
            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(environments);

            // Act
            var result = await _controller.GetEnvironments();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnValue = okResult.Value as List<Environment2D>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count);
        }

        [TestMethod]
        public async Task GetEnvironment_ReturnsNotFound_WhenEnvironmentDoesNotExist()
        {
            // Arrange
            var envId = Guid.NewGuid();
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync((Environment2D)null);

            // Act
            var result = await _controller.GetEnvironment(envId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DeleteEnvironment_ReturnsUnauthorized_WhenUserIsNotOwner()
        {
            // Arrange
            var envId = Guid.NewGuid();
            var env = new Environment2D { Id = envId, OwnerUserId = "otherUser" };
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(env);
            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");

            // Act
            var result = await _controller.DeleteEnvironment(envId);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual("You are not allowed to delete this environment.", unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task CreateEnvironment_ReturnsBadRequest_WhenEnvironmentNameExists()
        {
            // Arrange
            var postEnvironment = new PostEnvironment2D { Name = "ExistingEnv", MaxLength = 10, MaxHeight = 10 };
            var userId = "user123";
            var existingEnvironments = new List<Environment2D>
            {
                new Environment2D { Id = Guid.NewGuid(), Name = "ExistingEnv", OwnerUserId = userId, MaxLength = 10, MaxHeight = 10 }
            };
            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(existingEnvironments);

            // Act
            var result = await _controller.CreateEnvironment(postEnvironment);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("You already have a world with this name.", badRequestResult.Value);
        }

        // Add more tests as needed (e.g., for max environments, valid creation, etc.)
    }
}
