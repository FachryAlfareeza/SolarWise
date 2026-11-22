using SolarWise.Database;

namespace SolarWise;

// User accounts, stored in Supabase (tables users and admins). Business rules live in AuthService.
public static class UserRepository
{
    private static readonly UserDbRepository Db = new();

    public static List<User> Users => Db.GetAll();

    public static User FindByLogin(string login) => Db.FindByLogin(login);
    public static User FindByUsername(string username) => Db.FindByUsername(username);
    public static User FindByEmail(string email) => Db.FindByEmail(email);

    public static User AddUser(User user) => Db.Insert(user);
    public static bool UpdateUser(User user) => Db.Update(user);
    public static bool DeleteUser(User user) => Db.Delete(user);
    public static bool HasAnyUser() => Db.HasAnyUser();

    public static void GrantAdmin(User user) => Db.GrantAdmin(user);
}
