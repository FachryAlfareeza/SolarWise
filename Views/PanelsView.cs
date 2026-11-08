namespace SolarWise;

public class PanelsView : ViewBase
{
    private readonly ListView list = Ui.NewList(new Point(16, 70), new Size(800, 300),
        ("Name", 230), ("Efficiency (%)", 110), ("Price per Meter (IDR/m)", 170), ("Wattage (Wp)", 100), ("Description", 190));
    private readonly ClassicButton addButton;
    private readonly ClassicButton editButton;
    private readonly ClassicButton deleteButton;

    public PanelsView(MainForm host) : base(host, "Solar Panel Catalog")
    {
        AddHeader("Solar Panel Catalog", "Panels the calculator can use. Catalog entries marked [Sample] are demo data.");
        Controls.Add(list);

        addButton = Ui.NewButton("Add Solar Panel...", new Point(16, 390), (_, _) => AddPanel(), 150, true);
        editButton = Ui.NewButton("Edit...", new Point(174, 390), (_, _) => EditPanel(), 90);
        deleteButton = Ui.NewButton("Delete", new Point(272, 390), (_, _) => DeletePanel(), 90);
        Controls.Add(addButton);
        Controls.Add(editButton);
        Controls.Add(deleteButton);
        Controls.Add(Ui.NewButton("View Details", new Point(370, 390), (_, _) => ViewDetails(), 110));
        Controls.Add(Ui.NewButton("Refresh", new Point(488, 390), (_, _) => Reload(), 90));
        Controls.Add(Ui.NewButton("Close", new Point(586, 390), (_, _) => Host.OpenSection("Home"), 90));

        bool canEdit = DataStore.CurrentUser.IsAdmin;
        addButton.Enabled = canEdit;
        editButton.Enabled = canEdit;
        deleteButton.Enabled = canEdit;
        if (!canEdit)
        {
            Controls.Add(Ui.NewLabel("Read-only: only administrators can add, edit or delete catalog entries.", new Point(16, 430), null, Color.FromArgb(64, 64, 64)));
        }

        list.DoubleClick += (_, _) => ViewDetails();
        Reload();
    }

    public override void Reload()
    {
        list.Items.Clear();
        foreach (var panel in DataStore.Panels)
        {
            Ui.AddRow(list, panel,
                panel.Name,
                Theme.Num(panel.Efficiency, "0.0"),
                Theme.Num(panel.PricePerMeter),
                Theme.Num(panel.Wattage),
                panel.Description);
        }
    }

    private void AddPanel()
    {
        var panel = new SolarPanel();
        using var dialog = new PanelEditDialog(panel, isNew: true);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        DataStore.AddPanel(panel);
        Reload();
    }

    private void EditPanel()
    {
        var panel = Ui.Selected<SolarPanel>(list);
        if (panel == null)
        {
            ClassicMessage.Info(this, "Select a panel first.", "Solar Panel Catalog");
            return;
        }

        using var dialog = new PanelEditDialog(panel, isNew: false);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Reload();
    }

    private void DeletePanel()
    {
        var panel = Ui.Selected<SolarPanel>(list);
        if (panel == null)
        {
            ClassicMessage.Info(this, "Select a panel first.", "Solar Panel Catalog");
            return;
        }
        if (!ClassicMessage.Confirm(this, $"Delete \"{panel.Name}\" from the catalog? Saved calculations keep their results.", "Delete Solar Panel")) return;

        DataStore.DeletePanel(panel);
        Reload();
    }

    private void ViewDetails()
    {
        var panel = Ui.Selected<SolarPanel>(list);
        if (panel == null)
        {
            ClassicMessage.Info(this, "Select a panel first.", "Solar Panel Catalog");
            return;
        }

        ClassicMessage.Info(this,
            $"Name: {panel.Name}\n" +
            $"Efficiency: {Theme.Num(panel.Efficiency, "0.0")} %\n" +
            $"Wattage: {Theme.Num(panel.Wattage)} Wp\n" +
            $"Area per panel (calculated): {Theme.Num(SolarCalculator.PanelArea(panel), "0.00")} m²\n" +
            $"Price per meter: {Theme.Idr(panel.PricePerMeter)} (unit to confirm)\n" +
            $"Description: {panel.Description}",
            "Panel Details");
    }
}
