namespace ChatApp.Application.Exceptions;

public class RefreshTokenIsNotValidException : Exception
{
    public RefreshTokenIsNotValidException() : base("The provided refresh token is not valid.")
    {
        
    }
}