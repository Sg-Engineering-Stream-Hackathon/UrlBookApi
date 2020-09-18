using DataService.Interfaces;
using ModelService;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using WebApiCatalog.Controllers.Api.v1;

namespace UrlBookApiTest
{
    public class ProfileControllerTest
    {
        private readonly ProfileController _controller;
        private readonly Mock<IUserSvc> _userServiceMock = new Mock<IUserSvc>();

        public ProfileControllerTest()
        {
            _controller = new ProfileController(_userServiceMock.Object);
        }

        [Test]
        public void Get_All_Users_Test()
        {
            var users = _controller.GetAllUsers();
            Assert.IsNotNull(users);
        }

        [Test]
        public void UpdateProfile_Test()
        {
            ApplicationUser user = new ApplicationUser
            {
                Firstname = "Purushottam",
                Lastname = "Kumar",
                UserRole = "Admin",
                isAdmin = true
            };

            _userServiceMock.Setup(x => x.UpdateProfileAsync(user));

            var userList = new List<ApplicationUser>() { user };

            var users = _controller.UpdateProfile(userList);
            Assert.That(users.IsCompletedSuccessfully);
        }
    }
}
