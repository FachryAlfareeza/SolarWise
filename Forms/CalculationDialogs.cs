namespace SolarWise;

// Calculation Results: the summary, a 10-year savings chart and the result actions.
public class CalculationResultsForm : ClassicForm
{
    private readonly SolarCalculation calc;
    private readonly ClassicButton saveButton;

    public CalculationResultsForm(SolarCalculation calc) : base("Calculation Results", 640, 600, isDialog: true)
    {
        this.calc = calc;
        Content.Controls.Add(Ui.NewLabel("Calculation Results", new Point(16, 12), Theme.HeadingFont));
        Content.Controls.Add(Ui.NewLabel($"{calc.ProjectName} - {calc.Location.Address}", new Point(18, 42)));

        var summary = Ui.NewGroup("Recommended Configuration", new Rectangle(16, 66, 600, 222));
        double optimalArea = calc.PanelCount * SolarCalculator.PanelArea(calc.PanelType);
        int y = 24;
        Ui.AddPair(summary, 16, ref y, "Solar panel", calc.PanelType.Name);
        Ui.AddPair(summary, 16, ref y, "Number of panels", calc.PanelCount.ToString());
        Ui.AddPair(summary, 16, ref y, "System capacity", $"{Theme.Num(calc.SystemCapacityKwp, "0.00")} kWp");
        Ui.AddPair(summary, 16, ref y, "Input area", $"{Theme.Num(calc.InputArea, "N1")} m²");
        Ui.AddPair(summary, 16, ref y, "Optimal installation area", $"{Theme.Num(optimalArea, "N1")} m²");
        Ui.AddPair(summary, 16, ref y, "Irradiance used", $"{Theme.Num(calc.Irradiance, "0.00")} kWh/m²/day");
        Ui.AddPair(summary, 16, ref y, "Estimated output", $"{Theme.Num(calc.EstimatedOutput)} kWh/year");
        Ui.AddPair(summary, 16, ref y, "Estimated savings", $"{Theme.Idr(calc.EstimateSaving)} / year");
        Ui.AddPair(summary, 16, ref y, "Data status", calc.IsSample ? "SAMPLE irradiance (no weather data)" : "Saved weather data");
        Content.Controls.Add(summary);

        var chartGroup = Ui.NewGroup("Projected Savings, 10 Years (Rp juta per year)", new Rectangle(16, 300, 600, 186));
        chartGroup.Controls.Add(new SavingsChart
        {
            Bounds = new Rectangle(12, 22, 576, 150),
            Values = SolarCalculator.ProjectSavings(calc.EstimateSaving)
        });
        Content.Controls.Add(chartGroup);

        var note = Ui.NewLabel(
            "Assumptions: performance ratio 80 %, tariff Rp 1.444,70/kWh (assumed; confirm with PLN), output loses 0.5 % per year. " +
            "Payback period and installation cost are not calculated: the ERD has no cost model.",
            new Point(16, 490), null, Color.FromArgb(64, 64, 64));
        note.MaximumSize = new Size(600, 0);
        Content.Controls.Add(note);

        saveButton = Ui.NewButton("Save Calculation...", new Point(16, 530), (_, _) => Save(), 140, true);
        var download = Ui.NewButton("Download Report", new Point(164, 530), (_, _) => ClassicMessage.Info(this,
            "Report export is not implemented in this prototype. No file was created.", "Download Report"), 130);
        var recalculate = Ui.NewButton("Recalculate", new Point(302, 530), (_, _) => Finish(DialogResult.Retry), 100);
        var viewRecommendation = Ui.NewButton("View Recommendation", new Point(410, 530), (_, _) => ViewRecommendation(), 130);
        var close = Ui.NewButton("Close", new Point(548, 530), (_, _) => Finish(DialogResult.Cancel), 80);
        Content.Controls.AddRange(new Control[] { saveButton, download, recalculate, viewRecommendation, close });
        saveButton.Enabled = !calc.IsSaved;
        CancelButton = close;
    }

    private void Save()
    {
        DataStore.SaveCalculation(calc);
        if (!calc.IsSaved) return;

        saveButton.Enabled = false;
        ClassicMessage.Info(this, "Calculation saved to Calculation History.", "Save Calculation");
    }

    private void ViewRecommendation()
    {
        if (!calc.IsSaved && ClassicMessage.Confirm(this,
                "Recommendations are linked to saved calculations. Save this calculation now?", "View Recommendation"))
        {
            Save();
        }
        if (calc.IsSaved) Finish(DialogResult.Yes);
    }
}

// Calculation Details: read-only view of a saved calculation with report, recalculate and delete.
public class CalculationDetailsDialog : ClassicForm
{
    private readonly SolarCalculation calc;

    public CalculationDetailsDialog(SolarCalculation calc) : base("Calculation Details", 540, 430, isDialog: true)
    {
        this.calc = calc;
        var group = Ui.NewGroup("Calculation", new Rectangle(16, 12, 508, 352));
        int y = 24;
        Ui.AddPair(group, 16, ref y, "Project", calc.ProjectName);
        Ui.AddPair(group, 16, ref y, "Date", Ui.DateText(calc.CreatedAt));
        Ui.AddPair(group, 16, ref y, "Location", calc.Location.Address);
        Ui.AddPair(group, 16, ref y, "Coordinates", $"{calc.Location.Latitude}, {calc.Location.Longitude}");
        Ui.AddPair(group, 16, ref y, "Solar panel", calc.PanelType.Name);
        Ui.AddPair(group, 16, ref y, "Input area", $"{Theme.Num(calc.InputArea, "N1")} m²");
        Ui.AddPair(group, 16, ref y, "Number of panels", calc.PanelCount.ToString());
        Ui.AddPair(group, 16, ref y, "System capacity", $"{Theme.Num(calc.SystemCapacityKwp, "0.00")} kWp");
        Ui.AddPair(group, 16, ref y, "Estimated output", $"{Theme.Num(calc.EstimatedOutput)} kWh/year");
        Ui.AddPair(group, 16, ref y, "Estimated savings", $"{Theme.Idr(calc.EstimateSaving)} / year");
        Ui.AddPair(group, 16, ref y, "Weather data", calc.Weather == null ? "None (sample irradiance)" : $"{Ui.DateText(calc.Weather.Date)} (saved)");
        Ui.AddPair(group, 16, ref y, "Recommendation", DataStore.RecommendationFor(calc) == null ? "None yet" : "1 recommendation");
        Ui.AddPair(group, 16, ref y, "Data status", Ui.StatusOf(calc));
        Content.Controls.Add(group);

        var download = Ui.NewButton("Download Report", new Point(16, 380), (_, _) => ClassicMessage.Info(this,
            "Report export is not implemented in this prototype. No file was created.", "Download Report"), 130);
        var recalculate = Ui.NewButton("Recalculate", new Point(154, 380), (_, _) => Finish(DialogResult.Retry), 100);
        var delete = Ui.NewButton("Delete", new Point(262, 380), (_, _) => Delete(), 90);
        var back = Ui.NewButton("Back", new Point(440, 380), (_, _) => Finish(DialogResult.Cancel), 84);
        Content.Controls.AddRange(new Control[] { download, recalculate, delete, back });
        CancelButton = back;
    }

    private void Delete()
    {
        if (!ClassicMessage.Confirm(this, "Are you sure you want to delete this calculation?", "Delete Calculation")) return;
        DataStore.DeleteCalculation(calc);
        Finish(DialogResult.Yes);
    }
}

// Simple bar chart drawn with yellow bars, used for the savings projection.
public sealed class SavingsChart : Panel
{
    public SavingsChart()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.White;
        BorderStyle = BorderStyle.Fixed3D;
        Font = Theme.UiFont;
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public double[] Values { get; set; } = Array.Empty<double>();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Values.Length == 0) return;

        Graphics g = e.Graphics;
        const int left = 10;
        const int top = 22;
        int bottom = Height - 22;
        double max = Values.Max();
        if (max <= 0) max = 1;

        int slot = (Width - left - 10) / Values.Length;
        int barWidth = slot - 12;
        using var brush = new SolidBrush(Theme.SolarYellow);

        for (int i = 0; i < Values.Length; i++)
        {
            int h = (int)((bottom - top) * (Values[i] / max));
            int x = left + i * slot + 6;
            var rect = new Rectangle(x, bottom - h, barWidth, h);
            g.FillRectangle(brush, rect);
            g.DrawRectangle(Pens.Black, rect);

            TextRenderer.DrawText(g, (i + 1).ToString(), Font, new Point(x + barWidth / 2 - 4, bottom + 4), Color.Black);
            TextRenderer.DrawText(g, Theme.Num(Values[i] / 1_000_000, "0.0"), Font, new Point(x, bottom - h - 16), Color.Black);
        }

        g.DrawLine(Pens.Black, left, bottom, Width - 10, bottom);
    }
}
