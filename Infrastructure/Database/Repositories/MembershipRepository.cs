namespace SolarWise;

// Organization memberships, stored in Supabase (table memberships).
public static class MembershipRepository
{
    public static string RoleIn(User user, Organization org) =>
        OrganizationRepository.Database.RoleOf(org.OrganizationID, user.UserID) ?? "None";

    public static int MemberCount(Organization org) =>
        OrganizationRepository.Database.MemberCount(org.OrganizationID);

    public static bool CanManage(User user, Organization org) =>
        user.IsAdmin || RoleIn(user, org) == "Admin";

    public static void JoinOrganization(Organization org, User user) =>
        OrganizationRepository.Database.AddMember(org.OrganizationID, user.UserID, "Member");
}
