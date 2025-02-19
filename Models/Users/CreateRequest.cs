namespace WebApi.Models.Users;

using System.ComponentModel.DataAnnotations;

public class CreateRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }

}