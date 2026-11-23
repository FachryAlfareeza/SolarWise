using System.Runtime.InteropServices;

namespace SolarWise;

// Base window: borderless form with a classic blue title bar, gray frame and window controls.
public class ClassicForm : Form
{
    private const int WmNcLButtonDown = 0xA1;
    private const int WmNcHitTest = 0x84;
    private const int WmGetMinMaxInfo = 0x24;
    private const int HtCaption = 0x2;
    private const int HtLeft = 10, HtRight = 11, HtTop = 12, HtTopLeft = 13, HtTopRight = 14;
    private const int HtBottom = 15, HtBottomLeft = 16, HtBottomRight = 17;

    // Width of the invisible strip along the edges that can be dragged to resize.
    private const int ResizeBorder = 6;

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize;
    }

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
    private readonly ClassicButton maximizeButton = new() { Text = "□", Size = new Size(20, 18), Font = Theme.BoldFont };
    private readonly ClassicButton closeButton = new() { Text = "X", Size = new Size(20, 18), Font = Theme.BoldFont };
    private readonly bool resizable;

    protected Panel Content { get; } = new() { Dock = DockStyle.Fill, BackColor = Theme.WindowGray };

    // resizable: the edges can be dragged and the window can be maximized (main window only).
    protected ClassicForm(string title, int width, int height, bool isDialog = false, bool resizable = false)
    {
        this.resizable = resizable;
        Text = title;
        titleLabel.Text = title;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = isDialog ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
        BackColor = Theme.WindowGray;
        Font = Theme.UiFont;
        Padding = new Padding(resizable ? ResizeBorder : 3);
        ShowInTaskbar = !isDialog;
        // The title bar takes 24 px on top, so the content area is width x height.
        ClientSize = new Size(width + Padding.Horizontal - 6, height + 24 + Padding.Vertical - 6);
        // Screens are laid out for this size, so the window can grow but not shrink below it.
        if (resizable) MinimumSize = Size;

        minimizeButton.Visible = !isDialog;
        minimizeButton.Click += (_, _) => WindowState = FormWindowState.Minimized;
        closeButton.Click += (_, _) => Close();
        maximizeButton.Visible = resizable;
        maximizeButton.Click += (_, _) => ToggleMaximize();

        titleBar.Controls.Add(titleIcon);
        titleBar.Controls.Add(titleLabel);
        titleBar.Controls.Add(minimizeButton);
        titleBar.Controls.Add(maximizeButton);
        titleBar.Controls.Add(closeButton);
        titleBar.Resize += (_, _) => LayoutTitleButtons();
        titleBar.MouseDown += StartDrag;
        titleLabel.MouseDown += StartDrag;
        titleBar.DoubleClick += (_, _) => ToggleMaximize();
        titleLabel.DoubleClick += (_, _) => ToggleMaximize();

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

    protected void ToggleMaximize()
    {
        if (!resizable) return;
        WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        maximizeButton.Text = WindowState == FormWindowState.Maximized ? "❐" : "□";
        Invalidate(); // redraw the 3D frame at the new size
    }

    protected override void WndProc(ref Message m)
    {
        // A borderless window keeps the taskbar visible when maximized only if told the working area.
        if (m.Msg == WmGetMinMaxInfo && resizable)
        {
            var screen = Screen.FromHandle(Handle);
            var info = Marshal.PtrToStructure<MinMaxInfo>(m.LParam);
            info.MaxPosition = new Point(screen.WorkingArea.Left - screen.Bounds.Left, screen.WorkingArea.Top - screen.Bounds.Top);
            info.MaxSize = new Point(screen.WorkingArea.Width, screen.WorkingArea.Height);
            info.MinTrackSize = new Point(MinimumSize.Width, MinimumSize.Height);
            Marshal.StructureToPtr(info, m.LParam, false);
            m.Result = IntPtr.Zero;
            return;
        }

        base.WndProc(ref m);

        // The frame strip belongs to the form itself; report it as a sizing border.
        if (m.Msg == WmNcHitTest && resizable && WindowState == FormWindowState.Normal)
        {
            var p = PointToClient(new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16))));
            bool left = p.X < ResizeBorder, right = p.X >= ClientSize.Width - ResizeBorder;
            bool top = p.Y < ResizeBorder, bottom = p.Y >= ClientSize.Height - ResizeBorder;
            int hit = top && left ? HtTopLeft : top && right ? HtTopRight : bottom && left ? HtBottomLeft : bottom && right ? HtBottomRight
                : left ? HtLeft : right ? HtRight : top ? HtTop : bottom ? HtBottom : 0;
            if (hit != 0) m.Result = (IntPtr)hit;
        }
    }

    private void LayoutTitleButtons()
    {
        closeButton.Location = new Point(titleBar.Width - closeButton.Width - 4, 3);
        maximizeButton.Location = new Point(closeButton.Left - maximizeButton.Width - 2, 3);
        var beforeMinimize = maximizeButton.Visible ? maximizeButton : closeButton;
        minimizeButton.Location = new Point(beforeMinimize.Left - minimizeButton.Width - 2, 3);
    }

    private void StartDrag(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || e.Clicks > 1) return;
        ReleaseCapture();
        SendMessage(Handle, WmNcLButtonDown, (IntPtr)HtCaption, IntPtr.Zero);
    }
}
