namespace SolarWise;

public class ForgotPasswordForm : ClassicForm
{
    private readonly TextBox emailBox = Ui.NewText(new Point(16, 66), 340);

    public ForgotPasswordForm() : base("Forgot Password", 420, 230, isDialog: true)
    {
        var group = Ui.NewGroup("Password Recovery", new Rectangle(16, 12, 388, 140));
        var instructions = Ui.NewLabel(
            "Enter the email address of your account. If it is registered, recovery instructions will be sent to it.",
            new Point(16, 24));
        instructions.MaximumSize = new Size(350, 0);
        group.Controls.Add(instructions);
        group.Controls.Add(emailBox);

        var send = Ui.NewButton("Send Instructions", new Point(170, 170), (_, _) => SendInstructions(), 130, true);
        var cancel = Ui.NewButton("Cancel", new Point(306, 170), (_, _) => Finish(DialogResult.Cancel), 90);
        Content.Controls.Add(group);
        Content.Controls.Add(send);
        Content.Controls.Add(cancel);
        AcceptButton = send;
        CancelButton = cancel;
    }

    private void SendInstructions()
    {
        string email = emailBox.Text.Trim();
        if (!Ui.IsValidEmail(email))
        {
            Ui.Fail("Enter a valid email address.", emailBox);
            return;
        }

        // Prototype: no email service is connected, so nothing is sent.
        ClassicMessage.Info(this,
            "If an account is registered with this address, recovery instructions will be sent.\n\n" +
            "Prototype note: no email service is connected, so no email was actually sent.",
            "Forgot Password");
        Finish(DialogResult.OK);
    }
}
