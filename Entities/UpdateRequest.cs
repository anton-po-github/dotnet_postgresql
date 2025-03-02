using System.ComponentModel.DataAnnotations;

public class UpdateRequest
{
    required
    public string FirstName
    { get; set; }
    required
    public string LastName
    { get; set; }

    [EmailAddress]
    required
    public string Email
    { get; set; }
    public Byte[]? PhotoUrl { get; set; }


    private string replaceEmptyWithNull(string value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }
}