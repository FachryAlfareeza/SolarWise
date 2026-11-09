using System.Security.Cryptography;

namespace SolarWise;

// Organizations (PROTOTYPE).
public static class OrganizationRepository
{
    public static readonly List<Organization> Organizations = new();

    public static List<Organization> OrganizationsFor(User user) =>
        Organizations.Where(o => MembershipRepository.Memberships.Any(m => m.UserID == user.UserID && m.OrganizationID == o.OrganizationID)).ToList();

    public static Organization FindByJoinCode(string code) =>
        Organizations.FirstOrDefault(o => string.Equals(o.JoinCode, code?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static Organization AddOrganization(Organization org, User creator)
    {
        org.OrganizationID = InMemoryDatabase.NextId();
        if (string.IsNullOrEmpty(org.JoinCode))
        {
            org.JoinCode = "ORG-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
        }
        Organizations.Add(org);
        MembershipRepository.Memberships.Add(new OrgMembership { UserID = creator.UserID, OrganizationID = org.OrganizationID, Role = "Admin" });
        return org;
    }

    public static void DeleteOrganization(Organization org)
    {
        Organizations.Remove(org);
        MembershipRepository.Memberships.RemoveAll(m => m.OrganizationID == org.OrganizationID);
        if (SessionContext.ActiveOrganization == org) SessionContext.ActiveOrganization = null;
    }
}
