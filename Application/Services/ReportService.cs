using System.Net;
using System.Text;

namespace SolarWise;

// Writes a calculation report as an HTML page (open in a browser, print to PDF) or plain text.
public static class ReportService
{
    // Asks for a file name and writes the report. Returns false when the user cancels or writing fails.
    public static bool Download(IWin32Window owner, SolarCalculation calc)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Download Report",
            Filter = "HTML report (*.html)|*.html|Text report (*.txt)|*.txt",
            FileName = SafeFileName($"SolarWise Report - {calc.ProjectName}"),
            AddExtension = true,
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(owner) != DialogResult.OK) return false;

        try
        {
            bool text = Path.GetExtension(dialog.FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
            File.WriteAllText(dialog.FileName, text ? BuildText(calc) : BuildHtml(calc), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            ClassicMessage.Error(owner, "The report could not be saved:\n" + ex.Message, "Download Report");
            return false;
        }

        ClassicMessage.Info(owner, "Report saved to:\n" + dialog.FileName, "Download Report");
        return true;
    }

    private static List<(string Label, string Value)> Rows(SolarCalculation calc) => new()
    {
        ("Project", calc.ProjectName),
        ("Date", Ui.DateText(calc.CreatedAt)),
        ("Location", calc.Location.Address),
        ("Coordinates", $"{calc.Location.Latitude}, {calc.Location.Longitude}"),
        ("Solar panel", calc.PanelType.Name),
        ("Number of panels", calc.PanelCount.ToString()),
        ("System capacity", $"{Theme.Num(calc.SystemCapacityKwp, "0.00")} kWp"),
        ("Input area", $"{Theme.Num(calc.InputArea, "N1")} m²"),
        ("Optimal installation area", $"{Theme.Num(calc.PanelCount * SolarCalculator.PanelArea(calc.PanelType), "N1")} m²"),
        ("Irradiance used", $"{Theme.Num(calc.Irradiance, "0.00")} kWh/m²/day"),
        ("Estimated output", $"{Theme.Num(calc.EstimatedOutput)} kWh/year"),
        ("Estimated savings", $"{Theme.Idr(calc.EstimateSaving)} / year"),
        ("Data status", calc.IsSample ? "Sample irradiance (no weather data)" : "Weather data"),
    };

    private const string Assumptions =
        "Assumptions: performance ratio 80 %, tariff Rp 1.444,70/kWh (assumed; confirm with PLN), output loses 0.5 % per year. " +
        "Payback period and installation cost are not calculated.";

    private static string BuildText(SolarCalculation calc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SOLARWISE CALCULATION REPORT");
        sb.AppendLine(new string('=', 40));
        foreach (var (label, value) in Rows(calc)) sb.AppendLine($"{label,-28}{value}");
        sb.AppendLine();
        sb.AppendLine("Projected savings, 10 years");
        var savings = SolarCalculator.ProjectSavings(calc.EstimateSaving);
        for (int i = 0; i < savings.Length; i++) sb.AppendLine($"  Year {i + 1,2}: {Theme.Idr(savings[i])}");
        sb.AppendLine($"  Total  : {Theme.Idr(savings.Sum())}");
        sb.AppendLine();
        sb.AppendLine(Assumptions);
        sb.AppendLine($"Generated {DateTime.Now:dd MMM yyyy HH:mm}");
        return sb.ToString();
    }

    private static string BuildHtml(SolarCalculation calc)
    {
        static string E(string s) => WebUtility.HtmlEncode(s ?? "");
        var savings = SolarCalculator.ProjectSavings(calc.EstimateSaving);
        double max = Math.Max(savings.DefaultIfEmpty(0).Max(), 1);

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>SolarWise Report - ").Append(E(calc.ProjectName)).Append("</title>");
        sb.Append("<style>body{font-family:Tahoma,Arial,sans-serif;margin:32px;color:#222}h1{margin:0 0 4px}" +
                  "table{border-collapse:collapse;margin:12px 0;min-width:480px}td,th{border:1px solid #999;padding:6px 10px;text-align:left}" +
                  "th{background:#eee}.bar{background:#f5c518;border:1px solid #000;height:14px}.muted{color:#555;font-size:12px}</style></head><body>");
        sb.Append("<h1>SolarWise Calculation Report</h1>");
        sb.Append("<p class=\"muted\">Generated ").Append(E(DateTime.Now.ToString("dd MMM yyyy HH:mm"))).Append("</p>");
        sb.Append("<h2>Recommended configuration</h2><table>");
        foreach (var (label, value) in Rows(calc)) sb.Append("<tr><th>").Append(E(label)).Append("</th><td>").Append(E(value)).Append("</td></tr>");
        sb.Append("</table><h2>Projected savings, 10 years</h2><table><tr><th>Year</th><th>Savings</th><th></th></tr>");
        for (int i = 0; i < savings.Length; i++)
        {
            int width = (int)(240 * savings[i] / max);
            sb.Append("<tr><td>").Append(i + 1).Append("</td><td>").Append(E(Theme.Idr(savings[i])))
              .Append("</td><td><div class=\"bar\" style=\"width:").Append(width).Append("px\"></div></td></tr>");
        }
        sb.Append("<tr><th>Total</th><th>").Append(E(Theme.Idr(savings.Sum()))).Append("</th><th></th></tr></table>");
        sb.Append("<p class=\"muted\">").Append(E(Assumptions)).Append("</p></body></html>");
        return sb.ToString();
    }

    private static string SafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name;
    }
}
