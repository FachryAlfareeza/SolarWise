using SolarWise.Database;

namespace SolarWise;

// Solar panel catalog, stored in Supabase (table solar_panels).
public static class SolarPanelRepository
{
    private static readonly SolarPanelDbRepository Db = new();

    public static List<SolarPanel> Panels => Db.GetAll();

    public static SolarPanel AddPanel(SolarPanel panel) => Db.Insert(panel);
    public static bool UpdatePanel(SolarPanel panel) => Db.Update(panel);

    // Throws a PostgresException (23503) while saved calculations still use the panel.
    public static void DeletePanel(SolarPanel panel) => Db.Delete(panel);
}
