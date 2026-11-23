namespace SolarWise;

public class HomeView : ViewBase
{
    private const int CardCount = 2;

    private readonly Label welcomeLabel = Ui.NewLabel("", new Point(16, 12), Theme.HeadingFont);
    private readonly Label[] statValues = new Label[CardCount];
    private readonly GroupBox[] cards = new GroupBox[CardCount];
    private Label footnote;
    private ClassicButton newButton, viewAll;
    private readonly ListView recentList = Ui.NewList(new Point(16, 188), new Size(800, 240),
        ("Date", 80), ("Project", 130), ("Location", 190), ("Solar Panel", 160), ("Est. Output (kWh/yr)", 120), ("Est. Savings (IDR/yr)", 120));

    public HomeView(MainForm host) : base(host, "Home")
    {
        Controls.Add(welcomeLabel);
        Controls.Add(Ui.NewLabel("Overview of your solar calculations.", new Point(18, 42), null, Color.FromArgb(48, 48, 48)));

        // Summary cards share the full width equally and follow the window size.
        string[] titles = { "Total Calculations", "Total Locations" };
        for (int i = 0; i < CardCount; i++)
        {
            cards[i] = Ui.NewGroup(titles[i], new Rectangle(16, 72, 250, 76));
            statValues[i] = Ui.NewLabel("", new Point(14, 32), Theme.NumberFont);
            cards[i].Controls.Add(statValues[i]);

            // Quick-add button on the right edge of the card; it follows the card when resized.
            string section = i == 0 ? "Solar PV Calculator" : "Locations";
            var add = Ui.NewButton(i == 0 ? "Add Calculation" : "Add Location", new Point(cards[i].Width - 132, 30),
                (_, _) => Host.OpenSection(section), 120, true);
            add.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cards[i].Controls.Add(add);
            Controls.Add(cards[i]);
        }

        Controls.Add(Ui.NewLabel("Recent Calculations", new Point(16, 160), Theme.BoldFont));
        Controls.Add(recentList);
        footnote = Ui.NewLabel("Records marked Sample use demo data. Savings use an assumed electricity tariff.", new Point(16, 440), null, Color.FromArgb(64, 64, 64));
        Controls.Add(footnote);

        newButton = Ui.NewButton("New Calculation", new Point(16, 468), (_, _) => Host.OpenSection("Solar PV Calculator"), 150, true);
        viewAll = Ui.NewButton("View All", new Point(174, 468), (_, _) => Host.OpenSection("Calculation History"), 100);
        Controls.Add(newButton);
        Controls.Add(viewAll);

        // Positions are computed from the real size on every resize. Anchors are not used here:
        // they were captured while the view still had its default 150x150 size, which pushed the
        // list and the buttons below the visible area.
        Resize += (_, _) => LayoutView();
        LayoutView();

        recentList.DoubleClick += (_, _) =>
        {
            var calc = Ui.Selected<SolarCalculation>(recentList);
            if (calc != null) Host.ShowCalculationDetails(calc);
        };

        Reload();
    }

    private void LayoutView()
    {
        const int margin = 16, gap = 12;
        int width = Math.Max(120, (ClientSize.Width - margin * 2 - gap * (CardCount - 1)) / CardCount);
        for (int i = 0; i < CardCount; i++)
        {
            cards[i].SetBounds(margin + i * (width + gap), 72, width, 76);
        }
        recentList.Width = Math.Max(300, ClientSize.Width - margin * 2);

        // Buttons sit on the bottom edge, the note above them, and the list takes the space left.
        int buttonTop = Math.Max(recentList.Top + 80 + footnote.Height + 20, ClientSize.Height - margin - newButton.Height);
        newButton.Top = viewAll.Top = buttonTop;
        footnote.Top = buttonTop - footnote.Height - 10;
        recentList.Height = Math.Max(80, footnote.Top - 10 - recentList.Top);
    }

    public override void Reload()
    {
        var user = SessionContext.CurrentUser;
        var calcs = CalculationRepository.CalculationsFor(user);

        welcomeLabel.Text = $"Welcome, {user.FullName}";

        statValues[0].Text = calcs.Count.ToString();
        statValues[1].Text = LocationRepository.LocationsFor(user).Count.ToString();

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
