
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class User : BaseEntity
{
    public string? FirstName { get; set; }
    public string? Phone { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Details { get; set; }
}

public class AddUpdateUser
{
    [FromForm(Name = "firstName")]
    public string FirstName { get; set; }

    [FromForm(Name = "lastName")]
    public string LastName { get; set; }

    [FromForm(Name = "email")]
    [EmailAddress]
    public string Email { get; set; }

    [FromForm(Name = "photo")]
    public IFormFile File { get; set; }
}

public class UsersDetails
{
    public string Role { get; set; }
    public int Salary { get; set; }
}


public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class RegisterDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public string refreshToken { get; set; }
    public string accessToken { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string IdentityUserId { get; set; }
    public IList<string> Role { get; set; }
}

public class UpdateRequest
{
    [FromForm(Name = "firstName")]
    public string FirstName { get; set; }

    [FromForm(Name = "lastName")]
    public string LastName { get; set; }

    [FromForm(Name = "email")]
    [EmailAddress]
    public string Email { get; set; }

    [FromForm(Name = "photo")]
    public IFormFile File { get; set; }
    public Byte[]? PhotoUrl { get; set; }
}

public class CreateRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    [EmailAddress]
    public string Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; }
    public IdentityUser IdentityUser { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}


