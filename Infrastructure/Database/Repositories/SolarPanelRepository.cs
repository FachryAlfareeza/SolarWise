namespace SolarWise;

// Solar panel catalog.
public static class SolarPanelRepository
{
    public static readonly List<SolarPanel> Panels = new();

    public static SolarPanel AddPanel(SolarPanel panel)
    {
        panel.PanelID = InMemoryDatabase.NextId();
        Panels.Add(panel);
        return panel;
    }

    public static void DeletePanel(SolarPanel panel) => Panels.Remove(panel);
}
