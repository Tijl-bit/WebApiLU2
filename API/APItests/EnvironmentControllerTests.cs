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

        [TestInitialize]
        public void Setup()
        {
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _environment2DRepositoryMock = new Mock<IEnvironment2DRepository>();
            _controller = new EnvironmentController(_authenticationServiceMock.Object, _environment2DRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsOk_WithList()
        {
            var userId = "user123";
            var environments = new List<Environment2D>
            {
                new Environment2D { Id = Guid.NewGuid(), Name = "Env1", OwnerUserId = userId },
                new Environment2D { Id = Guid.NewGuid(), Name = "Env2", OwnerUserId = userId }
            };

            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(environments);

            var result = await _controller.GetEnvironments();
            var okResult = result.Result as OkObjectResult;

            Assert.IsNotNull(okResult);
            var returnedEnvs = okResult.Value as List<Environment2D>;
            Assert.IsNotNull(returnedEnvs);
            Assert.AreEqual(2, returnedEnvs.Count);
        }

        [TestMethod]
        public async Task GetEnvironment_ReturnsNotFound_WhenMissing()
        {
            var envId = Guid.NewGuid();
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync((Environment2D)null);

            var result = await _controller.GetEnvironment(envId);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }
        // fail
        [TestMethod]
        public async Task CreateEnvironment_ReturnsCreatedAtAction_WhenCreated()
        {
            var userId = "user123";
            var newEnv = new PostEnvironment2D { Name = "NewEnv", MaxHeight = 5, MaxLength = 5 };
            var expectedId = Guid.NewGuid();

            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<Environment2D>())).ReturnsAsync(expectedId);

            var result = await _controller.CreateEnvironment(newEnv);

            var created = result.Result as CreatedAtActionResult;  // FIX: Correct type
            Assert.IsNotNull(created);
            Assert.AreEqual(nameof(_controller.GetEnvironment), created.ActionName);
            Assert.IsInstanceOfType(created.Value, typeof(Guid));
            Assert.AreEqual(expectedId, created.Value);
        }



        [TestMethod]
        public async Task UpdateEnvironment2D_ReturnsUnauthorized_WhenUserNotOwner()
        {
            var envId = Guid.NewGuid();
            var existing = new Environment2D { Id = envId, OwnerUserId = "otherUser" };

            // Note: here you create an Environment2D instance similar to your update model
            var updateModel = new Environment2D { Id = envId, Name = "UpdatedEnv", MaxHeight = 6, MaxLength = 6 };

            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(existing);

            var result = await _controller.UpdateEnvironment2D(updateModel);

            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("You are not allowed to update this environment.", unauthorized.Value);
        }


        [TestMethod]
        public async Task UpdateEnvironment_ReturnsOk_WhenSuccessful()
        {
            var envId = Guid.NewGuid();
            var userId = "user123";

            var updateModel = new Environment2D
            {
                Id = envId,
                OwnerUserId = userId,
                Name = "UpdatedEnv",
                MaxHeight = 6,
                MaxLength = 6
            };

            var existing = new Environment2D
            {
                Id = envId,
                OwnerUserId = userId,
                Name = "OldEnv",
                MaxHeight = 5,
                MaxLength = 5
            };

            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(existing);
            _environment2DRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Environment2D>())).ReturnsAsync(true);

            var result = await _controller.UpdateEnvironment2D(updateModel);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult)); // or OkResult if your controller returns that
        }


        [TestMethod]
        public async Task DeleteEnvironment_ReturnsUnauthorized_WhenUserNotOwner()
        {
            var envId = Guid.NewGuid();
            var env = new Environment2D { Id = envId, OwnerUserId = "otherUser" };

            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(env);
            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");

            var result = await _controller.DeleteEnvironment(envId);

            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("You are not allowed to delete this environment.", unauthorized.Value);
        }

        [TestMethod]
        public async Task DeleteEnvironment_ReturnsNotFound_WhenMissing()
        {
            var envId = Guid.NewGuid();
            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync((Environment2D)null);

            var result = await _controller.DeleteEnvironment(envId);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        [TestMethod]
        public async Task DeleteEnvironment_ReturnsNoContent_WhenSuccessful()
        {
            var envId = Guid.NewGuid();
            var userId = "user123";
            var env = new Environment2D { Id = envId, OwnerUserId = userId };

            _environment2DRepositoryMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(env);
            _authenticationServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _environment2DRepositoryMock.Setup(r => r.DeleteAsync(envId)).ReturnsAsync(true);

            var result = await _controller.DeleteEnvironment(envId);

            Assert.IsInstanceOfType(result, typeof(NoContentResult)); // FIXED
        }

    }
}
