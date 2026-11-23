namespace SolarWise;

// Edit > Preferences: settings for this computer (window and remembered login), not the user profile.
public class PreferencesDialog : ClassicForm
{
    private readonly CheckBox startMaximizedBox = new()
    {
        Text = "Open the main window maximized after login",
        Location = new Point(16, 26),
        AutoSize = true,
        Font = Theme.UiFont,
        BackColor = Color.Transparent
    };
    private readonly Label rememberedValue = Ui.NewLabel("", new Point(16, 28));
    private readonly ClassicButton forgetButton;

    public PreferencesDialog() : base("Preferences", 460, 260, isDialog: true)
    {
        var window = Ui.NewGroup("Window", new Rectangle(16, 12, 428, 84));
        window.Controls.Add(startMaximizedBox);
        window.Controls.Add(Ui.NewLabel("Drag the edges to resize; double-click the title bar to maximize.",
            new Point(16, 54), null, Color.FromArgb(64, 64, 64)));
        Content.Controls.Add(window);

        var login = Ui.NewGroup("Remember me", new Rectangle(16, 106, 428, 96));
        login.Controls.Add(rememberedValue);
        forgetButton = Ui.NewButton("Forget", new Point(16, 54), (_, _) => Forget(), 90);
        login.Controls.Add(forgetButton);
        login.Controls.Add(Ui.NewLabel("Passwords are never saved.", new Point(116, 59), null, Color.FromArgb(64, 64, 64)));
        Content.Controls.Add(login);

        var save = Ui.NewButton("Save", new Point(258, 216), (_, _) => Save(), 90, true);
        var cancel = Ui.NewButton("Cancel", new Point(354, 216), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(save);
        Content.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;

        startMaximizedBox.Checked = LocalSettings.StartMaximized;
        ShowRemembered();
    }

    private void ShowRemembered()
    {
        string remembered = LocalSettings.RememberedLogin;
        rememberedValue.Text = remembered == null ? "No login is remembered on this computer." : $"Remembered login: {remembered}";
        forgetButton.Enabled = remembered != null;
    }

    private void Forget()
    {
        LocalSettings.RememberedLogin = null;
        ShowRemembered();
    }

    private void Save()
    {
        LocalSettings.StartMaximized = startMaximizedBox.Checked;
        Finish(DialogResult.OK);
    }
}
