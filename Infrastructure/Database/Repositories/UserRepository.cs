namespace SolarWise;

// In-memory user accounts (logic lives in AuthService).
public static class UserRepository
{
    public static readonly List<User> Users = new();
}
