namespace SolarWise;

public class LocationsView : ViewBase
{
    private static readonly string[] MonthNames =
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    private readonly TextBox placeBox = Ui.NewText(new Point(150, 24), 420);
    private readonly ListView list = Ui.NewList(new Point(16, 190), new Size(800, 112),
        ("Address", 360), ("Latitude", 210), ("Longitude", 210));
    private readonly ListView monthList = Ui.NewList(new Point(16, 354), new Size(800, 145),
        ("Month", 220), ("Solar irradiance (kWh/m²/day)", 300), ("Source", 240));
    private readonly Label statusLabel = Ui.NewLabel("Enter a region or city to fetch its solar climatology.", new Point(16, 164), Theme.BoldFont);
    private readonly Label averageValue = Ui.NewLabel("-", new Point(250, 24), Theme.BoldFont);
    private readonly ClassicButton fetchButton;

    public LocationsView(MainForm host) : base(host, "Locations")
    {
        AddHeader("Locations", "Search a region with Open-Meteo and load NASA POWER monthly solar climatology.");

        var search = Ui.NewGroup("Find Region and Solar Data", new Rectangle(16, 70, 800, 78));
        search.Controls.Add(Ui.NewLabel("Region or city:", new Point(20, 28)));
        search.Controls.Add(placeBox);
        fetchButton = Ui.NewButton("Fetch Climatology", new Point(590, 23), async (_, _) => await FetchAsync(), 150, true);
        search.Controls.Add(fetchButton);
        Controls.Add(search);

        Controls.Add(statusLabel);
        Controls.Add(list);

        int x = 16;
        Controls.Add(Ui.NewButton("Add Manually...", new Point(x, 310), (_, _) => AddLocation(), 130)); x += 138;
        Controls.Add(Ui.NewButton("Edit...", new Point(x, 310), (_, _) => EditLocation(), 90)); x += 98;
        Controls.Add(Ui.NewButton("Delete", new Point(x, 310), (_, _) => DeleteLocation(), 90)); x += 98;
        Controls.Add(Ui.NewButton("Select Location", new Point(x, 310), (_, _) => SelectLocation(), 120)); x += 128;
        Controls.Add(Ui.NewButton("Refresh", new Point(x, 310), (_, _) => Reload(), 90));

        Controls.Add(monthList);
        var summary = Ui.NewGroup("Annual Irradiance Used by Calculator", new Rectangle(16, 510, 800, 58));
        summary.Controls.Add(Ui.NewLabel("Day-weighted average:", new Point(20, 22)));
        summary.Controls.Add(averageValue);
        Controls.Add(summary);

        list.SelectedIndexChanged += (_, _) => ShowSelectedClimatology();
        list.DoubleClick += (_, _) => EditLocation();
        Reload();
    }

    public override void Reload()
    {
        var selected = Ui.Selected<SiteLocation>(list);
        list.Items.Clear();
        foreach (var site in LocationRepository.LocationsFor(SessionContext.CurrentUser))
        {
            Ui.AddRow(list, site, site.Address, site.Latitude, site.Longitude);
        }

        if (selected != null) Ui.SelectRow(list, selected);
        if (list.SelectedItems.Count == 0 && list.Items.Count > 0)
        {
            list.Items[0].Selected = true;
            list.Items[0].Focused = true;
        }
        if (list.Items.Count == 0) ClearClimatology("No saved locations yet. Search for a region above.");
    }

    private async Task FetchAsync()
    {
        string place = placeBox.Text.Trim();
        if (place.Length == 0)
        {
            Ui.Fail("Enter an Indonesian city, regency, or region.", placeBox);
            return;
        }

        fetchButton.Enabled = false;
        statusLabel.Text = "Status: finding region and fetching NASA POWER climatology...";
        statusLabel.ForeColor = Color.Black;
        try
        {
            var site = await WeatherService.FetchAndSaveClimatologyAsync(place, SessionContext.CurrentUser);
            placeBox.Clear();
            Reload();
            Ui.SelectRow(list, site);
            statusLabel.Text = $"Status: climatology loaded for {site.Address}";
            statusLabel.ForeColor = Theme.SuccessGreen;
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: fetch failed";
            statusLabel.ForeColor = Theme.ErrorRed;
            ClassicMessage.Error(this, ex.Message, "Fetch Climatology");
        }
        finally
        {
            fetchButton.Enabled = true;
        }
    }

    private void ShowSelectedClimatology()
    {
        if (Ui.Selected<SiteLocation>(list) is not SiteLocation site)
        {
            ClearClimatology("Select a saved location to view its monthly solar data.");
            return;
        }

        monthList.Items.Clear();
        var monthly = WeatherService.MonthlyClimatologyFor(site);
        foreach (var record in monthly)
        {
            Ui.AddRow(monthList, record,
                MonthNames[record.Date.Month - 1],
                Theme.Num(record.SolarIrradiance, "0.00"),
                "NASA POWER climatology");
        }

        var average = WeatherService.SavedIrradiance(site);
        averageValue.Text = average.HasValue
            ? $"{Theme.Num(average.Value, "0.00")} kWh/m²/day"
            : "No data";
        if (monthly.Count != 12)
        {
            statusLabel.Text = $"Status: no NASA POWER climatology saved for {site.Address}";
            statusLabel.ForeColor = Color.Black;
        }
    }

    private void ClearClimatology(string message)
    {
        monthList.Items.Clear();
        averageValue.Text = "-";
        statusLabel.Text = message;
        statusLabel.ForeColor = Color.Black;
    }

    private void AddLocation()
    {
        var site = new SiteLocation();
        using var dialog = new LocationEditDialog(site);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        LocationRepository.AddLocation(site, SessionContext.CurrentUser);
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

        LocationRepository.UpdateLocation(site);
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
        if (!ClassicMessage.Confirm(this, "Delete this location? Calculations and weather records saved with it are deleted too.", "Delete Location")) return;

        LocationRepository.DeleteLocation(site);
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

        SessionContext.SelectedLocation = site;
        ClassicMessage.Info(this, $"{site.Address} selected. It will be pre-filled in the next calculation.", "Select Location");
    }
}
