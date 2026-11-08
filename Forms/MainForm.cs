namespace SolarWise;

public class MainForm : ClassicForm
{
    private sealed class NavItem
    {
        public string Text { get; init; } = "";
        public Func<ViewBase> Create { get; init; }
        public bool AdminOnly { get; init; }
        public override string ToString() => Text;
    }

    private readonly MenuStrip menuStrip = new() { Dock = DockStyle.Top, RenderMode = ToolStripRenderMode.System };
    private readonly ListBox navList = new()
    {
        Dock = DockStyle.Left,
        Width = 176,
        BorderStyle = BorderStyle.Fixed3D,
        Font = Theme.UiFont,
        IntegralHeight = false
    };
    private readonly StatusStrip statusStrip = new() { Dock = DockStyle.Bottom, RenderMode = ToolStripRenderMode.System, SizingGrip = false };
    private readonly ToolStripStatusLabel statusText = new("Prototype: demo data is kept in memory only") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel userStatus = new();
    private readonly ToolStripStatusLabel orgStatus = new();
    private readonly Panel viewHost = new() { Dock = DockStyle.Fill, BackColor = Theme.WindowGray };
    private ViewBase currentView;

    public MainForm() : base("SolarWise", 1000, 640)
    {
        BuildMenu();
        BuildNavigation();

        statusStrip.Items.Add(statusText);
        statusStrip.Items.Add(userStatus);
        statusStrip.Items.Add(orgStatus);

        // Fill first, then the edges, so the docking lays out correctly.
        Content.Controls.Add(viewHost);
        Content.Controls.Add(navList);
        Content.Controls.Add(statusStrip);
        Content.Controls.Add(menuStrip);

        RefreshStatus();
        navList.SelectedIndex = 0;
    }

    public bool LogoutRequested { get; private set; }

    // Opens a navigation section by its label, as if the user had clicked it.
    public void OpenSection(string text)
    {
        var item = navList.Items.Cast<NavItem>().FirstOrDefault(i => i.Text == text);
        if (item == null) return;

        if (navList.SelectedItem == item) ShowView(item.Create());
        else navList.SelectedItem = item;
    }

    // Shows a view in the content area (used for parameterized screens too).
    public void ShowView(ViewBase view)
    {
        viewHost.Controls.Clear();
        currentView = view;
        viewHost.Controls.Add(view);
        statusText.Text = $"{view.Title} - ready";
    }

    public void RefreshStatus()
    {
        var user = DataStore.CurrentUser;
        userStatus.Text = user == null ? "" : $"User: {user.Username} ({user.Role})";
        orgStatus.Text = DataStore.ActiveOrganization == null
            ? "Organization: none"
            : $"Organization: {DataStore.ActiveOrganization.Name}";
    }

    public void OrganizationChanged()
    {
        RefreshStatus();
        currentView?.Reload();
    }

    public void ShowCalculationDetails(SolarCalculation calc)
    {
        using var dialog = new CalculationDetailsDialog(calc);
        var result = dialog.ShowDialog(this);
        if (result == DialogResult.Retry)
        {
            ShowView(new CalculatorView(this, calc));
        }
        else if (result == DialogResult.Yes)
        {
            currentView?.Reload();
        }
    }

    public void Logout()
    {
        if (!ClassicMessage.Confirm(this, "Log out of SolarWise?", "Logout")) return;
        DataStore.CurrentUser = null;
        DataStore.ActiveOrganization = null;
        LogoutRequested = true;
        Close();
    }

    public void SelectOrganization()
    {
        var orgs = DataStore.OrganizationsFor(DataStore.CurrentUser);
        if (orgs.Count == 0)
        {
            ClassicMessage.Info(this, "You are not a member of any organization yet. Create or join one first.", "Select Organization");
            return;
        }

        using var dialog = new SelectOrganizationForm(orgs, DataStore.ActiveOrganization);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        DataStore.ActiveOrganization = dialog.SelectedOrganization;
        OrganizationChanged();
    }

    public bool CreateOrganization()
    {
        var org = new Organization();
        using var dialog = new OrganizationEditDialog(org);
        if (dialog.ShowDialog(this) != DialogResult.OK) return false;

        DataStore.AddOrganization(org, DataStore.CurrentUser);
        ClassicMessage.Info(this, $"Organization \"{org.Name}\" created.\nJoin code for members: {org.JoinCode}", "Create Organization");
        OrganizationChanged();
        return true;
    }

    public bool JoinOrganization()
    {
        using var dialog = new JoinOrganizationDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return false;

        var org = DataStore.FindByJoinCode(dialog.Code);
        if (org == null)
        {
            ClassicMessage.Error(this, "Invalid organization code. Check the code and try again.", "Join Organization");
            return false;
        }
        if (DataStore.RoleIn(DataStore.CurrentUser, org) != "None")
        {
            ClassicMessage.Info(this, $"You are already a member of {org.Name}.", "Join Organization");
            return false;
        }

        DataStore.JoinOrganization(org, DataStore.CurrentUser);
        ClassicMessage.Info(this, $"Joined {org.Name} as Member.", "Join Organization");
        OrganizationChanged();
        return true;
    }

    public void OpenComponentLibrary()
    {
        using var library = new ComponentLibraryForm();
        library.ShowDialog(this);
    }

    private void BuildMenu()
    {
        menuStrip.Items.Add(Menu("File",
            Item("New Calculation", (_, _) => OpenSection("Solar PV Calculator")),
            Item("Open Calculation History", (_, _) => OpenSection("Calculation History")),
            Item("Close Current Window", (_, _) => OpenSection("Home")),
            new ToolStripSeparator(),
            Item("Logout", (_, _) => Logout())));

        menuStrip.Items.Add(Menu("Edit",
            Item("User Profile", (_, _) => OpenSection("Settings")),
            Item("Preferences", (_, _) => OpenSection("Settings"))));

        menuStrip.Items.Add(Menu("Organization",
            Item("Select Organization...", (_, _) => SelectOrganization()),
            Item("Manage Organizations", (_, _) => OpenSection("Organizations")),
            Item("Create Organization...", (_, _) => CreateOrganization()),
            Item("Join Organization...", (_, _) => JoinOrganization())));

        menuStrip.Items.Add(Menu("Help",
            Item("User Guide", (_, _) => ClassicMessage.Info(this,
                "1. Calculate: Solar PV Calculator, then Next through the three steps.\n" +
                "2. Save results from the Calculation Results window.\n" +
                "3. Locations and Solar Panel Catalog store your data (catalog editing is for administrators).",
                "User Guide")),
            Item("Component Library", (_, _) => OpenComponentLibrary()),
            new ToolStripSeparator(),
            Item("About SolarWise", (_, _) => ClassicMessage.Info(this,
                "SolarWise - high-fidelity prototype\nSolar PV potential calculator, academic UI/UX prototype.",
                "About SolarWise"))));
    }

    private void BuildNavigation()
    {
        var items = new List<NavItem>
        {
            new() { Text = "Home", Create = () => new HomeView(this) },
            new() { Text = "Locations", Create = () => new LocationsView(this) },
            new() { Text = "Solar PV Calculator", Create = () => new CalculatorView(this) },
            new() { Text = "Calculation History", Create = () => new HistoryView(this) },
            new() { Text = "Solar Panel Catalog", Create = () => new PanelsView(this) },
            new() { Text = "Weather Data", Create = () => new WeatherView(this) },
            new() { Text = "Recommendations", Create = () => new RecommendationsView(this) },
            new() { Text = "Organizations", Create = () => new OrganizationsView(this) },
            new() { Text = "User Management", Create = () => new UsersView(this), AdminOnly = true },
            new() { Text = "Settings", Create = () => new SettingsView(this) },
        };

        bool isAdmin = DataStore.CurrentUser?.IsAdmin == true;
        foreach (var item in items.Where(i => isAdmin || !i.AdminOnly))
        {
            navList.Items.Add(item);
        }

        navList.SelectedIndexChanged += (_, _) =>
        {
            if (navList.SelectedItem is NavItem item) ShowView(item.Create());
        };
    }

    private static ToolStripMenuItem Menu(string text, params ToolStripItem[] items)
    {
        var menu = new ToolStripMenuItem(text);
        menu.DropDownItems.AddRange(items);
        return menu;
    }

    private static ToolStripMenuItem Item(string text, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += onClick;
        return item;
    }
}
