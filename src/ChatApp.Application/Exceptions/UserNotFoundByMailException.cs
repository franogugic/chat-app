namespace ChatApp.Application.Exceptions;

public class UserNotFoundByMailException : Exception
{
    private readonly string Mail;
    
    public UserNotFoundByMailException(string mail) : base($"User with mail '{mail}' was not found.")
    {
        Mail = mail;
    }
}