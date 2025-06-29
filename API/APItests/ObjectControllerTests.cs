using API.Controllers;
using API.Models;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectTests
{
    [TestClass]
    public class ObjectControllerTests
    {
        private Mock<IObject2DRepository> _objectRepoMock;
        private Mock<IEnvironment2DRepository> _envRepoMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private ObjectController _controller;

        [TestInitialize]
        public void Setup()
        {
            _objectRepoMock = new Mock<IObject2DRepository>();
            _envRepoMock = new Mock<IEnvironment2DRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new ObjectController(_objectRepoMock.Object, _envRepoMock.Object, _authServiceMock.Object);
        }

        [TestMethod]
        public async Task GetObjects_ReturnsUserOwnedObjects()
        {
            var userId = "user123";
            var envs = new List<Environment2D>
            {
                new Environment2D { Id = Guid.NewGuid(), OwnerUserId = userId }
            };
            var objs = new List<Object2D>
            {
                new Object2D { Id = Guid.NewGuid(), EnvironmentId = envs[0].Id }
            };

            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns(userId);
            _envRepoMock.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(envs);
            _objectRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(objs);

            var result = await _controller.GetObjects();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var data = okResult.Value as IEnumerable<Object2D>;
            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public async Task GetObject_ReturnsUnauthorized_IfUserNotOwner()
        {
            var objId = Guid.NewGuid();
            var envId = Guid.NewGuid();

            _objectRepoMock.Setup(r => r.GetByIdAsync(objId)).ReturnsAsync(new Object2D { Id = objId, EnvironmentId = envId });
            _envRepoMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(new Environment2D { Id = envId, OwnerUserId = "someoneElse" });
            _authServiceMock.Setup(a => a.GetCurrentAuthenticatedUserId()).Returns("user123");

            var result = await _controller.GetObject(objId);

            Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task UpdateObject_ReturnsNoContent_IfSuccessful()
        {
            var obj = new Object2D
            {
                Id = Guid.NewGuid(),
                EnvironmentId = Guid.NewGuid()
            };

            _envRepoMock.Setup(r => r.GetByIdAsync(obj.EnvironmentId)).ReturnsAsync(new Environment2D { Id = obj.EnvironmentId, OwnerUserId = "user123" });
            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");
            _objectRepoMock.Setup(r => r.UpdateAsync(obj)).ReturnsAsync(true);

            var result = await _controller.UpdateObject(obj.Id, obj);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task DeleteObject_ReturnsNotFound_IfObjectNotFound()
        {
            _objectRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Object2D)null);

            var result = await _controller.DeleteObject(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetByEnvironment_ReturnsUnauthorized_IfUserDoesNotOwnEnvironment()
        {
            var envId = Guid.NewGuid();
            _envRepoMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(new Environment2D { Id = envId, OwnerUserId = "otherUser" });
            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");

            var result = await _controller.GetByEnvironment(envId);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task GetByEnvironment_ReturnsObjects_IfAuthorized()
        {
            var envId = Guid.NewGuid();
            var objects = new List<Object2D> { new Object2D { Id = Guid.NewGuid(), EnvironmentId = envId } };

            _envRepoMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(new Environment2D { Id = envId, OwnerUserId = "user123" });
            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");
            _objectRepoMock.Setup(r => r.GetByEnvironmentIdAsync(envId)).ReturnsAsync(objects);

            var result = await _controller.GetByEnvironment(envId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var data = okResult.Value as IEnumerable<Object2D>;
            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public async Task UpdateObject_ReturnsBadRequest_WhenIdMismatch()
        {
            var obj = new Object2D { Id = Guid.NewGuid(), EnvironmentId = Guid.NewGuid() };

            var result = await _controller.UpdateObject(Guid.NewGuid(), obj);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UpdateObject_ReturnsUnauthorized_WhenUserNotOwner()
        {
            var obj = new Object2D { Id = Guid.NewGuid(), EnvironmentId = Guid.NewGuid() };

            _envRepoMock.Setup(r => r.GetByIdAsync(obj.EnvironmentId)).ReturnsAsync(new Environment2D { Id = obj.EnvironmentId, OwnerUserId = "otherUser" });
            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");

            var result = await _controller.UpdateObject(obj.Id, obj);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task DeleteObject_ReturnsNoContent_WhenSuccessful()
        {
            var objId = Guid.NewGuid();
            var envId = Guid.NewGuid();

            _objectRepoMock.Setup(r => r.GetByIdAsync(objId)).ReturnsAsync(new Object2D { Id = objId, EnvironmentId = envId });
            _envRepoMock.Setup(r => r.GetByIdAsync(envId)).ReturnsAsync(new Environment2D { Id = envId, OwnerUserId = "user123" });
            _authServiceMock.Setup(s => s.GetCurrentAuthenticatedUserId()).Returns("user123");
            _objectRepoMock.Setup(r => r.DeleteAsync(objId)).ReturnsAsync(true);

            var result = await _controller.DeleteObject(objId);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
    }
}
