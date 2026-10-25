using System.Security.Cryptography;
using System.Text;

namespace SolarWise;

// Accounts: login, registration and uniqueness checks.
public static class AuthService
{
    // Prototype only: a real system needs a salted password hash (for example, ASP.NET Identity or BCrypt).
    public static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public static User Authenticate(string login, string password)
    {
        string hash = Hash(password);
        return DataStore.Users.FirstOrDefault(u =>
            (string.Equals(u.Username, login, StringComparison.OrdinalIgnoreCase)
             || string.Equals(u.EmailAddress, login, StringComparison.OrdinalIgnoreCase))
            && u.PasswordHash == hash);
    }

    public static bool EmailTaken(string email, User except = null) =>
        DataStore.Users.Any(u => u != except && string.Equals(u.EmailAddress, email, StringComparison.OrdinalIgnoreCase));

    public static bool UsernameTaken(string username, User except = null) =>
        DataStore.Users.Any(u => u != except && string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public static User Register(string fullName, string username, string email, string password)
    {
        var user = new User
        {
            UserID = DataStore.NextId(),
            FullName = fullName,
            Username = username,
            EmailAddress = email,
            PasswordHash = Hash(password)
        };
        DataStore.Users.Add(user);
        return user;
    }
}
