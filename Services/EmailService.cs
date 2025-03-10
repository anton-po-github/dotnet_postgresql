using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService(IConfiguration config)
{
    public async Task SendEmailConfirmation(string email, string configLink)
    {
        var client = new SendGridClient(config["SendGridApiKey"]);
        var from = new EmailAddress("myworkinggemail@gmail.com", "Company name");
        var subject = "Account Confirmation";
        var to = new EmailAddress(email);
        var plainTextContent = configLink;
        // var htmlContent = "<strong>and easy to do anywhere with C#.</strong>";
        var htmlContent = string.Empty;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
        //var response = await client.SendEmailAsync(msg);
    }

}