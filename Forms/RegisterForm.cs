namespace SolarWise;

public class RegisterForm : ClassicForm
{
    private readonly TextBox fullNameBox = Ui.NewText(new Point(170, 26), 230);
    private readonly TextBox usernameBox = Ui.NewText(new Point(170, 62), 230);
    private readonly TextBox emailBox = Ui.NewText(new Point(170, 98), 230);
    private readonly TextBox passwordBox = Ui.NewText(new Point(170, 134), 230, true);
    private readonly TextBox confirmBox = Ui.NewText(new Point(170, 170), 230, true);

    public RegisterForm() : base("Registration", 460, 330, isDialog: true)
    {
        var group = Ui.NewGroup("Create Account", new Rectangle(16, 12, 428, 262));
        group.Controls.Add(Ui.NewLabel("Full name:", new Point(16, 29)));
        group.Controls.Add(fullNameBox);
        group.Controls.Add(Ui.NewLabel("Username:", new Point(16, 65)));
        group.Controls.Add(usernameBox);
        group.Controls.Add(Ui.NewLabel("Email address:", new Point(16, 101)));
        group.Controls.Add(emailBox);
        group.Controls.Add(Ui.NewLabel("Password:", new Point(16, 137)));
        group.Controls.Add(passwordBox);
        group.Controls.Add(Ui.NewLabel("Confirm password:", new Point(16, 173)));
        group.Controls.Add(confirmBox);

        var hint = Ui.NewLabel("Password: at least 8 characters, with letters and digits.", new Point(16, 210));
        hint.MaximumSize = new Size(400, 0);
        group.Controls.Add(hint);

        var register = Ui.NewButton("Register", new Point(250, 286), (_, _) => Register(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(346, 286), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(group);
        Content.Controls.Add(register);
        Content.Controls.Add(cancel);
        AcceptButton = register;
        CancelButton = cancel;
    }

    public string RegisteredEmail { get; private set; } = "";

    private void Register()
    {
        string fullName = fullNameBox.Text.Trim();
        string username = usernameBox.Text.Trim();
        string email = emailBox.Text.Trim();

        if (fullName.Length == 0) { Ui.Fail("Full name is required.", fullNameBox); return; }
        if (username.Length == 0) { Ui.Fail("Username is required.", usernameBox); return; }
        if (email.Length == 0) { Ui.Fail("Email address is required.", emailBox); return; }
        if (!Ui.IsValidEmail(email)) { Ui.Fail("Enter a valid email address, for example name@example.com.", emailBox); return; }
        if (passwordBox.Text.Length == 0) { Ui.Fail("Password is required.", passwordBox); return; }
        if (!Ui.IsStrongPassword(passwordBox.Text)) { Ui.Fail("Password must be at least 8 characters and contain letters and digits.", passwordBox); return; }
        if (passwordBox.Text != confirmBox.Text) { Ui.Fail("Password and confirmation do not match.", confirmBox); return; }
        if (DataStore.UsernameTaken(username)) { Ui.Fail("That username is already taken.", usernameBox); return; }
        if (DataStore.EmailTaken(email)) { Ui.Fail("An account with that email already exists.", emailBox); return; }

        DataStore.Register(fullName, username, email, passwordBox.Text);
        RegisteredEmail = email;
        ClassicMessage.Info(this, "Account created successfully. You can now log in.", "Registration");
        Finish(DialogResult.OK);
    }
}
