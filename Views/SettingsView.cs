namespace SolarWise;

public class SettingsView : ViewBase
{
    private readonly TextBox fullNameBox = Ui.NewText(new Point(190, 24), 300);
    private readonly TextBox usernameBox = Ui.NewText(new Point(190, 56), 300);
    private readonly TextBox emailBox = Ui.NewText(new Point(190, 88), 300);
    private readonly Label roleValue = Ui.NewLabel("", new Point(190, 122));
    private readonly TextBox currentPasswordBox = Ui.NewText(new Point(190, 24), 300, true);
    private readonly TextBox newPasswordBox = Ui.NewText(new Point(190, 56), 300, true);
    private readonly TextBox confirmPasswordBox = Ui.NewText(new Point(190, 88), 300, true);

    public SettingsView(MainForm host) : base(host, "Settings")
    {
        AddHeader("User Settings", "Changes are kept in memory for this prototype.");

        var profile = Ui.NewGroup("Profile", new Rectangle(16, 70, 800, 152));
        profile.Controls.Add(Ui.NewLabel("Full name:", new Point(20, 27)));
        profile.Controls.Add(fullNameBox);
        profile.Controls.Add(Ui.NewLabel("Username:", new Point(20, 59)));
        profile.Controls.Add(usernameBox);
        profile.Controls.Add(Ui.NewLabel("Email address:", new Point(20, 91)));
        profile.Controls.Add(emailBox);
        profile.Controls.Add(Ui.NewLabel("Role:", new Point(20, 123)));
        profile.Controls.Add(roleValue);
        Controls.Add(profile);

        var password = Ui.NewGroup("Change Password", new Rectangle(16, 236, 800, 150));
        password.Controls.Add(Ui.NewLabel("Current password:", new Point(20, 27)));
        password.Controls.Add(currentPasswordBox);
        password.Controls.Add(Ui.NewLabel("New password:", new Point(20, 59)));
        password.Controls.Add(newPasswordBox);
        password.Controls.Add(Ui.NewLabel("Confirm new password:", new Point(20, 91)));
        password.Controls.Add(confirmPasswordBox);
        var hint = Ui.NewLabel("Leave these blank to keep your current password. New password: at least 8 characters, with letters and digits.",
            new Point(20, 122), null, Color.FromArgb(64, 64, 64));
        hint.MaximumSize = new Size(760, 0);
        password.Controls.Add(hint);
        Controls.Add(password);

        Controls.Add(Ui.NewLabel("Display preferences are not supported in this prototype.", new Point(16, 398), null, Color.FromArgb(64, 64, 64)));
        Controls.Add(Ui.NewButton("Save", new Point(16, 430), (_, _) => Save(), 100, true));
        Controls.Add(Ui.NewButton("Cancel", new Point(124, 430), (_, _) => Reload(), 100));
        Controls.Add(Ui.NewButton("Logout", new Point(716, 430), (_, _) => Host.Logout(), 100));

        Reload();
    }

    public override void Reload()
    {
        var user = DataStore.CurrentUser;
        fullNameBox.Text = user.FullName;
        usernameBox.Text = user.Username;
        emailBox.Text = user.EmailAddress;
        roleValue.Text = user.Role;
        currentPasswordBox.Clear();
        newPasswordBox.Clear();
        confirmPasswordBox.Clear();
    }

    private void Save()
    {
        var user = DataStore.CurrentUser;
        string fullName = fullNameBox.Text.Trim();
        string username = usernameBox.Text.Trim();
        string email = emailBox.Text.Trim();

        if (fullName.Length == 0) { Ui.Fail("Full name is required.", fullNameBox); return; }
        if (username.Length == 0) { Ui.Fail("Username is required.", usernameBox); return; }
        if (!Ui.IsValidEmail(email)) { Ui.Fail("Enter a valid email address.", emailBox); return; }
        if (DataStore.UsernameTaken(username, user)) { Ui.Fail("That username is already taken.", usernameBox); return; }
        if (DataStore.EmailTaken(email, user)) { Ui.Fail("That email is already used by another account.", emailBox); return; }

        bool changingPassword = currentPasswordBox.Text.Length > 0 || newPasswordBox.Text.Length > 0 || confirmPasswordBox.Text.Length > 0;
        if (changingPassword)
        {
            if (DataStore.Hash(currentPasswordBox.Text) != user.PasswordHash) { Ui.Fail("Current password is incorrect.", currentPasswordBox); return; }
            if (!Ui.IsStrongPassword(newPasswordBox.Text)) { Ui.Fail("New password must be at least 8 characters and contain letters and digits.", newPasswordBox); return; }
            if (newPasswordBox.Text != confirmPasswordBox.Text) { Ui.Fail("New password and confirmation do not match.", confirmPasswordBox); return; }
        }

        user.FullName = fullName;
        user.Username = username;
        user.EmailAddress = email;
        if (changingPassword) user.PasswordHash = DataStore.Hash(newPasswordBox.Text);

        Host.RefreshStatus();
        Reload();
        ClassicMessage.Info(this, "Settings saved.", "User Settings");
    }
}
