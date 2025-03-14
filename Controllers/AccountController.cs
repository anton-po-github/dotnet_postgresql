using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly EmailService _emailService;
    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        TokenService tokenService,
        EmailService emailService,
        IMapper mapper)
    {
        _mapper = mapper;
        _tokenService = tokenService;
        _emailService = emailService;
        _signInManager = signInManager;
        _userManager = userManager;

    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.FindByEmailFromClaimsPrinciple(User);

        return new UserDto
        {
            Email = user.Email,
            Token = _tokenService.CreateToken(user),
            UserName = user.UserName
        };
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<List<AppUser>>> GetAllUsers()
    {
        return await _userManager.Users.ToListAsync();
    }

    [HttpGet("emailexists")]
    public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized(new ApiResponse(401));

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed) throw new AppException("Email is not Confirmed");

        return new UserDto
        {
            Email = user.Email,
            Token = _tokenService.CreateToken(user),
            UserName = user.UserName
        };
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName
        };

        if (_userManager.Users.Any(x => x.Email == registerDto.Email))
            throw new AppException("User with the email '" + registerDto.Email + "' already exists");

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(new ApiResponse(400));

        var confirmLink = $"http://localhost:5135/api/account/confirm-email?email={user.Email}";

        var textEmail = @"
        Hello.

        Please confirm your email address by clicking the link below

        " + confirmLink + @"

        Thank You !
        Regards
        ";

        await _emailService.SendEmailConfirmation(user.Email, textEmail);

        return new UserDto
        {
            Email = user.Email,
            Token = _tokenService.CreateToken(user),
            UserName = user.UserName
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

        var result = await _userManager.DeleteAsync(user);

        return result;

    }
}
