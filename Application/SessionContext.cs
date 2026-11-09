namespace SolarWise;

// The signed-in user and their current selections. Reset on logout.
public static class SessionContext
{
    public static User CurrentUser { get; set; }
    public static Organization ActiveOrganization { get; set; }
    public static SiteLocation SelectedLocation { get; set; }

    public static int ActiveOrganizationId => ActiveOrganization?.OrganizationID ?? 0;
}
