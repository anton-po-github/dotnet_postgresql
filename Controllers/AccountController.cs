using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
//  [Authorize]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly EmailService _emailService;
    private readonly IdentityContext _identityContext;
    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IdentityContext identityContext,
        TokenService tokenService,
        EmailService emailService,
        ILogger<AccountController> logger,
        IMapper mapper
        )
    {
        _identityContext = identityContext;
        _tokenService = tokenService;
        _emailService = emailService;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("secret")]
    public IActionResult SecretEndpoint()
    {
        return Ok("You are an admin, congratulations!");
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not found or token is invalid" }); ;

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "User not found or token is invalid" });

        return new UserDto
        {
            Email = user.Email,
            UserName = user.UserName,
            IdentityUserId = userId,
            Role = await _userManager.GetRolesAsync(user)
        };
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<IdentityUser>>> GetAllUsers()
    {
        return await _userManager.Users.ToListAsync();
    }

    [HttpGet("emailexists")]
    public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new IdentityUser
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName
        };

        if (_userManager.Users.Any(x => x.Email == registerDto.Email))
            throw new AppException("User with the email '" + registerDto.Email + "' already exists");

        if (_userManager.Users.Any(x => x.UserName == registerDto.UserName))
            throw new AppException("User with the UserName '" + registerDto.UserName + "' already exists");

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(new ApiResponse(400));

        var addUserToRoleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!addUserToRoleResult.Succeeded) throw new AppException($"Create user succeeded but could not add user to role {addUserToRoleResult?.Errors?.First()?.Description}");

        var confirmLink = $"http://127.0.0.1:8080/api/account/confirm-email?email={user.Email}";

        var textEmail = @"
        Hello.

        Please confirm your email address by clicking the link below

        " + confirmLink + @"

        Thank You !
        Regards
        ";

        if (!string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendEmailConfirmation(user.Email, textEmail);
        }

        return new UserDto
        {
            Email = user.Email,
            IdentityUserId = user.Id,
            UserName = user.UserName,
            Role = await _userManager.GetRolesAsync(user)
        };
    }

    // [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized(new ApiResponse(401));

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed) throw new AppException("Email is not Confirmed");

        var accessToken = await _tokenService.CreateAccessTokenAsync(user);

        var refreshToken = await _tokenService.CreateRefreshTokenAsync(GetClientIp(), user);

        _identityContext.RefreshTokens.Add(refreshToken);

        await _identityContext.SaveChangesAsync();

        return new UserDto
        {
            accessToken = accessToken,
            refreshToken = refreshToken.Token,
            Email = user.Email,
            IdentityUserId = user.Id,
            UserName = user.UserName,
            Role = await _userManager.GetRolesAsync(user)
        };
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<UserDto>> Refresh(RefreshRequestDto refreshRequestDto)
    {
        _logger.LogInformation("Refresh: AccessToken={token}, RefreshToken={rt}", refreshRequestDto.AccessToken, refreshRequestDto.RefreshToken);

        // 1) получаем principal из просроченного access
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshRequestDto.AccessToken);
        if (principal == null)
        {
            _logger.LogWarning("Не удалось распарсить AccessToken");
            return Unauthorized();
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        _logger.LogInformation("Пользователь из токена: {userId}", userId);

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        var storedToken = await _identityContext.RefreshTokens
        .SingleOrDefaultAsync(rt => rt.Token == refreshRequestDto.RefreshToken);

        _logger.LogInformation("Найден RefreshToken в БД: {found}", storedToken != null);

        if (storedToken == null) return Unauthorized();

        // 3) отзываем старый и сохраняем новый
        storedToken.Revoked = DateTime.UtcNow;
        storedToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(GetClientIp(), user);
        storedToken.ReplacedByToken = newRefreshToken.Token;

        _identityContext.RefreshTokens.Add(newRefreshToken);

        await _identityContext.SaveChangesAsync();

        // 4) создаём новый access и отдаем оба
        var newAccessToken = await _tokenService.CreateAccessTokenAsync(user);

        return new UserDto
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token,
            Email = user.Email,
            IdentityUserId = user.Id,
            UserName = user.UserName,
            Role = await _userManager.GetRolesAsync(user)
        };
    }

    [HttpGet("confirm-email")]
    public async Task<IResult> ConfirmEmail([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return Results.NotFound("User not found.");

        user.EmailConfirmed = true;

        await _userManager.UpdateAsync(user);

        return Results.Ok("Confirmed!");
    }

    [HttpDelete("{id}")]
    public async Task<IdentityResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new AppException("User not found.");

        var result = await _userManager.DeleteAsync(user);

        return result;

    }

    private string GetClientIp()
    {
        // HttpContext and Connection should be non-null when handling a request in ControllerBase
        // RemoteIpAddress may be null (e.g., non-TCP transports); use null-conditional and fallback
        return HttpContext?
            .Connection?
            .RemoteIpAddress?
            .ToString()
            ?? "unknown";
    }
}
