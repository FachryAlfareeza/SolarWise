using System.Security.Cryptography;
using SolarWise.Database;

namespace SolarWise;

// Organizations, stored in Supabase (tables organizations and memberships).
public static class OrganizationRepository
{
    private static readonly OrganizationDbRepository Db = new();

    public static List<Organization> Organizations => Db.GetAll();

    public static List<Organization> OrganizationsFor(User user) => Db.ForUser(user.UserID);

    public static Organization FindByJoinCode(string code) => Db.FindByJoinCode(code?.Trim());

    public static bool NameTaken(string name, Organization except = null) =>
        Db.NameTaken(name, except?.OrganizationID ?? 0);

    public static Organization AddOrganization(Organization org, User creator)
    {
        if (string.IsNullOrEmpty(org.JoinCode))
        {
            org.JoinCode = "ORG-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
        }
        Db.Insert(org);
        Db.AddMember(org.OrganizationID, creator.UserID, "Admin");
        return org;
    }

    public static bool UpdateOrganization(Organization org) => Db.Update(org);

    // Memberships are removed with it (ON DELETE CASCADE); calculations keep their results.
    public static void DeleteOrganization(Organization org)
    {
        Db.Delete(org);
        if (SessionContext.ActiveOrganization?.OrganizationID == org.OrganizationID) SessionContext.ActiveOrganization = null;
    }

    internal static OrganizationDbRepository Database => Db;
}
