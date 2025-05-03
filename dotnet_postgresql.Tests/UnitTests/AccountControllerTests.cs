using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AccountControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userMgr;
    private readonly Mock<SignInManager<IdentityUser>> _signInMgr;
    private readonly Mock<TokenService> _tokenSvc;
    private readonly Mock<EmailService> _emailSvc;
    private readonly Mock<IdentityContext> _context;
    private readonly Mock<ILogger<AccountController>> _logger;
    private readonly Mock<IMapper> _mapper;

    public AccountControllerTests()
    {
        // UserManager mock
        var userStore = new Mock<IUserStore<IdentityUser>>();
        _userMgr = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // SignInManager mock
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInMgr = new Mock<SignInManager<IdentityUser>>(
            _userMgr.Object, httpContextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

        _tokenSvc = new Mock<TokenService>(MockBehavior.Strict, null as ILogger<TokenService>, null as IMapper);
        _emailSvc = new Mock<EmailService>(MockBehavior.Strict, null as ILogger<EmailService>, null as IMapper);
        _context = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
        _logger = new Mock<ILogger<AccountController>>();
        _mapper = new Mock<IMapper>();
    }

    private AccountController CreateControllerWithUser(string userId)
    {
        var controller = new AccountController(
            _userMgr.Object,
            _signInMgr.Object,
            _context.Object,
            _tokenSvc.Object,
            _emailSvc.Object,
            _logger.Object,
            _mapper.Object
        );

        // Подготовка HttpContext и ClaimsPrincipal
        var claims = new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, userId) };
        var identity = new ClaimsIdentity(claims, "test");
        var userPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        return controller;
    }

    [Fact]
    public void SecretEndpoint_Admin_ReturnsOk()
    {
        // Arrange
        var ctrl = CreateControllerWithUser("admin-id");
        // Добавляем роль Admin в claims, чтобы пройти Authorize(Roles="Admin")
        ctrl.ControllerContext.HttpContext.User.AddIdentity(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));

        // Act
        var result = ctrl.SecretEndpoint();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("You are an admin, congratulations!", ok.Value);
    }

    [Fact]
    public async Task GetCurrentUser_NoClaims_ReturnsUnauthorized()
    {
        // Arrange
        var ctrl = CreateControllerWithUser(string.Empty);

        // Act
        var result = await ctrl.GetCurrentUser();

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task CheckEmailExistsAsync_EmailFound_ReturnsTrue()
    {
        // Arrange
        var testEmail = "test@example.com";
        _userMgr.Setup(u => u.FindByEmailAsync(testEmail))
                .ReturnsAsync(new IdentityUser { Email = testEmail });

        var ctrl = CreateControllerWithUser("any-id");

        // Act
        var actionResult = await ctrl.CheckEmailExistsAsync(testEmail);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(actionResult);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CheckEmailExistsAsync_EmailNotFound_ReturnsFalse()
    {
        // Arrange
        var testEmail = "noone@nowhere.com";
        _userMgr.Setup(u => u.FindByEmailAsync(testEmail))
                .ReturnsAsync((IdentityUser)null);

        var ctrl = CreateControllerWithUser("any-id");

        // Act
        var actionResult = await ctrl.CheckEmailExistsAsync(testEmail);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(actionResult);
        Assert.False(okResult.Value);
    }

    // Дополнительно: тест успешной регистрации (пример упрощённый)
    [Fact]
    public async Task Register_NewUser_ReturnsUserDto()
    {
        // Arrange
        var dto = new RegisterDto { Email = "a@b.com", UserName = "user1", Password = "Pwd123!" };
        _userMgr.Setup(u => u.Users).Returns(new List<IdentityUser>().AsQueryable());
        _userMgr.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
        _userMgr.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);
        _tokenSvc.Setup(t => t.CreateAccessTokenAsync(It.IsAny<IdentityUser>()))
                 .ReturnsAsync("access_token");
        _emailSvc.Setup(e => e.SendEmailConfirmation(dto.Email, It.IsAny<string>()))
                 .Returns(Task.CompletedTask);

        var ctrl = CreateControllerWithUser("any-id");

        // Act
        var result = await ctrl.Register(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
        var userDto = Assert.IsType<UserDto>(actionResult.Value);
        Assert.Equal(dto.Email, userDto.Email);
        Assert.Equal("access_token", userDto.Token);
    }
}

