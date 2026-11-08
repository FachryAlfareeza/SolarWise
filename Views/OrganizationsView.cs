namespace SolarWise;

// PROTOTYPE: organizations are not part of the supplied ERD. See the banner on this screen.
public class OrganizationsView : ViewBase
{
    private readonly ListView list = Ui.NewList(new Point(16, 96), new Size(800, 260),
        ("Organization", 170), ("User Role", 110), ("Member Count", 100), ("Status", 110), ("Description", 310));
    private readonly ClassicButton editButton;
    private readonly ClassicButton deleteButton;
    private readonly ClassicButton selectButton;

    public OrganizationsView(MainForm host) : base(host, "Organizations")
    {
        AddHeader("Manage Organizations", "");
        Controls.Add(Ui.NewLabel(
            "PROTOTYPE: organizations are not in the supplied ERD. Data is kept in memory and resets when the application closes.",
            new Point(18, 66), null, Theme.ErrorRed));
        Controls.Add(list);

        Controls.Add(Ui.NewButton("Create Organization...", new Point(16, 366), (_, _) => { if (Host.CreateOrganization()) Reload(); }, 160, true));
        Controls.Add(Ui.NewButton("Join Organization...", new Point(184, 366), (_, _) => { if (Host.JoinOrganization()) Reload(); }, 150));
        editButton = Ui.NewButton("Edit...", new Point(342, 366), (_, _) => EditOrganization(), 90);
        deleteButton = Ui.NewButton("Delete", new Point(440, 366), (_, _) => DeleteOrganization(), 90);
        selectButton = Ui.NewButton("Select Organization", new Point(538, 366), (_, _) => SelectOrganization(), 140);
        Controls.Add(editButton);
        Controls.Add(deleteButton);
        Controls.Add(selectButton);
        Controls.Add(Ui.NewButton("Close", new Point(686, 366), (_, _) => Host.OpenSection("Home"), 90));

        Controls.Add(Ui.NewLabel("Edit and Delete need the Admin role in that organization (or the system administrator role).",
            new Point(16, 404), null, Color.FromArgb(64, 64, 64)));

        list.SelectedIndexChanged += (_, _) => UpdateButtons();
        list.DoubleClick += (_, _) => EditOrganization();
        Reload();
    }

    public override void Reload()
    {
        var user = DataStore.CurrentUser;
        list.Items.Clear();
        foreach (var org in DataStore.OrganizationsFor(user))
        {
            string status = org == DataStore.ActiveOrganization ? "Active" : "Available";
            Ui.AddRow(list, org,
                org.Name,
                DataStore.RoleIn(user, org),
                DataStore.MemberCount(org).ToString(),
                status,
                org.Description);
        }
        if (list.Items.Count == 0)
        {
            Ui.AddRow(list, null, "No organizations yet. Create one or join with a code.");
        }
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var org = Ui.Selected<Organization>(list);
        bool canManage = org != null && DataStore.CanManage(DataStore.CurrentUser, org);
        editButton.Enabled = canManage;
        deleteButton.Enabled = canManage;
        selectButton.Enabled = org != null;
    }

    private void EditOrganization()
    {
        var org = Ui.Selected<Organization>(list);
        if (org == null || !DataStore.CanManage(DataStore.CurrentUser, org)) return;

        using var dialog = new OrganizationEditDialog(org);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Host.OrganizationChanged();
    }

    private void DeleteOrganization()
    {
        var org = Ui.Selected<Organization>(list);
        if (org == null || !DataStore.CanManage(DataStore.CurrentUser, org)) return;
        if (!ClassicMessage.Confirm(this, $"Delete organization \"{org.Name}\"? All memberships will be removed.", "Delete Organization")) return;

        DataStore.DeleteOrganization(org);
        Host.OrganizationChanged();
    }

    private void SelectOrganization()
    {
        var org = Ui.Selected<Organization>(list);
        if (org == null) return;

        DataStore.ActiveOrganization = org;
        ClassicMessage.Info(this, $"Active organization: {org.Name}", "Select Organization");
        Host.OrganizationChanged();
    }
}
