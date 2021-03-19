using IAM.Areas.Authentication;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IAM.Api.UnitTest
{
    public class TwoFactorAuthenticationControllerTests
    {
        private TwoFactorAuthenticationController _controller;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;

        public TwoFactorAuthenticationControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

           _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object,
                contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            var mockLogger = new Mock<ILogger<ExternalLoginModel>>();

            //Mock controller and http context
            _controller = new TwoFactorAuthenticationController(_mockSignInManager.Object, _mockUserManager.Object, mockLogger.Object);
            _controller.ControllerContext = new ControllerContext();
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Request.Headers["device-id"] = "123";
        }

        [Fact]
        public async Task TwoFactorAuthenticationWhenUserIsNull()
        {
            var rememberMe = true;
            var twoFactorCode = "code1";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);

            // execute method of controller
            var resultType = await _controller.TwoFactorAuthenticationUser(model);

            // Check that the result is as expected
            Assert.IsType<NotFoundObjectResult>(resultType);
        }

        [Fact]
        public async Task TwoFactorAuthenticationWhenUserIsNotUseTwoFactor()
        {
            var rememberMe = true;
            var twoFactorCode = "";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser();

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));

            // execute method of controller
            var resultType = await _controller.TwoFactorAuthenticationUser(model);

            // Check that the result is as expected
            Assert.IsType<BadRequestObjectResult>(resultType);
        }

        [Fact]
        public async Task TwoFactorAuthenticationSignInFiled()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser() { TwoFactorEnabled = true };
            var result = new Microsoft.AspNetCore.Identity.SignInResult();

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.GetTwoFactorAuthenticationUserAsync()).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));
            
            // execute method of controller
            var resultType = await _controller.TwoFactorAuthenticationUser(model);

            // Check that the result is as expected
            Assert.IsType<BadRequestObjectResult>(resultType);
        }

        [Fact]
        public async Task TwoFactorAuthenticationSignInSucceeded()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser() { TwoFactorEnabled = true };

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.GetTwoFactorAuthenticationUserAsync()).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            // execute method of controller
            var resultType = await _controller.TwoFactorAuthenticationUser(model);

            // Check that the result is as expected
            Assert.IsType<OkObjectResult>(resultType);
        }

        [Fact]
        public async Task TwoFactorAuthenticationSignInIsLockedOut()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser() { TwoFactorEnabled = true };

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.GetTwoFactorAuthenticationUserAsync()).Returns(Task.FromResult(user));
            _mockSignInManager.Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.LockedOut));

            // execute method of controller
            var resultType = await _controller.TwoFactorAuthenticationUser(model);

            // Check that the result is as expected
            Assert.IsType<BadRequestObjectResult>(resultType);
        }

        [Fact]
        public async Task EnableAuthenticatorUserIsNull()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);

            // execute method of controller
            var resultType = await _controller.EnableAuthenticator(model);

            // Check that the result is as expected
            Assert.IsType<NotFoundObjectResult>(resultType);
        }

        [Fact]
        public async Task EnableAuthenticatorInvalidCode()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser() { TwoFactorEnabled = true };

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));

            // execute method of controller
            var resultType = await _controller.EnableAuthenticator(model);

            // Check that the result is as expected
            Assert.IsType<BadRequestObjectResult>(resultType);
        }

        [Fact]
        public async Task EnableAuthenticatorOK()
        {
            var rememberMe = true;
            var twoFactorCode = "code2";
            var isPersistent = true;
            var model = GetTwoFactorAuthenticationModel(twoFactorCode, isPersistent, rememberMe);
            var user = new ApplicationUser() { TwoFactorEnabled = true };
            var codes = new List<string>();

            codes.Add("123");
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
            _mockUserManager.Setup(u => u.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _mockUserManager.Setup(u => u.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<ApplicationUser>(), It.IsAny<int>())).Returns(Task.FromResult(codes as IEnumerable<string>));

            // execute method of controller
            var resultType = await _controller.EnableAuthenticator(model);


            // Check that the result is as expected
            var result = Assert.IsType<OkObjectResult>(resultType);
            var codesFromController = Assert.IsType<List<string>>(result.Value);
            Assert.Equal(codesFromController.Count, codes.Count);
        }

        private TwoFactorAuthenticationModel GetTwoFactorAuthenticationModel(string twoFactorCode, bool isPersistent, bool rememberMe)
        {
            return new TwoFactorAuthenticationModel() {
                Code = twoFactorCode,
                IsPersistent = isPersistent,
                RememberMe = rememberMe
            };
        }

    }
}
