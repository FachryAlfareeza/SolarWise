namespace SolarWise;

public class CalculatorView : ViewBase
{
    private readonly SolarCalculation prior;
    private readonly Label stepLabel = Ui.NewLabel("", new Point(16, 68), Theme.BoldFont);
    private readonly GroupBox stage1 = Ui.NewGroup("1. Property Location", new Rectangle(16, 94, 800, 330));
    private readonly GroupBox stage2 = Ui.NewGroup("2. Solar Panel and Electricity Information", new Rectangle(16, 94, 800, 330));
    private readonly GroupBox stage3 = Ui.NewGroup("3. Area and Preferences", new Rectangle(16, 94, 800, 330));

    // Stage 1
    private readonly TextBox projectBox = Ui.NewText(new Point(190, 24), 400);
    private readonly TextBox addressBox = Ui.NewReadOnly(new Point(190, 60), 400);
    private readonly TextBox latBox = Ui.NewText(new Point(190, 96), 160);
    private readonly TextBox lonBox = Ui.NewText(new Point(190, 128), 160);

    // Stage 2
    private readonly ComboBox panelCombo = Ui.NewCombo(new Point(190, 24), 420);
    private readonly TextBox wattageBox = Ui.NewReadOnly(new Point(190, 60), 160);
    private readonly TextBox efficiencyBox = Ui.NewReadOnly(new Point(190, 92), 160);
    private readonly TextBox areaPerPanelBox = Ui.NewReadOnly(new Point(190, 124), 160);

    // Stage 3
    private readonly TextBox areaBox = Ui.NewText(new Point(250, 24), 160);
    private readonly Label previewLabel = Ui.NewLabel("Enter an area to preview the layout.", new Point(250, 63));

    private readonly ClassicButton backButton;
    private readonly ClassicButton nextButton;
    private int step = 1;

    public CalculatorView(MainForm host, SolarCalculation prior = null) : base(host, "Solar PV Calculator")
    {
        this.prior = prior;
        AddHeader("Solar PV Calculator", "Estimate the annual output and savings of a rooftop PV system, in three steps.");
        Controls.Add(stepLabel);
        Controls.Add(stage1);
        Controls.Add(stage2);
        Controls.Add(stage3);

        BuildStage1();
        BuildStage2();
        BuildStage3();

        backButton = Ui.NewButton("< Back", new Point(440, 440), (_, _) => Back(), 90);
        nextButton = Ui.NewButton("Next >", new Point(540, 440), (_, _) => Next(), 110, true);
        Controls.Add(backButton);
        Controls.Add(nextButton);
        Controls.Add(Ui.NewButton("Cancel", new Point(660, 440), (_, _) => Cancel(), 90));

        panelCombo.Items.AddRange(DataStore.Panels.ToArray());
        if (prior != null)
        {
            projectBox.Text = prior.ProjectName;
            SetSite(prior.Location);
            panelCombo.SelectedItem = prior.PanelType;
            areaBox.Text = prior.InputArea.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            if (DataStore.SelectedLocation != null) SetSite(DataStore.SelectedLocation);
            if (panelCombo.Items.Count > 0) panelCombo.SelectedIndex = 0;
        }

        panelCombo.SelectedIndexChanged += (_, _) => ShowPanelDetails();
        areaBox.TextChanged += (_, _) => UpdatePreview();
        ShowPanelDetails();
        ShowStep();
    }

    private void BuildStage1()
    {
        stage1.Controls.Add(Ui.NewLabel("Project name:", new Point(20, 27)));
        stage1.Controls.Add(projectBox);
        stage1.Controls.Add(Ui.NewLabel("Address:", new Point(20, 63)));
        stage1.Controls.Add(addressBox);
        stage1.Controls.Add(Ui.NewLabel("Latitude:", new Point(20, 99)));
        stage1.Controls.Add(latBox);
        stage1.Controls.Add(Ui.NewLabel("Longitude:", new Point(20, 131)));
        stage1.Controls.Add(lonBox);
        stage1.Controls.Add(Ui.NewButton("Select Location...", new Point(610, 58), (_, _) => PickLocation(), 150));

        var note = Ui.NewLabel(
            "Use Select Location... to fill coordinates from your saved locations, or type coordinates directly. " +
            "Typed coordinates are used for this calculation only and are not saved as a location.",
            new Point(190, 166), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(580, 0);
        stage1.Controls.Add(note);
    }

    private void BuildStage2()
    {
        stage2.Controls.Add(Ui.NewLabel("Solar panel type:", new Point(20, 27)));
        stage2.Controls.Add(panelCombo);
        stage2.Controls.Add(Ui.NewLabel("Panel wattage (Wp):", new Point(20, 63)));
        stage2.Controls.Add(wattageBox);
        stage2.Controls.Add(Ui.NewLabel("Panel efficiency (%):", new Point(20, 95)));
        stage2.Controls.Add(efficiencyBox);
        stage2.Controls.Add(Ui.NewLabel("Area per panel (m²):", new Point(20, 127)));
        stage2.Controls.Add(areaPerPanelBox);

        var note = Ui.NewLabel(
            "Gray fields come from the catalog and cannot be edited here. The electricity bill input is not part of this prototype.",
            new Point(190, 170), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(580, 0);
        stage2.Controls.Add(note);
    }

    private void BuildStage3()
    {
        stage3.Controls.Add(Ui.NewLabel("Available installation area (m²):", new Point(20, 27)));
        stage3.Controls.Add(areaBox);
        stage3.Controls.Add(Ui.NewLabel("Preview (calculated):", new Point(20, 63)));
        stage3.Controls.Add(previewLabel);

        var note = Ui.NewLabel(
            "Output uses the average saved weather irradiance for the location. Without weather data, a sample value of " +
            $"{Theme.Num(SolarCalculator.DefaultIrradiance, "0.0")} kWh/m²/day is used and the result is marked as sample.",
            new Point(20, 110), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(700, 0);
        stage3.Controls.Add(note);
    }

    private void SetSite(SiteLocation site)
    {
        addressBox.Text = site.Address;
        latBox.Text = site.Latitude;
        lonBox.Text = site.Longitude;
    }

    private void PickLocation()
    {
        using var picker = new LocationPickerDialog();
        if (picker.ShowDialog(FindForm()) == DialogResult.OK && picker.Selected != null)
        {
            SetSite(picker.Selected);
        }
    }

    private void ShowPanelDetails()
    {
        if (panelCombo.SelectedItem is SolarPanel panel)
        {
            wattageBox.Text = Theme.Num(panel.Wattage);
            efficiencyBox.Text = Theme.Num(panel.Efficiency, "0.0");
            areaPerPanelBox.Text = Theme.Num(SolarCalculator.PanelArea(panel), "0.00");
        }
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (panelCombo.SelectedItem is not SolarPanel panel || !Ui.TryNumber(areaBox.Text, out double area) || area <= 0)
        {
            previewLabel.Text = "Enter an area to preview the layout.";
            return;
        }

        int count = SolarCalculator.FindOptimalPanelCount(area, panel);
        previewLabel.Text = count < 1
            ? $"Area is smaller than one panel ({Theme.Num(SolarCalculator.PanelArea(panel), "0.00")} m²)."
            : $"{count} panels, {Theme.Num(count * panel.Wattage / 1000.0, "0.00")} kWp";
    }

    private void ShowStep()
    {
        stage1.Visible = step == 1;
        stage2.Visible = step == 2;
        stage3.Visible = step == 3;
        stepLabel.Text = step switch
        {
            1 => "Step 1 of 3: Property location",
            2 => "Step 2 of 3: Solar panel",
            _ => "Step 3 of 3: Area and calculation"
        };
        backButton.Enabled = step > 1;
        nextButton.Text = step == 3 ? "Calculate" : "Next >";
    }

    private void Back()
    {
        if (step <= 1) return;
        step--;
        ShowStep();
    }

    private void Next()
    {
        switch (step)
        {
            case 1 when !ValidateStage1():
            case 2 when !ValidateStage2():
                return;
            case 3:
                Calculate();
                return;
        }

        step++;
        ShowStep();
    }

    private bool ValidateStage1()
    {
        if (projectBox.Text.Trim().Length == 0) return Ui.Fail("Project name is required.", projectBox);
        if (!Ui.TryNumber(latBox.Text, out double lat) || lat < -90 || lat > 90)
            return Ui.Fail("Latitude must be a number between -90 and 90.", latBox);
        if (!Ui.TryNumber(lonBox.Text, out double lon) || lon < -180 || lon > 180)
            return Ui.Fail("Longitude must be a number between -180 and 180.", lonBox);
        return true;
    }

    private bool ValidateStage2()
    {
        if (panelCombo.SelectedItem == null) return Ui.Fail("Select a solar panel type.", panelCombo);
        return true;
    }

    private void Calculate()
    {
        if (!Ui.TryNumber(areaBox.Text, out double area) || area <= 0)
        {
            Ui.Fail("Installation area must be a positive number in m².", areaBox);
            return;
        }

        var panel = (SolarPanel)panelCombo.SelectedItem;
        if (SolarCalculator.FindOptimalPanelCount(area, panel) < 1)
        {
            Ui.Fail($"The area is smaller than one panel ({Theme.Num(SolarCalculator.PanelArea(panel), "0.00")} m²).", areaBox);
            return;
        }

        var calc = SolarCalculator.GenerateCalculation(projectBox.Text.Trim(), ResolveSite(), panel, area,
            DataStore.CurrentUser, DataStore.ActiveOrganizationId);

        using var results = new CalculationResultsForm(calc);
        if (results.ShowDialog(FindForm()) == DialogResult.Yes)
        {
            Host.ShowView(new RecommendationsView(Host, calc));
        }
    }

    // Matches the typed coordinates to a saved location, or creates a temporary one.
    private SiteLocation ResolveSite()
    {
        string lat = latBox.Text.Trim();
        string lon = lonBox.Text.Trim();
        var saved = DataStore.LocationsFor(DataStore.CurrentUser)
            .FirstOrDefault(l => l.Latitude == lat && l.Longitude == lon);
        if (saved != null) return saved;

        string address = addressBox.Text.Trim();
        return new SiteLocation
        {
            Address = address.Length == 0 ? $"Lat {lat}, Lon {lon}" : address,
            Latitude = lat,
            Longitude = lon
        };
    }

    private void Cancel()
    {
        if (ClassicMessage.Confirm(FindForm(), "Discard this calculation and return to Home?", "Cancel"))
        {
            Host.OpenSection("Home");
        }
    }
}
