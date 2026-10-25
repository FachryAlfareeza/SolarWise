namespace SolarWise;

public class LocationsView : ViewBase
{
    private readonly ListView list = Ui.NewList(new Point(16, 70), new Size(800, 300),
        ("Address", 360), ("Latitude", 210), ("Longitude", 210));

    public LocationsView(MainForm host) : base(host, "Locations")
    {
        AddHeader("Locations", "Saved locations for your account. Coordinates are stored as text, as in the class diagram.");
        Controls.Add(list);

        int x = 16;
        Controls.Add(Ui.NewButton("Add Location...", new Point(x, 390), (_, _) => AddLocation(), 130, true)); x += 138;
        Controls.Add(Ui.NewButton("Edit...", new Point(x, 390), (_, _) => EditLocation(), 90)); x += 98;
        Controls.Add(Ui.NewButton("Delete", new Point(x, 390), (_, _) => DeleteLocation(), 90)); x += 98;
        Controls.Add(Ui.NewButton("Select Location", new Point(x, 390), (_, _) => SelectLocation(), 120)); x += 128;
        Controls.Add(Ui.NewButton("Refresh", new Point(x, 390), (_, _) => Reload(), 90)); x += 98;
        Controls.Add(Ui.NewButton("Close", new Point(x, 390), (_, _) => Host.OpenSection("Home"), 90));

        list.DoubleClick += (_, _) => EditLocation();
        Reload();
    }

    public override void Reload()
    {
        list.Items.Clear();
        foreach (var site in DataStore.LocationsFor(DataStore.CurrentUser))
        {
            Ui.AddRow(list, site, site.Address, site.Latitude, site.Longitude);
        }
    }

    private void AddLocation()
    {
        var site = new SiteLocation();
        using var dialog = new LocationEditDialog(site);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        DataStore.AddLocation(site, DataStore.CurrentUser);
        Reload();
        ClassicMessage.Info(this, "Location saved.", "Locations");
    }

    private void EditLocation()
    {
        var site = Ui.Selected<SiteLocation>(list);
        if (site == null)
        {
            ClassicMessage.Info(this, "Select a location first.", "Locations");
            return;
        }

        using var dialog = new LocationEditDialog(site);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Reload();
        ClassicMessage.Info(this, "Location updated.", "Locations");
    }

    private void DeleteLocation()
    {
        var site = Ui.Selected<SiteLocation>(list);
        if (site == null)
        {
            ClassicMessage.Info(this, "Select a location first.", "Locations");
            return;
        }
        if (!ClassicMessage.Confirm(this, "Delete this location? Calculations already saved with it keep their results.", "Delete Location")) return;

        DataStore.DeleteLocation(site);
        Reload();
    }

    private void SelectLocation()
    {
        var site = Ui.Selected<SiteLocation>(list);
        if (site == null)
        {
            ClassicMessage.Info(this, "Select a location first.", "Select Location");
            return;
        }

        DataStore.SelectedLocation = site;
        ClassicMessage.Info(this, $"{site.Address} selected. It will be pre-filled in the next calculation.", "Select Location");
    }
}
