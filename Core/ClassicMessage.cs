namespace SolarWise;

public enum MessageKind { Information, Warning, Error, Question }

public enum MessageButtons { Ok, OkRetry, YesNo, YesNoCancel }

// Classic message boxes that share the application's title bar and button style.
public static class ClassicMessage
{
    public static DialogResult Show(IWin32Window owner, string text, string title = "SolarWise",
        MessageKind kind = MessageKind.Information, MessageButtons buttons = MessageButtons.Ok)
    {
        using var dialog = new MessageForm(text, title, kind, buttons);
        return owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
    }

    public static void Info(IWin32Window owner, string text, string title = "SolarWise") =>
        Show(owner, text, title);

    public static void Error(IWin32Window owner, string text, string title = "Error") =>
        Show(owner, text, title, MessageKind.Error);

    public static bool Confirm(IWin32Window owner, string text, string title = "Confirm") =>
        Show(owner, text, title, MessageKind.Question, MessageButtons.YesNo) == DialogResult.Yes;

    private sealed class MessageForm : ClassicForm
    {
        public MessageForm(string text, string title, MessageKind kind, MessageButtons buttons)
            : base(title, 420, 170, isDialog: true)
        {
            Content.Controls.Add(new PictureBox
            {
                Image = kind switch
                {
                    MessageKind.Warning => SystemIcons.Warning.ToBitmap(),
                    MessageKind.Error => SystemIcons.Error.ToBitmap(),
                    MessageKind.Question => SystemIcons.Question.ToBitmap(),
                    _ => SystemIcons.Information.ToBitmap()
                },
                Location = new Point(16, 18),
                Size = new Size(32, 32),
                BackColor = Color.Transparent
            });

            var message = Ui.NewLabel(text, new Point(64, 18));
            message.MaximumSize = new Size(330, 0);
            Content.Controls.Add(message);

            var choices = buttons switch
            {
                MessageButtons.OkRetry => new[] { ("OK", DialogResult.OK), ("Retry", DialogResult.Retry) },
                MessageButtons.YesNo => new[] { ("Yes", DialogResult.Yes), ("No", DialogResult.No) },
                MessageButtons.YesNoCancel => new[] { ("Yes", DialogResult.Yes), ("No", DialogResult.No), ("Cancel", DialogResult.Cancel) },
                _ => new[] { ("OK", DialogResult.OK) }
            };

            int area = ClientSize.Width - 6;
            int x = area - choices.Length * 86 + 6 - 12;
            var created = new List<ClassicButton>();
            foreach (var (label, result) in choices)
            {
                var button = Ui.NewButton(label, new Point(x, 126), (_, _) => Finish(result), 80);
                Content.Controls.Add(button);
                created.Add(button);
                x += 86;
            }

            AcceptButton = created[0];
            CancelButton = created[^1];
        }
    }
}
