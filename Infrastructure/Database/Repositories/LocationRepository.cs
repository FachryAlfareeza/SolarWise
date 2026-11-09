namespace SolarWise;

// Site locations owned by users.
public static class LocationRepository
{
    public static readonly List<SiteLocation> Locations = new();

    public static List<SiteLocation> LocationsFor(User user) =>
        Locations.Where(l => l.OwnerUserID == user.UserID).ToList();

    public static SiteLocation AddLocation(SiteLocation site, User owner)
    {
        site.LocationID = InMemoryDatabase.NextId();
        site.OwnerUserID = owner.UserID;
        Locations.Add(site);
        return site;
    }

    public static void DeleteLocation(SiteLocation site)
    {
        Locations.Remove(site);
        if (SessionContext.SelectedLocation == site) SessionContext.SelectedLocation = null;
    }
}
