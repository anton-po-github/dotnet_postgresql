using System.ComponentModel.DataAnnotations;

public class CreateRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    [EmailAddress]
    public string Email { get; set; }
    public Byte[]? PhotoUrl { get; set; }

}