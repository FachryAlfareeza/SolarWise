namespace SolarWise;

public class WeatherView : ViewBase
{
    private static readonly string[] MonthNames =
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    private readonly TextBox placeBox = Ui.NewText(new Point(150, 24), 350);
    private readonly ComboBox locationCombo = Ui.NewCombo(new Point(150, 60), 350);
    private readonly Label statusLabel = Ui.NewLabel("Enter an Indonesian city or region.", new Point(16, 190), Theme.BoldFont);
    private readonly Label coordinatesValue = Ui.NewLabel("-", new Point(150, 24));
    private readonly Label averageValue = Ui.NewLabel("-", new Point(250, 24), Theme.BoldFont);
    private readonly ListView monthList = Ui.NewList(new Point(16, 226), new Size(800, 210),
        ("Month", 220), ("Solar irradiance (kWh/m²/day)", 300), ("Source", 240));
    private readonly ClassicButton fetchButton;

    public WeatherView(MainForm host) : base(host, "Weather Data")
    {
        AddHeader("Solar Irradiance by Region", "Search a region with Open-Meteo, then load NASA POWER monthly solar climatology.");

        var location = Ui.NewGroup("Region Lookup", new Rectangle(16, 70, 800, 106));
        location.Controls.Add(Ui.NewLabel("Region or city:", new Point(20, 28)));
        location.Controls.Add(placeBox);
        fetchButton = Ui.NewButton("Fetch Weather Data", new Point(515, 23), async (_, _) => await FetchAsync(), 170, true);
        location.Controls.Add(fetchButton);
        location.Controls.Add(Ui.NewLabel("Saved location:", new Point(20, 64)));
        location.Controls.Add(locationCombo);
        location.Controls.Add(Ui.NewButton("Refresh", new Point(515, 59), (_, _) => ShowSavedClimatology(), 90));
        location.Controls.Add(Ui.NewLabel("Coordinates:", new Point(610, 64)));
        location.Controls.Add(coordinatesValue);
        Controls.Add(location);

        Controls.Add(statusLabel);
        Controls.Add(monthList);

        var summary = Ui.NewGroup("Annual Irradiance Used by Calculator", new Rectangle(16, 448, 800, 62));
        summary.Controls.Add(Ui.NewLabel("Day-weighted average:", new Point(20, 24)));
        summary.Controls.Add(averageValue);
        Controls.Add(summary);

        var note = Ui.NewLabel(
            "NASA POWER climatology is a long-term monthly average, not a live forecast. Monthly values are saved for this account and used by the solar calculator.",
            new Point(16, 516), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(800, 0);
        Controls.Add(note);

        locationCombo.SelectedIndexChanged += (_, _) => ShowSavedClimatology();
        Reload();
    }

    public override void Reload()
    {
        var selected = locationCombo.SelectedItem as SiteLocation;
        locationCombo.Items.Clear();
        foreach (var site in LocationRepository.LocationsFor(SessionContext.CurrentUser)) locationCombo.Items.Add(site);

        if (selected != null && locationCombo.Items.Contains(selected)) locationCombo.SelectedItem = selected;
        else if (locationCombo.Items.Count > 0) locationCombo.SelectedIndex = 0;
        else ClearDisplay("No saved locations yet. Enter a region above to fetch its solar climatology.");
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
        statusLabel.Text = "Status: finding region and fetching NASA POWER data...";
        statusLabel.ForeColor = Color.Black;
        try
        {
            var site = await WeatherService.FetchAndSaveClimatologyAsync(place, SessionContext.CurrentUser);
            Reload();
            locationCombo.SelectedItem = site;
            statusLabel.Text = $"Status: climatology loaded for {site.Address}";
            statusLabel.ForeColor = Theme.SuccessGreen;
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: fetch failed";
            statusLabel.ForeColor = Theme.ErrorRed;
            ClassicMessage.Error(this, ex.Message, "Fetch Weather Data");
        }
        finally
        {
            fetchButton.Enabled = true;
        }
    }

    private void ShowSavedClimatology()
    {
        if (locationCombo.SelectedItem is not SiteLocation site)
        {
            ClearDisplay("Select a saved location or search for an Indonesian region above.");
            return;
        }

        var coordinates = site.GetCoordinates();
        coordinatesValue.Text = $"{coordinates.Lat:0.####}, {coordinates.Lon:0.####}";
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
        statusLabel.Text = monthly.Count == 12
            ? $"Status: {site.Address} · {monthly.Count} monthly values"
            : $"Status: no NASA POWER climatology saved for {site.Address}";
        statusLabel.ForeColor = monthly.Count == 12 ? Theme.SuccessGreen : Color.Black;
    }

    private void ClearDisplay(string message)
    {
        coordinatesValue.Text = "-";
        averageValue.Text = "-";
        monthList.Items.Clear();
        statusLabel.Text = message;
        statusLabel.ForeColor = Color.Black;
    }
}
