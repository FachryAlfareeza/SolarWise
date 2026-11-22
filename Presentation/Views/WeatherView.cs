namespace SolarWise;

public class WeatherView : ViewBase
{
    private readonly ComboBox locationCombo = Ui.NewCombo(new Point(150, 24), 420);
    private readonly DateTimePicker datePicker = new() { Location = new Point(150, 56), Width = 200, Format = DateTimePickerFormat.Short, Font = Theme.UiFont };
    private readonly Label statusLabel = Ui.NewLabel("Status: not loaded", new Point(600, 60), Theme.BoldFont);
    private readonly Label irradianceValue = Ui.NewLabel("-", new Point(230, 26));
    private readonly Label temperatureValue = Ui.NewLabel("-", new Point(230, 52));
    private readonly Label sunshineValue = Ui.NewLabel("-", new Point(230, 78));
    private readonly Label averageValue = Ui.NewLabel("-", new Point(230, 104));
    private readonly Label sourceValue = Ui.NewLabel("-", new Point(230, 130));

    public WeatherView(MainForm host) : base(host, "Weather Data")
    {
        AddHeader("Weather Data", "View saved weather or monthly solar climatology for one of your locations.");

        var location = Ui.NewGroup("Location and Date", new Rectangle(16, 70, 800, 96));
        location.Controls.Add(Ui.NewLabel("Location:", new Point(20, 28)));
        location.Controls.Add(locationCombo);
        location.Controls.Add(Ui.NewLabel("Date:", new Point(20, 60)));
        location.Controls.Add(datePicker);
        location.Controls.Add(statusLabel);
        Controls.Add(location);

        var values = Ui.NewGroup("Weather Values", new Rectangle(16, 178, 800, 160));
        values.Controls.Add(Ui.NewLabel("Solar irradiance (kWh/m²/day):", new Point(20, 26)));
        values.Controls.Add(irradianceValue);
        values.Controls.Add(Ui.NewLabel("Temperature (°C):", new Point(20, 52)));
        values.Controls.Add(temperatureValue);
        values.Controls.Add(Ui.NewLabel("Sunshine hours (h/day):", new Point(20, 78)));
        values.Controls.Add(sunshineValue);
        values.Controls.Add(Ui.NewLabel("Average irradiance, saved records:", new Point(20, 104)));
        values.Controls.Add(averageValue);
        values.Controls.Add(Ui.NewLabel("Data source:", new Point(20, 130)));
        values.Controls.Add(sourceValue);
        Controls.Add(values);

        Controls.Add(Ui.NewButton("Fetch Weather Data", new Point(16, 356), (_, _) => Fetch(), 150, true));
        Controls.Add(Ui.NewButton("Refresh", new Point(174, 356), (_, _) => ShowRecord(), 90));
        Controls.Add(Ui.NewButton("Load Sample Data", new Point(272, 356), (_, _) => LoadSample(), 140));
        Controls.Add(Ui.NewButton("Close", new Point(420, 356), (_, _) => Host.OpenSection("Home"), 90));

        var note = Ui.NewLabel(
            "Region search and NASA POWER monthly climatology are managed from Locations. Climatology provides irradiance only; temperature and sunshine duration are not supplied here.",
            new Point(16, 400), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(800, 0);
        Controls.Add(note);

        locationCombo.SelectedIndexChanged += (_, _) => ShowRecord();
        datePicker.ValueChanged += (_, _) => ShowRecord();
        Reload();
    }

    public override void Reload()
    {
        var selected = locationCombo.SelectedItem as SiteLocation;
        locationCombo.Items.Clear();
        foreach (var site in LocationRepository.LocationsFor(SessionContext.CurrentUser)) locationCombo.Items.Add(site);
        if (selected != null && locationCombo.Items.Contains(selected)) locationCombo.SelectedItem = selected;
        else if (locationCombo.Items.Count > 0) locationCombo.SelectedIndex = 0;
        ShowRecord();
    }

    private SiteLocation SelectedSite => locationCombo.SelectedItem as SiteLocation;

    private void ShowRecord()
    {
        var site = SelectedSite;
        if (site == null)
        {
            irradianceValue.Text = temperatureValue.Text = sunshineValue.Text = averageValue.Text = sourceValue.Text = "-";
            statusLabel.Text = "Status: add a location first";
            return;
        }

        var average = WeatherService.SavedIrradiance(site);
        averageValue.Text = average.HasValue ? Theme.Num(average.Value, "0.00") : "No saved records";

        var record = WeatherService.WeatherFor(site, datePicker.Value);
        if (record == null)
        {
            irradianceValue.Text = temperatureValue.Text = sunshineValue.Text = "-";
            sourceValue.Text = "-";
            statusLabel.Text = "Status: weather data unavailable for this date";
            statusLabel.ForeColor = Color.Black;
            return;
        }

        irradianceValue.Text = Theme.Num(record.SolarIrradiance, "0.00");
        temperatureValue.Text = record.Temperature == 0 ? "-" : Theme.Num(record.Temperature, "0.0");
        sunshineValue.Text = record.SunshineHours == 0 ? "-" : Theme.Num(record.SunshineHours, "0.0");

        bool climatology = record.Date.Year == WeatherRepository.ClimatologyDateYear;
        sourceValue.Text = record.IsSample ? "SAMPLE data (not live)" : climatology ? "NASA POWER monthly climatology" : "Saved weather data";
        statusLabel.Text = record.IsSample ? "Status: sample data loaded" : climatology ? "Status: climatology for selected month" : "Status: loaded";
        statusLabel.ForeColor = record.IsSample ? Color.Black : Theme.SuccessGreen;
    }

    private void Fetch()
    {
        ClassicMessage.Info(this,
            "To search a region and fetch NASA POWER monthly solar climatology, open Locations.",
            "Fetch Weather Data");
    }

    private void LoadSample()
    {
        var site = SelectedSite;
        if (site == null)
        {
            ClassicMessage.Info(this, "Add a location first.", "Load Sample Data");
            return;
        }
        if (WeatherService.WeatherFor(site, datePicker.Value) is { } record &&
            record.Date.Date == datePicker.Value.Date)
        {
            ClassicMessage.Info(this, "A weather record already exists for this location and date.", "Load Sample Data");
            return;
        }

        WeatherRepository.Add(new WeatherData
        {
            Location = site,
            Date = datePicker.Value.Date,
            SolarIrradiance = SolarCalculator.DefaultIrradiance,
            Temperature = 29.5,
            SunshineHours = 6.4,
            IsSample = true
        });
        ShowRecord();
        ClassicMessage.Info(this, "Sample values loaded. They are labelled as sample and are not live weather data.", "Load Sample Data");
    }
}
