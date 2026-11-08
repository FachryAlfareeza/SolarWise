namespace SolarWise;

public class UsersView : ViewBase
{
    private readonly ListView list = Ui.NewList(new Point(16, 70), new Size(800, 300),
        ("User ID", 80), ("Username", 150), ("Full Name", 200), ("Email Address", 260), ("Role", 100));
    private readonly ClassicButton viewButton;
    private readonly ClassicButton editButton;

    public UsersView(MainForm host) : base(host, "User Management")
    {
        bool isAdmin = DataStore.CurrentUser.IsAdmin;
        AddHeader("User Management", isAdmin
            ? "Administrators only. Passwords are never displayed."
            : "Access denied: this screen needs the administrator role.");
        Controls.Add(list);

        viewButton = Ui.NewButton("View User", new Point(16, 390), (_, _) => ViewUser(), 110);
        editButton = Ui.NewButton("Edit User...", new Point(134, 390), (_, _) => EditUser(), 110, true);
        Controls.Add(viewButton);
        Controls.Add(editButton);
        Controls.Add(Ui.NewButton("Refresh", new Point(252, 390), (_, _) => Reload(), 90));
        Controls.Add(Ui.NewButton("Close", new Point(350, 390), (_, _) => Host.OpenSection("Home"), 90));

        viewButton.Enabled = isAdmin;
        editButton.Enabled = isAdmin;
        list.DoubleClick += (_, _) => ViewUser();
        Reload();
    }

    public override void Reload()
    {
        list.Items.Clear();
        if (!DataStore.CurrentUser.IsAdmin) return;

        foreach (var user in DataStore.Users)
        {
            Ui.AddRow(list, user, user.UserID.ToString(), user.Username, user.FullName, user.EmailAddress, user.Role);
        }
    }

    private void ViewUser()
    {
        var user = Ui.Selected<User>(list);
        if (user == null)
        {
            ClassicMessage.Info(this, "Select a user first.", "View User");
            return;
        }

        // Password hash is deliberately left out.
        ClassicMessage.Info(this,
            $"User ID: {user.UserID}\nUsername: {user.Username}\nFull name: {user.FullName}\n" +
            $"Email address: {user.EmailAddress}\nRole: {user.Role}",
            "View User");
    }

    private void EditUser()
    {
        var user = Ui.Selected<User>(list);
        if (user == null)
        {
            ClassicMessage.Info(this, "Select a user first.", "Edit User");
            return;
        }

        using var dialog = new UserEditDialog(user);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Reload();
        ClassicMessage.Info(this, "User updated.", "Edit User");
    }
}
