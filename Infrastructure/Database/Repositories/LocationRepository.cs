using SolarWise.Database;

namespace SolarWise;

// Site locations owned by users, stored in Supabase (table locations).
public static class LocationRepository
{
    private static readonly LocationDbRepository Db = new();

    public static List<SiteLocation> LocationsFor(User user) => Db.ForUser(user.UserID);

    public static SiteLocation AddLocation(SiteLocation site, User owner) => AddLocation(site, owner.UserID);

    public static SiteLocation AddLocation(SiteLocation site, int ownerUserId)
    {
        site.OwnerUserID = ownerUserId;
        return Db.Insert(site);
    }

    public static bool UpdateLocation(SiteLocation site) => Db.Update(site);

    // Saved calculations and weather records of this location are removed with it (ON DELETE CASCADE).
    public static void DeleteLocation(SiteLocation site)
    {
        Db.Delete(site);
        if (SessionContext.SelectedLocation?.LocationID == site.LocationID) SessionContext.SelectedLocation = null;
    }
}
