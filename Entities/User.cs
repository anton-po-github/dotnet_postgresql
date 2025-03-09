
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

public class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? Phone { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Details { get; set; }
}
public class AppUser : IdentityUser
{
    public string DisplayName { get; set; }
}
public class LoginDto
{
    required
    public string Email
    { get; set; }
    required
    public string Password
    { get; set; }
}
public class RegisterDto
{
    required
    public string DisplayName
    { get; set; }
    required
    public string Email
    { get; set; }
    required
    public string Password
    { get; set; }
}

public class UserDto
{
    required
    public string Email
    { get; set; }
    required
    public string DisplayName
    { get; set; }
    required
    public string Token
    { get; set; }
}