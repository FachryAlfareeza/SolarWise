using System.Runtime.InteropServices;

namespace SolarWise;

// Base window: borderless form with a classic blue title bar, gray frame and window controls.
public class ClassicForm : Form
{
    private const int WmNcLButtonDown = 0xA1;
    private const int HtCaption = 0x2;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private readonly Panel titleBar = new() { Dock = DockStyle.Top, Height = 24, BackColor = Theme.TitleBlue };
    private readonly PictureBox titleIcon = new()
    {
        Image = Theme.SolarIcon(),
        Size = new Size(16, 16),
        Location = new Point(6, 4),
        BackColor = Color.Transparent
    };
    private readonly Label titleLabel = new()
    {
        Location = new Point(28, 4),
        AutoSize = true,
        ForeColor = Color.White,
        BackColor = Color.Transparent,
        Font = Theme.BoldFont
    };
    private readonly ClassicButton minimizeButton = new() { Text = "_", Size = new Size(20, 18), Font = Theme.BoldFont };
    private readonly ClassicButton closeButton = new() { Text = "X", Size = new Size(20, 18), Font = Theme.BoldFont };

    protected Panel Content { get; } = new() { Dock = DockStyle.Fill, BackColor = Theme.WindowGray };

    protected ClassicForm(string title, int width, int height, bool isDialog = false)
    {
        Text = title;
        titleLabel.Text = title;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = isDialog ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
        BackColor = Theme.WindowGray;
        Font = Theme.UiFont;
        Padding = new Padding(3);
        ShowInTaskbar = !isDialog;
        // The title bar takes 24 px on top, so the content area is width x height.
        ClientSize = new Size(width, height + 24);

        minimizeButton.Visible = !isDialog;
        minimizeButton.Click += (_, _) => WindowState = FormWindowState.Minimized;
        closeButton.Click += (_, _) => Close();

        titleBar.Controls.Add(titleIcon);
        titleBar.Controls.Add(titleLabel);
        titleBar.Controls.Add(minimizeButton);
        titleBar.Controls.Add(closeButton);
        titleBar.Resize += (_, _) => LayoutTitleButtons();
        titleBar.MouseDown += StartDrag;
        titleLabel.MouseDown += StartDrag;

        // Fill panel first, then the title bar docks to the top.
        Controls.Add(Content);
        Controls.Add(titleBar);
    }

    protected void SetTitle(string title)
    {
        Text = title;
        titleLabel.Text = title;
    }

    // Sets the dialog result and closes the window.
    protected void Finish(DialogResult result)
    {
        DialogResult = result;
        Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Raised);
    }

    private void LayoutTitleButtons()
    {
        closeButton.Location = new Point(titleBar.Width - closeButton.Width - 4, 3);
        minimizeButton.Location = new Point(closeButton.Left - minimizeButton.Width - 2, 3);
    }

    private void StartDrag(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture();
        SendMessage(Handle, WmNcLButtonDown, (IntPtr)HtCaption, IntPtr.Zero);
    }
}
