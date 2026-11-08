namespace SolarWise;

// Base for the content screens shown inside the main window.
public abstract class ViewBase : UserControl
{
    protected ViewBase(MainForm host, string title)
    {
        Host = host;
        Title = title;
        Dock = DockStyle.Fill;
        Font = Theme.UiFont;
        BackColor = Theme.WindowGray;
    }

    protected MainForm Host { get; }

    public string Title { get; }

    protected void AddHeader(string heading, string subtitle)
    {
        Controls.Add(Ui.NewLabel(heading, new Point(16, 12), Theme.HeadingFont));
        Controls.Add(Ui.NewLabel(subtitle, new Point(18, 42), null, Color.FromArgb(48, 48, 48)));
    }

    // Re-reads the DataStore; called on creation and after changes elsewhere.
    public virtual void Reload() { }
}
