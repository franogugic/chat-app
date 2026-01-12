namespace ChatApp.Application.Exceptions;

public class UserAlreadyExistsException : Exception
{
    private readonly string Mail;
    
    public UserAlreadyExistsException(string mail) : base($"User wiht mail '{mail}' already exists.")
    {
        Mail = mail;
    }
}