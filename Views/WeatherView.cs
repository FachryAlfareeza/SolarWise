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
    private readonly ClassicButton fetchButton;

    public WeatherView(MainForm host) : base(host, "Weather Data")
    {
        AddHeader("Weather Data", "Weather values appear only when retrieved from a data source. No live weather API is connected in this prototype.");

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

        fetchButton = Ui.NewButton("Fetch Weather Data", new Point(16, 356), (_, _) => Fetch(), 150, true);
        Controls.Add(fetchButton);
        Controls.Add(Ui.NewButton("Refresh", new Point(174, 356), (_, _) => ShowRecord(), 90));
        Controls.Add(Ui.NewButton("Load Sample Data", new Point(272, 356), (_, _) => LoadSample(), 140));
        Controls.Add(Ui.NewButton("Close", new Point(420, 356), (_, _) => Host.OpenSection("Home"), 90));

        var note = Ui.NewLabel(
            "Sample data is labelled as sample and is not live weather data. It is used only when a location has no weather record.",
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
        foreach (var site in DataStore.LocationsFor(DataStore.CurrentUser)) locationCombo.Items.Add(site);
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

        var average = DataStore.WeatherRecords.Where(w => w.Location.LocationID == site.LocationID).ToList();
        averageValue.Text = average.Count == 0 ? "No saved records" : Theme.Num(average.Average(w => w.SolarIrradiance), "0.00");

        var record = DataStore.WeatherFor(site, datePicker.Value);
        if (record == null)
        {
            irradianceValue.Text = temperatureValue.Text = sunshineValue.Text = "-";
            sourceValue.Text = "-";
            statusLabel.Text = "Status: weather data unavailable for this date";
            statusLabel.ForeColor = Color.Black;
            return;
        }

        irradianceValue.Text = Theme.Num(record.SolarIrradiance, "0.00");
        temperatureValue.Text = Theme.Num(record.Temperature, "0.0");
        sunshineValue.Text = Theme.Num(record.SunshineHours, "0.0");
        sourceValue.Text = record.IsSample ? "SAMPLE data (not live)" : "Saved weather data";
        statusLabel.Text = record.IsSample ? "Status: sample data loaded" : "Status: loaded";
        statusLabel.ForeColor = record.IsSample ? Color.Black : Theme.SuccessGreen;
    }

    private void Fetch()
    {
        var site = SelectedSite;
        if (site == null)
        {
            ClassicMessage.Info(this, "Add a location first.", "Fetch Weather Data");
            return;
        }

        // Loading state, then the honest result: no weather API is connected.
        statusLabel.Text = "Status: loading...";
        statusLabel.ForeColor = Color.Black;
        Application.DoEvents();

        statusLabel.Text = "Status: unavailable";
        statusLabel.ForeColor = Theme.ErrorRed;
        ClassicMessage.Error(this,
            "Weather data unavailable. No weather API is connected in this prototype, so no live values were retrieved.",
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
        if (DataStore.WeatherFor(site, datePicker.Value) != null)
        {
            ClassicMessage.Info(this, "A weather record already exists for this location and date.", "Load Sample Data");
            return;
        }

        DataStore.WeatherRecords.Add(new WeatherData
        {
            WeatherDataID = DataStore.NextId(),
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
