namespace SolarWise;

public class HistoryView : ViewBase
{
    private readonly TextBox searchBox = Ui.NewText(new Point(70, 64), 240);
    private readonly ComboBox dateFilter = Ui.NewCombo(new Point(390, 64), 160);
    private readonly ComboBox panelFilter = Ui.NewCombo(new Point(630, 64), 186);
    private readonly ListView list = Ui.NewList(new Point(16, 100), new Size(800, 330),
        ("Date", 90), ("Project", 120), ("Location", 160), ("Solar Panel", 150), ("Area (m²)", 70), ("Output (kWh/yr)", 100), ("Savings (IDR/yr)", 110));

    public HistoryView(MainForm host) : base(host, "Calculation History")
    {
        AddHeader("Calculation History", "Saved calculations for your account in the active organization.");
        Controls.Add(Ui.NewLabel("Search:", new Point(16, 68)));
        Controls.Add(searchBox);
        Controls.Add(Ui.NewLabel("Date:", new Point(340, 68)));
        Controls.Add(dateFilter);
        Controls.Add(Ui.NewLabel("Solar panel:", new Point(560, 68)));
        Controls.Add(panelFilter);
        Controls.Add(list);

        dateFilter.Items.AddRange(new object[] { "All dates", "Last 30 days", "Last 365 days" });
        dateFilter.SelectedIndex = 0;
        panelFilter.Items.Add("All panels");
        foreach (var panel in DataStore.Panels) panelFilter.Items.Add(panel);
        panelFilter.SelectedIndex = 0;

        searchBox.TextChanged += (_, _) => Reload();
        dateFilter.SelectedIndexChanged += (_, _) => Reload();
        panelFilter.SelectedIndexChanged += (_, _) => Reload();
        list.DoubleClick += (_, _) => OpenSelected();

        Controls.Add(Ui.NewButton("Open", new Point(16, 446), (_, _) => OpenSelected(), 110, true));
        Controls.Add(Ui.NewButton("Rename", new Point(134, 446), (_, _) => Rename(), 110));
        Controls.Add(Ui.NewButton("Delete", new Point(252, 446), (_, _) => Delete(), 110));
        Controls.Add(Ui.NewButton("New Calculation", new Point(370, 446), (_, _) => Host.OpenSection("Solar PV Calculator"), 120));
        Controls.Add(Ui.NewButton("Refresh", new Point(498, 446), (_, _) => Reload(), 110));
        Controls.Add(Ui.NewButton("Close", new Point(616, 446), (_, _) => Host.OpenSection("Home"), 110));

        Reload();
    }

    public override void Reload()
    {
        string text = searchBox.Text.Trim();
        int? days = dateFilter.SelectedIndex switch { 1 => 30, 2 => 365, _ => null };
        var panel = panelFilter.SelectedItem as SolarPanel;

        var calcs = DataStore.CalculationsFor(DataStore.CurrentUser).Where(c =>
            (text.Length == 0
             || c.ProjectName.Contains(text, StringComparison.OrdinalIgnoreCase)
             || c.Location.Address.Contains(text, StringComparison.OrdinalIgnoreCase))
            && (days == null || c.CreatedAt >= DateTime.Now.AddDays(-days.Value))
            && (panel == null || c.PanelType == panel));

        list.Items.Clear();
        foreach (var c in calcs)
        {
            Ui.AddRow(list, c,
                Ui.DateText(c.CreatedAt),
                c.ProjectName + (c.IsSample ? " [Sample]" : ""),
                c.Location.Address,
                c.PanelType.Name,
                Theme.Num(c.InputArea, "N1"),
                Theme.Num(c.EstimatedOutput),
                Theme.Idr(c.EstimateSaving));
        }
        if (list.Items.Count == 0)
        {
            Ui.AddRow(list, null, "No calculations match the filters.");
        }
    }

    private void OpenSelected()
    {
        var calc = Ui.Selected<SolarCalculation>(list);
        if (calc == null)
        {
            ClassicMessage.Info(this, "Select a calculation first.", "Calculation History");
            return;
        }
        Host.ShowCalculationDetails(calc);
    }

    private void Rename()
    {
        var calc = Ui.Selected<SolarCalculation>(list);
        if (calc == null)
        {
            ClassicMessage.Info(this, "Select a calculation first.", "Calculation History");
            return;
        }

        using var dialog = new TextInputDialog("Rename Calculation", "Project name:", calc.ProjectName);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (dialog.Value.Length == 0)
        {
            ClassicMessage.Error(this, "Project name cannot be empty.", "Rename Calculation");
            return;
        }

        calc.ProjectName = dialog.Value;
        Reload();
    }

    private void Delete()
    {
        var calc = Ui.Selected<SolarCalculation>(list);
        if (calc == null)
        {
            ClassicMessage.Info(this, "Select a calculation first.", "Calculation History");
            return;
        }
        if (!ClassicMessage.Confirm(this, "Are you sure you want to delete this calculation?", "Delete Calculation")) return;

        DataStore.DeleteCalculation(calc);
        Reload();
    }
}
