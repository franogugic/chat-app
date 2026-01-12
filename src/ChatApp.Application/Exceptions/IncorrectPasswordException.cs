namespace ChatApp.Application.Exceptions;

public class IncorrectPasswordException : Exception
{
    public IncorrectPasswordException() : base("The provided password is incorrect.")
    {
    }
    
}