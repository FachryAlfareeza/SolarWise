namespace SolarWise;

public class LoginForm : ClassicForm
{
    private readonly TextBox loginBox = Ui.NewText(new Point(16, 46), 290);
    private readonly TextBox passwordBox = Ui.NewText(new Point(16, 102), 290, true);
    private readonly CheckBox rememberBox = new()
    {
        Text = "Remember me",
        Location = new Point(16, 138),
        AutoSize = true,
        Font = Theme.UiFont,
        BackColor = Color.Transparent
    };
    private readonly ClassicButton loginButton;

    public LoginForm() : base("SolarWise - Login", 560, 330)
    {
        var brand = Ui.NewGroup("", new Rectangle(12, 12, 190, 300));
        brand.Controls.Add(new PictureBox
        {
            Image = Theme.SolarIcon(96),
            Size = new Size(96, 96),
            Location = new Point(47, 36),
            BackColor = Color.Transparent
        });
        brand.Controls.Add(Ui.NewLabel("SolarWise", new Point(30, 150), Theme.HeadingFont));
        var tagline = Ui.NewLabel("Estimate solar energy potential for a better future.", new Point(16, 190));
        tagline.MaximumSize = new Size(160, 0);
        brand.Controls.Add(tagline);

        var signIn = Ui.NewGroup("Sign In", new Rectangle(214, 12, 326, 300));
        signIn.Controls.Add(Ui.NewLabel("Email / Username:", new Point(16, 26)));
        signIn.Controls.Add(loginBox);
        signIn.Controls.Add(Ui.NewLabel("Password:", new Point(16, 82)));
        signIn.Controls.Add(passwordBox);
        signIn.Controls.Add(rememberBox);

        loginButton = Ui.NewButton("Login", new Point(120, 176), (_, _) => Login(), 90, true);
        var cancelButton = Ui.NewButton("Cancel", new Point(216, 176), (_, _) => ClearForm(), 90);
        signIn.Controls.Add(loginButton);
        signIn.Controls.Add(cancelButton);

        var register = new LinkLabel { Text = "Create a New Account", Location = new Point(16, 222), AutoSize = true, Font = Theme.UiFont, LinkColor = Color.Blue };
        register.LinkClicked += (_, _) => OpenRegistration();
        var forgot = new LinkLabel { Text = "Forgot Password?", Location = new Point(16, 248), AutoSize = true, Font = Theme.UiFont, LinkColor = Color.Blue };
        forgot.LinkClicked += (_, _) => OpenForgotPassword();
        signIn.Controls.Add(register);
        signIn.Controls.Add(forgot);

        Content.Controls.Add(brand);
        Content.Controls.Add(signIn);
        AcceptButton = loginButton;
        passwordBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true; };
    }

    private void Login()
    {
        string login = loginBox.Text.Trim();
        string password = passwordBox.Text;
        if (login.Length == 0 || password.Length == 0)
        {
            ClassicMessage.Error(this, "Email/username and password are required.", "Login");
            return;
        }

        var user = DataStore.Authenticate(login, password);
        if (user == null)
        {
            ClassicMessage.Error(this, "Invalid email/username or password.", "Login");
            passwordBox.Clear();
            passwordBox.Focus();
            return;
        }

        DataStore.CurrentUser = user;
        var orgs = DataStore.OrganizationsFor(user);
        if (orgs.Count == 1)
        {
            DataStore.ActiveOrganization = orgs[0];
        }
        else if (orgs.Count > 1)
        {
            using var picker = new SelectOrganizationForm(orgs, null);
            if (picker.ShowDialog(this) != DialogResult.OK)
            {
                DataStore.CurrentUser = null;
                return;
            }
            DataStore.ActiveOrganization = picker.SelectedOrganization;
        }
        else
        {
            // No organization yet: the user can create or join one from the Organization menu.
            DataStore.ActiveOrganization = null;
        }

        Finish(DialogResult.OK);
    }

    private void ClearForm()
    {
        loginBox.Clear();
        passwordBox.Clear();
        rememberBox.Checked = false;
        loginBox.Focus();
    }

    private void OpenRegistration()
    {
        using var register = new RegisterForm();
        if (register.ShowDialog(this) == DialogResult.OK)
        {
            loginBox.Text = register.RegisteredEmail;
            passwordBox.Clear();
            passwordBox.Focus();
        }
    }

    private void OpenForgotPassword()
    {
        using var forgot = new ForgotPasswordForm();
        forgot.ShowDialog(this);
    }
}
