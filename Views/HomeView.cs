namespace SolarWise;

public class HomeView : ViewBase
{
    private readonly Label welcomeLabel = Ui.NewLabel("", new Point(16, 12), Theme.HeadingFont);
    private readonly Label orgLabel = Ui.NewLabel("", new Point(18, 42));
    private readonly Label[] statValues = new Label[4];
    private readonly ListView recentList = Ui.NewList(new Point(16, 228), new Size(800, 210),
        ("Date", 80), ("Project", 120), ("Location", 190), ("Solar Panel", 160), ("Est. Output (kWh/yr)", 110), ("Est. Savings (IDR/yr)", 140));

    public HomeView(MainForm host) : base(host, "Home")
    {
        Controls.Add(welcomeLabel);
        Controls.Add(orgLabel);

        string[] titles = { "Total Calculations", "Total Locations", "Total Recommendations", "Estimated Savings / Year" };
        for (int i = 0; i < titles.Length; i++)
        {
            var group = Ui.NewGroup(titles[i], new Rectangle(16 + i * 200, 76, 192, 84));
            statValues[i] = Ui.NewLabel("", new Point(14, 34), Theme.NumberFont);
            group.Controls.Add(statValues[i]);
            Controls.Add(group);
        }

        Controls.Add(Ui.NewLabel("Records marked Sample use demo data. Savings use an assumed electricity tariff.", new Point(16, 170), null, Color.FromArgb(64, 64, 64)));
        Controls.Add(Ui.NewLabel("Recent Calculations", new Point(16, 204), Theme.BoldFont));
        Controls.Add(recentList);
        Controls.Add(Ui.NewButton("New Calculation", new Point(16, 452), (_, _) => Host.OpenSection("Solar PV Calculator"), 150, true));
        Controls.Add(Ui.NewButton("View All", new Point(174, 452), (_, _) => Host.OpenSection("Calculation History"), 100));

        recentList.DoubleClick += (_, _) =>
        {
            var calc = Ui.Selected<SolarCalculation>(recentList);
            if (calc != null) Host.ShowCalculationDetails(calc);
        };

        Reload();
    }

    public override void Reload()
    {
        var user = DataStore.CurrentUser;
        var calcs = DataStore.CalculationsFor(user);

        welcomeLabel.Text = $"Welcome, {user.FullName}";
        orgLabel.Text = DataStore.ActiveOrganization == null
            ? "No active organization. Use the Organization menu to select or join one."
            : $"Active organization: {DataStore.ActiveOrganization.Name}";

        statValues[0].Text = calcs.Count.ToString();
        statValues[1].Text = DataStore.LocationsFor(user).Count.ToString();
        statValues[2].Text = DataStore.RecommendationsFor(user).Count.ToString();
        statValues[3].Text = Theme.Idr(calcs.Sum(c => c.EstimateSaving));

        recentList.Items.Clear();
        foreach (var c in calcs.Take(5))
        {
            Ui.AddRow(recentList, c,
                Ui.DateText(c.CreatedAt),
                c.ProjectName + (c.IsSample ? " [Sample]" : ""),
                c.Location.Address,
                c.PanelType.Name,
                Theme.Num(c.EstimatedOutput),
                Theme.Idr(c.EstimateSaving));
        }
        if (recentList.Items.Count == 0)
        {
            Ui.AddRow(recentList, null, "No calculations yet. Click New Calculation to start.");
        }
    }
}
