using ChatApp.Domain.Entities;

namespace ChatApp.IntegrationTests;

public static class TestUserFactory
{
    public static User CreateUser(
        string name = "Test User", 
        string mail = "test@example.com", 
        string passwordHash = "hashed_pass", 
        string phone = "123456")
    {
        return User.Create(name, mail, passwordHash, phone);
    }
}