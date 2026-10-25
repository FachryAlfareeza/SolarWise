namespace SolarWise;

public class RecommendationsView : ViewBase
{
    private readonly ComboBox sourceCombo = Ui.NewCombo(new Point(150, 64), 430);
    private readonly ListView list = Ui.NewList(new Point(16, 100), new Size(800, 200),
        ("Date", 110), ("Project", 200), ("Optimal Area (m²)", 120), ("Est. Output (kWh/yr)", 170), ("Est. Savings (IDR/yr)", 200));
    private readonly Label calcValue = Ui.NewLabel("-", new Point(200, 26));
    private readonly Label areaValue = Ui.NewLabel("-", new Point(200, 48));
    private readonly Label outputValue = Ui.NewLabel("-", new Point(200, 70));
    private readonly Label savingValue = Ui.NewLabel("-", new Point(200, 92));
    private readonly Label summaryValue = new() { Location = new Point(200, 114), MaximumSize = new Size(560, 0), AutoSize = true, Font = Theme.UiFont };

    public RecommendationsView(MainForm host, SolarCalculation focus = null) : base(host, "Recommendations")
    {
        AddHeader("Solar Recommendations", "Recommendations come from saved calculations. Sizing uses the calculated panel count; it is not an engineering certificate.");
        Controls.Add(Ui.NewLabel("Source calculation:", new Point(16, 68)));
        Controls.Add(sourceCombo);
        Controls.Add(Ui.NewButton("Generate Recommendation", new Point(596, 62), (_, _) => Generate(), 200, true));
        Controls.Add(list);

        var summary = Ui.NewGroup("Recommendation Summary", new Rectangle(16, 314, 800, 150));
        summary.Controls.Add(Ui.NewLabel("Related calculation:", new Point(16, 26), Theme.BoldFont));
        summary.Controls.Add(calcValue);
        summary.Controls.Add(Ui.NewLabel("Optimal area:", new Point(16, 48), Theme.BoldFont));
        summary.Controls.Add(areaValue);
        summary.Controls.Add(Ui.NewLabel("Estimated output:", new Point(16, 70), Theme.BoldFont));
        summary.Controls.Add(outputValue);
        summary.Controls.Add(Ui.NewLabel("Estimated savings:", new Point(16, 92), Theme.BoldFont));
        summary.Controls.Add(savingValue);
        summary.Controls.Add(Ui.NewLabel("Summary:", new Point(16, 114), Theme.BoldFont));
        summary.Controls.Add(summaryValue);
        Controls.Add(summary);

        Controls.Add(Ui.NewButton("View Calculation", new Point(16, 476), (_, _) => ViewCalculation(), 140));
        Controls.Add(Ui.NewButton("Close", new Point(164, 476), (_, _) => Host.OpenSection("Home"), 90));

        list.SelectedIndexChanged += (_, _) => ShowSelected();
        list.DoubleClick += (_, _) => ViewCalculation();

        Reload();
        if (focus != null)
        {
            sourceCombo.SelectedItem = focus;
        }
    }

    public override void Reload()
    {
        var user = DataStore.CurrentUser;

        sourceCombo.Items.Clear();
        foreach (var calc in DataStore.CalculationsFor(user)) sourceCombo.Items.Add(calc);
        if (sourceCombo.Items.Count > 0) sourceCombo.SelectedIndex = 0;

        list.Items.Clear();
        foreach (var rec in DataStore.RecommendationsFor(user))
        {
            var calc = DataStore.CalculationFor(rec);
            Ui.AddRow(list, rec,
                Ui.DateText(calc.CreatedAt),
                calc.ProjectName,
                Theme.Num(rec.OptimalArea, "N1"),
                Theme.Num(rec.EstimatedOutput),
                Theme.Idr(rec.EstimatedSaving));
        }
        if (list.Items.Count == 0)
        {
            Ui.AddRow(list, null, sourceCombo.Items.Count == 0
                ? "No saved calculations yet. Save a calculation first."
                : "No recommendations yet. Choose a calculation above and click Generate Recommendation.");
        }
        ShowSelected();
    }

    private void Generate()
    {
        if (sourceCombo.SelectedItem is not SolarCalculation calc)
        {
            ClassicMessage.Info(this, "No saved calculation is available. Save a calculation first.", "Generate Recommendation");
            return;
        }

        var existing = DataStore.RecommendationFor(calc);
        if (existing != null)
        {
            ClassicMessage.Info(this, "This calculation already has a recommendation. It is selected in the list.", "Generate Recommendation");
            Ui.SelectRow(list, existing);
            return;
        }

        var rec = SolarCalculator.GenerateFromCalculation(calc);
        DataStore.AddRecommendation(rec);
        Reload();
        Ui.SelectRow(list, rec);
    }

    private void ViewCalculation()
    {
        var rec = Ui.Selected<Recommendation>(list);
        if (rec == null)
        {
            ClassicMessage.Info(this, "Select a recommendation first.", "Solar Recommendations");
            return;
        }
        Host.ShowCalculationDetails(DataStore.CalculationFor(rec));
    }

    private void ShowSelected()
    {
        var rec = Ui.Selected<Recommendation>(list);
        if (rec == null)
        {
            calcValue.Text = areaValue.Text = outputValue.Text = savingValue.Text = summaryValue.Text = "-";
            return;
        }

        var calc = DataStore.CalculationFor(rec);
        calcValue.Text = calc.ProjectName;
        areaValue.Text = $"{Theme.Num(rec.OptimalArea, "N1")} m²";
        outputValue.Text = $"{Theme.Num(rec.EstimatedOutput)} kWh/year";
        savingValue.Text = $"{Theme.Idr(rec.EstimatedSaving)} / year";
        summaryValue.Text = rec.Summary;
    }
}
