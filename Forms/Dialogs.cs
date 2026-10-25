namespace SolarWise;

// Add/Edit location. Changes are applied to the object only when Save passes validation.
public class LocationEditDialog : ClassicForm
{
    private readonly SiteLocation site;
    private readonly TextBox addressBox = Ui.NewText(new Point(150, 20), 260);
    private readonly TextBox latBox = Ui.NewText(new Point(150, 52), 160);
    private readonly TextBox lonBox = Ui.NewText(new Point(150, 84), 160);

    public LocationEditDialog(SiteLocation site)
        : base(site.LocationID == 0 ? "Add Location" : "Edit Location", 440, 250, isDialog: true)
    {
        this.site = site;
        Content.Controls.Add(Ui.NewLabel("Address:", new Point(16, 23)));
        Content.Controls.Add(addressBox);
        Content.Controls.Add(Ui.NewLabel("Latitude:", new Point(16, 55)));
        Content.Controls.Add(latBox);
        Content.Controls.Add(Ui.NewLabel("Longitude:", new Point(16, 87)));
        Content.Controls.Add(lonBox);

        var hint = Ui.NewLabel("Latitude -90 to 90, longitude -180 to 180. Use a decimal point, e.g. -7.7523.", new Point(16, 120));
        hint.MaximumSize = new Size(400, 0);
        Content.Controls.Add(hint);

        addressBox.Text = site.Address;
        latBox.Text = site.Latitude;
        lonBox.Text = site.Longitude;

        var save = Ui.NewButton("Save", new Point(240, 180), (_, _) => Save(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(336, 180), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(save);
        Content.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void Save()
    {
        string address = addressBox.Text.Trim();
        if (address.Length == 0) { Ui.Fail("Address is required.", addressBox); return; }
        if (!Ui.TryNumber(latBox.Text, out double lat) || lat < -90 || lat > 90)
        {
            Ui.Fail("Latitude must be a number between -90 and 90.", latBox);
            return;
        }
        if (!Ui.TryNumber(lonBox.Text, out double lon) || lon < -180 || lon > 180)
        {
            Ui.Fail("Longitude must be a number between -180 and 180.", lonBox);
            return;
        }

        site.Address = address;
        site.Latitude = latBox.Text.Trim();
        site.Longitude = lonBox.Text.Trim();
        Finish(DialogResult.OK);
    }
}

// Lets the calculator pick one of the user's saved locations.
public class LocationPickerDialog : ClassicForm
{
    private readonly ListView list = Ui.NewList(new Point(16, 16), new Size(540, 220),
        ("Address", 300), ("Latitude", 110), ("Longitude", 110));

    public LocationPickerDialog() : base("Select Location", 580, 300, isDialog: true)
    {
        foreach (var site in DataStore.LocationsFor(DataStore.CurrentUser))
        {
            Ui.AddRow(list, site, site.Address, site.Latitude, site.Longitude);
        }
        list.DoubleClick += (_, _) => Choose();

        var select = Ui.NewButton("Select", new Point(370, 250), (_, _) => Choose(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(466, 250), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(list);
        Content.Controls.Add(select);
        Content.Controls.Add(cancel);
        AcceptButton = select;
        CancelButton = cancel;
    }

    public SiteLocation Selected { get; private set; }

    private void Choose()
    {
        var site = Ui.Selected<SiteLocation>(list);
        if (site == null)
        {
            ClassicMessage.Info(this, "Select a location first, or cancel and type the coordinates.", "Select Location");
            return;
        }
        Selected = site;
        Finish(DialogResult.OK);
    }
}

// Add/Edit solar panel (administrators only).
public class PanelEditDialog : ClassicForm
{
    private readonly SolarPanel panel;
    private readonly TextBox nameBox = Ui.NewText(new Point(180, 20), 250);
    private readonly TextBox efficiencyBox = Ui.NewText(new Point(180, 52), 110);
    private readonly TextBox priceBox = Ui.NewText(new Point(180, 84), 150);
    private readonly TextBox wattageBox = Ui.NewText(new Point(180, 116), 110);
    private readonly TextBox descriptionBox = Ui.NewText(new Point(180, 148), 250);

    public PanelEditDialog(SolarPanel panel, bool isNew)
        : base(isNew ? "Add Solar Panel" : "Edit Solar Panel", 460, 260, isDialog: true)
    {
        this.panel = panel;
        Content.Controls.Add(Ui.NewLabel("Name:", new Point(16, 23)));
        Content.Controls.Add(nameBox);
        Content.Controls.Add(Ui.NewLabel("Efficiency (%):", new Point(16, 55)));
        Content.Controls.Add(efficiencyBox);
        Content.Controls.Add(Ui.NewLabel("Price per Meter (IDR/m):", new Point(16, 87)));
        Content.Controls.Add(priceBox);
        Content.Controls.Add(Ui.NewLabel("Wattage (Wp):", new Point(16, 119)));
        Content.Controls.Add(wattageBox);
        Content.Controls.Add(Ui.NewLabel("Description:", new Point(16, 151)));
        Content.Controls.Add(descriptionBox);

        nameBox.Text = panel.Name;
        efficiencyBox.Text = panel.Efficiency > 0 ? panel.Efficiency.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
        priceBox.Text = panel.PricePerMeter > 0 ? panel.PricePerMeter.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
        wattageBox.Text = panel.Wattage > 0 ? panel.Wattage.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
        descriptionBox.Text = panel.Description;

        var save = Ui.NewButton("Save", new Point(250, 200), (_, _) => Save(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(346, 200), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(save);
        Content.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void Save()
    {
        string name = nameBox.Text.Trim();
        if (name.Length == 0) { Ui.Fail("Name is required.", nameBox); return; }
        if (!Ui.TryNumber(efficiencyBox.Text, out double efficiency) || efficiency <= 0 || efficiency > 100)
        {
            Ui.Fail("Efficiency must be a number above 0 and up to 100 (percent).", efficiencyBox);
            return;
        }
        if (!Ui.TryNumber(priceBox.Text, out double price) || price < 0)
        {
            Ui.Fail("Price per meter must be a number of 0 or more.", priceBox);
            return;
        }
        if (!Ui.TryNumber(wattageBox.Text, out double wattage) || wattage <= 0)
        {
            Ui.Fail("Wattage must be a number above 0 (Wp).", wattageBox);
            return;
        }

        panel.Name = name;
        panel.Efficiency = efficiency;
        panel.PricePerMeter = price;
        panel.Wattage = wattage;
        panel.Description = descriptionBox.Text.Trim();
        Finish(DialogResult.OK);
    }
}

// Edit user (administrators only). Role and password are not edited here.
public class UserEditDialog : ClassicForm
{
    private readonly User user;
    private readonly TextBox usernameBox = Ui.NewText(new Point(150, 20), 260);
    private readonly TextBox fullNameBox = Ui.NewText(new Point(150, 52), 260);
    private readonly TextBox emailBox = Ui.NewText(new Point(150, 84), 260);

    public UserEditDialog(User user) : base("Edit User", 440, 230, isDialog: true)
    {
        this.user = user;
        Content.Controls.Add(Ui.NewLabel("Username:", new Point(16, 23)));
        Content.Controls.Add(usernameBox);
        Content.Controls.Add(Ui.NewLabel("Full name:", new Point(16, 55)));
        Content.Controls.Add(fullNameBox);
        Content.Controls.Add(Ui.NewLabel("Email address:", new Point(16, 87)));
        Content.Controls.Add(emailBox);
        Content.Controls.Add(Ui.NewLabel($"Role: {user.Role} (read-only here)", new Point(150, 118)));

        usernameBox.Text = user.Username;
        fullNameBox.Text = user.FullName;
        emailBox.Text = user.EmailAddress;

        var save = Ui.NewButton("Save", new Point(240, 170), (_, _) => Save(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(336, 170), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(save);
        Content.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void Save()
    {
        string username = usernameBox.Text.Trim();
        string fullName = fullNameBox.Text.Trim();
        string email = emailBox.Text.Trim();
        if (username.Length == 0) { Ui.Fail("Username is required.", usernameBox); return; }
        if (fullName.Length == 0) { Ui.Fail("Full name is required.", fullNameBox); return; }
        if (!Ui.IsValidEmail(email)) { Ui.Fail("Enter a valid email address.", emailBox); return; }
        if (DataStore.UsernameTaken(username, user)) { Ui.Fail("That username is already taken.", usernameBox); return; }
        if (DataStore.EmailTaken(email, user)) { Ui.Fail("That email is already used by another account.", emailBox); return; }

        user.Username = username;
        user.FullName = fullName;
        user.EmailAddress = email;
        Finish(DialogResult.OK);
    }
}

// Create or edit an organization. PROTOTYPE: not in the ERD.
public class OrganizationEditDialog : ClassicForm
{
    private readonly Organization org;
    private readonly TextBox nameBox = Ui.NewText(new Point(150, 20), 260);
    private readonly TextBox descriptionBox = Ui.NewText(new Point(150, 52), 260);

    public OrganizationEditDialog(Organization org)
        : base(org.OrganizationID == 0 ? "Create Organization" : "Edit Organization", 440, 200, isDialog: true)
    {
        this.org = org;
        Content.Controls.Add(Ui.NewLabel("Organization name:", new Point(16, 23)));
        Content.Controls.Add(nameBox);
        Content.Controls.Add(Ui.NewLabel("Description:", new Point(16, 55)));
        Content.Controls.Add(descriptionBox);

        nameBox.Text = org.Name;
        descriptionBox.Text = org.Description;

        var save = Ui.NewButton(org.OrganizationID == 0 ? "Create" : "Save", new Point(240, 130), (_, _) => Save(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(336, 130), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(save);
        Content.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void Save()
    {
        string name = nameBox.Text.Trim();
        if (name.Length == 0) { Ui.Fail("Organization name is required.", nameBox); return; }
        if (DataStore.Organizations.Any(o => o != org && string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            Ui.Fail("An organization with that name already exists.", nameBox);
            return;
        }

        org.Name = name;
        org.Description = descriptionBox.Text.Trim();
        Finish(DialogResult.OK);
    }
}

// Enter an organization code. Demo codes are sample data.
public class JoinOrganizationDialog : ClassicForm
{
    private readonly TextBox codeBox = Ui.NewText(new Point(16, 46), 380);

    public JoinOrganizationDialog() : base("Join Organization", 420, 230, isDialog: true)
    {
        var group = Ui.NewGroup("Organization Code", new Rectangle(16, 12, 388, 120));
        group.Controls.Add(Ui.NewLabel("Invitation or organization code:", new Point(16, 24)));
        group.Controls.Add(codeBox);
        var hint = Ui.NewLabel("Sample codes: VAYU-2026, UGM-RSCH, PROJ-INT", new Point(16, 80), null, Color.FromArgb(64, 64, 64));
        group.Controls.Add(hint);

        var join = Ui.NewButton("Join", new Point(230, 150), (_, _) => Submit(), 80, true);
        var cancel = Ui.NewButton("Cancel", new Point(316, 150), (_, _) => Finish(DialogResult.Cancel), 80);
        Content.Controls.Add(group);
        Content.Controls.Add(join);
        Content.Controls.Add(cancel);
        AcceptButton = join;
        CancelButton = cancel;
    }

    public string Code => codeBox.Text.Trim();

    private void Submit()
    {
        if (Code.Length == 0)
        {
            Ui.Fail("Enter the organization code.", codeBox);
            return;
        }
        Finish(DialogResult.OK);
    }
}

// Choose the active organization.
public class SelectOrganizationForm : ClassicForm
{
    private readonly ComboBox orgCombo = Ui.NewCombo(new Point(16, 40), 330);

    public SelectOrganizationForm(List<Organization> orgs, Organization current) : base("Select Organization", 380, 190, isDialog: true)
    {
        Content.Controls.Add(Ui.NewLabel("Select the organization to use:", new Point(16, 16)));
        Content.Controls.Add(orgCombo);
        foreach (var org in orgs) orgCombo.Items.Add(org);
        if (current != null && orgs.Contains(current)) orgCombo.SelectedItem = current;
        else if (orgs.Count > 0) orgCombo.SelectedIndex = 0;

        var ok = Ui.NewButton("OK", new Point(176, 100), (_, _) => Confirm(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(272, 100), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(ok);
        Content.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    public Organization SelectedOrganization { get; private set; }

    private void Confirm()
    {
        if (orgCombo.SelectedItem is not Organization org) return;
        SelectedOrganization = org;
        Finish(DialogResult.OK);
    }
}

// Single-value prompt, used for renaming.
public class TextInputDialog : ClassicForm
{
    private readonly TextBox valueBox;

    public TextInputDialog(string title, string prompt, string initial) : base(title, 380, 160, isDialog: true)
    {
        Content.Controls.Add(Ui.NewLabel(prompt, new Point(16, 16)));
        valueBox = Ui.NewText(new Point(16, 42), 340);
        valueBox.Text = initial;
        Content.Controls.Add(valueBox);

        var ok = Ui.NewButton("OK", new Point(196, 92), (_, _) => Finish(DialogResult.OK), 80, true);
        var cancel = Ui.NewButton("Cancel", new Point(282, 92), (_, _) => Finish(DialogResult.Cancel), 80);
        Content.Controls.Add(ok);
        Content.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    public string Value => valueBox.Text.Trim();
}
