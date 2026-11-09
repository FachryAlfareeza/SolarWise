namespace SolarWise;

// Organization memberships (PROTOTYPE join table).
public static class MembershipRepository
{
    public static readonly List<OrgMembership> Memberships = new();

    public static string RoleIn(User user, Organization org) =>
        Memberships.FirstOrDefault(m => m.UserID == user.UserID && m.OrganizationID == org.OrganizationID)?.Role ?? "None";

    public static int MemberCount(Organization org) =>
        Memberships.Count(m => m.OrganizationID == org.OrganizationID);

    public static bool CanManage(User user, Organization org) =>
        user.IsAdmin || RoleIn(user, org) == "Admin";

    public static void JoinOrganization(Organization org, User user) =>
        Memberships.Add(new OrgMembership { UserID = user.UserID, OrganizationID = org.OrganizationID, Role = "Member" });
}
