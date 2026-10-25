namespace SolarWise;

// Showcase of the reusable classic controls and their states.
public class ComponentLibraryForm : ClassicForm
{
    private readonly ToolTip toolTip = new();

    public ComponentLibraryForm() : base("UI Component Library", 900, 620, isDialog: true)
    {
        Content.AutoScroll = true;

        var menu = new MenuStrip { Dock = DockStyle.Top, RenderMode = ToolStripRenderMode.System };
        menu.Items.Add(new ToolStripMenuItem("File"));
        menu.Items.Add(new ToolStripMenuItem("Edit"));
        menu.Items.Add(new ToolStripMenuItem("Help"));
        Content.Controls.Add(menu);

        BuildButtons(Section("Buttons: normal, primary, disabled, pressed, focused", 16, 40, 420, 130));
        BuildInputs(Section("Input fields: text, password, search", 452, 40, 420, 130));
        BuildChoices(Section("Dropdown, checkbox and radio button", 16, 180, 420, 140));
        BuildDialogs(Section("Message dialogs", 452, 180, 420, 140));
        BuildProgress(Section("Progress bar and loading indicator", 16, 330, 420, 110));
        BuildTabs(Section("Tab control", 452, 330, 420, 140));
        BuildList(Section("List view and data table", 16, 450, 420, 160));
        BuildTree(Section("Tree view", 452, 450, 420, 160));
        BuildScrollAndStatus(Section("Scrollbar, tooltip and status bar", 16, 620, 420, 150));
        BuildWindowNotes(Section("Window controls and states", 452, 620, 420, 150));
    }

    private GroupBox Section(string title, int x, int y, int width, int height)
    {
        var group = Ui.NewGroup(title, new Rectangle(x, y, width, height));
        Content.Controls.Add(group);
        return group;
    }

    private static ClassicButton Sample(string text, Point at, int width = 84,
        bool primary = false, bool enabled = true, bool pressed = false, bool focused = false) => new()
    {
        Text = text,
        Location = at,
        Size = new Size(width, 26),
        IsPrimary = primary,
        Enabled = enabled,
        ShowPressed = pressed,
        ShowFocus = focused
    };

    private void BuildButtons(GroupBox group)
    {
        group.Controls.Add(Sample("Normal", new Point(16, 28)));
        group.Controls.Add(Sample("Primary", new Point(108, 28), primary: true));
        group.Controls.Add(Sample("Secondary", new Point(200, 28)));
        group.Controls.Add(Sample("Disabled", new Point(292, 28), enabled: false));
        group.Controls.Add(Sample("Pressed", new Point(16, 64), pressed: true));
        group.Controls.Add(Sample("Focused", new Point(108, 64), focused: true));
        group.Controls.Add(Sample("Primary off", new Point(200, 64), primary: true, enabled: false));
    }

    private void BuildInputs(GroupBox group)
    {
        group.Controls.Add(Ui.NewLabel("Text input:", new Point(16, 30)));
        group.Controls.Add(Ui.NewText(new Point(120, 26), 260));
        group.Controls.Add(Ui.NewLabel("Password:", new Point(16, 62)));
        group.Controls.Add(Ui.NewText(new Point(120, 58), 260, true));
        group.Controls.Add(Ui.NewLabel("Search:", new Point(16, 94)));
        var search = Ui.NewText(new Point(120, 90), 260);
        search.PlaceholderText = "Search...";
        group.Controls.Add(search);
    }

    private void BuildChoices(GroupBox group)
    {
        var combo = Ui.NewCombo(new Point(16, 26), 200);
        combo.Items.AddRange(new object[] { "Option 1", "Option 2", "Option 3" });
        combo.SelectedIndex = 0;
        group.Controls.Add(combo);
        group.Controls.Add(new CheckBox { Text = "Checked", Checked = true, Location = new Point(16, 64), AutoSize = true, BackColor = Color.Transparent, Font = Theme.UiFont });
        group.Controls.Add(new CheckBox { Text = "Unchecked", Location = new Point(16, 94), AutoSize = true, BackColor = Color.Transparent, Font = Theme.UiFont });
        group.Controls.Add(new RadioButton { Text = "Selected", Checked = true, Location = new Point(200, 64), AutoSize = true, BackColor = Color.Transparent, Font = Theme.UiFont });
        group.Controls.Add(new RadioButton { Text = "Not selected", Location = new Point(200, 94), AutoSize = true, BackColor = Color.Transparent, Font = Theme.UiFont });
    }

    private void BuildDialogs(GroupBox group)
    {
        group.Controls.Add(Sample("Information", new Point(16, 28), 92).WithClick((_, _) =>
            ClassicMessage.Show(this, "Data saved successfully.", "Information")));
        group.Controls.Add(Sample("Warning", new Point(116, 28), 92).WithClick((_, _) =>
            ClassicMessage.Show(this, "You have unsaved changes. Do you want to close this window?", "Warning",
                MessageKind.Warning, MessageButtons.YesNoCancel)));
        group.Controls.Add(Sample("Error", new Point(16, 64), 92).WithClick((_, _) =>
            ClassicMessage.Show(this, "An error occurred while saving the data.", "Error",
                MessageKind.Error, MessageButtons.OkRetry)));
        group.Controls.Add(Sample("Confirmation", new Point(116, 64), 92).WithClick((_, _) =>
            ClassicMessage.Show(this, "Are you sure you want to delete this calculation?", "Confirmation",
                MessageKind.Question, MessageButtons.YesNo)));
    }

    private void BuildProgress(GroupBox group)
    {
        group.Controls.Add(new ProgressBar { Location = new Point(16, 28), Size = new Size(380, 20), Value = 75 });
        group.Controls.Add(new ProgressBar { Location = new Point(16, 60), Size = new Size(380, 16), Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30 });
        group.Controls.Add(Ui.NewLabel("Loading indicator (marquee) and determinate progress: 75 %", new Point(16, 82)));
    }

    private void BuildTabs(GroupBox group)
    {
        var tabs = new TabControl { Location = new Point(16, 24), Size = new Size(388, 100), Font = Theme.UiFont };
        tabs.TabPages.Add(new TabPage("Summary"));
        tabs.TabPages.Add(new TabPage("Details"));
        group.Controls.Add(tabs);
    }

    private void BuildList(GroupBox group)
    {
        var list = Ui.NewList(new Point(16, 24), new Size(388, 120), ("Name", 200), ("Status", 180));
        Ui.AddRow(list, null, "Panel Surya A", "Active");
        Ui.AddRow(list, null, "Panel Surya B", "Active");
        Ui.AddRow(list, null, "Inverter", "Inactive");
        Ui.AddRow(list, null, "Battery", "Active");
        list.Items[2].Selected = true;
        group.Controls.Add(list);
    }

    private void BuildTree(GroupBox group)
    {
        var tree = new TreeView { Location = new Point(16, 24), Size = new Size(388, 120), Font = Theme.UiFont };
        var org = tree.Nodes.Add("Organizations");
        var vayu = org.Nodes.Add("Vayuastra");
        vayu.Nodes.Add("Members");
        vayu.Nodes.Add("Projects");
        org.Nodes.Add("UGM Research");
        tree.ExpandAll();
        group.Controls.Add(tree);
    }

    private void BuildScrollAndStatus(GroupBox group)
    {
        group.Controls.Add(new VScrollBar { Location = new Point(16, 28), Size = new Size(17, 60) });
        var tipButton = Sample("Hover me", new Point(50, 28), 100);
        toolTip.SetToolTip(tipButton, "Tooltip: short help text for this control");
        group.Controls.Add(tipButton);

        var status = new StatusStrip { Dock = DockStyle.None, Location = new Point(16, 100), RenderMode = ToolStripRenderMode.System, SizingGrip = false };
        status.Items.Add(new ToolStripStatusLabel("Ready"));
        status.Items.Add(new ToolStripStatusLabel("Organization: Vayuastra"));
        group.Controls.Add(status);
    }

    private void BuildWindowNotes(GroupBox group)
    {
        var notes = Ui.NewLabel(
            "Title bar: minimize and close are shown. Maximize is not used because the layouts are fixed-size prototypes.\n\n" +
            "Status colours: success green, error red, solar yellow for primary actions and key values.",
            new Point(16, 28));
        notes.MaximumSize = new Size(388, 0);
        group.Controls.Add(notes);
    }
}

internal static class ClassicButtonExtensions
{
    // Attaches a click handler and returns the button, so samples fit on one line.
    public static ClassicButton WithClick(this ClassicButton button, EventHandler onClick)
    {
        button.Click += onClick;
        return button;
    }
}
