using System.Security.Cryptography;
using System.Text;

namespace SolarWise;

// Accounts: login, registration and uniqueness checks. Accounts live in Supabase (UserRepository).
public static class AuthService
{
    // Prototype only: a real system needs a salted password hash (for example, ASP.NET Identity or BCrypt).
    public static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public static User Authenticate(string login, string password)
    {
        var user = UserRepository.FindByLogin(login);
        return user != null && user.PasswordHash == Hash(password) ? user : null;
    }

    public static bool EmailTaken(string email, User except = null)
    {
        var found = UserRepository.FindByEmail(email);
        return found != null && (except == null || found.UserID != except.UserID);
    }

    public static bool UsernameTaken(string username, User except = null)
    {
        var found = UserRepository.FindByUsername(username);
        return found != null && (except == null || found.UserID != except.UserID);
    }

    public static User Register(string fullName, string username, string email, string password) =>
        UserRepository.AddUser(new User
        {
            FullName = fullName,
            Username = username,
            EmailAddress = email,
            PasswordHash = Hash(password)
        });
}
