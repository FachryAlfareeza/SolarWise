using System.ComponentModel;

namespace SolarWise;

// Owner-drawn push button with Windows 95/98 style raised and sunken bevels.
public class ClassicButton : Button
{
    private const DesignerSerializationVisibility Hidden = DesignerSerializationVisibility.Hidden;

    private bool pressed;

    public ClassicButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        Font = Theme.UiFont;
        ForeColor = Color.Black;
        BackColor = Theme.WindowGray;
        Size = new Size(96, 26);
    }

    // Solar-yellow face for the main action of a screen.
    [DesignerSerializationVisibility(Hidden)]
    public bool IsPrimary { get; set; }

    // Showcase only: forces the pressed / focused look in the component library.
    [DesignerSerializationVisibility(Hidden)]
    public bool ShowPressed { get; set; }

    [DesignerSerializationVisibility(Hidden)]
    public bool ShowFocus { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle rect = ClientRectangle;
        bool sunken = (pressed && Enabled) || ShowPressed;

        using (var brush = new SolidBrush(IsPrimary && Enabled ? Theme.SolarYellow : Theme.WindowGray))
        {
            g.FillRectangle(brush, rect);
        }

        ControlPaint.DrawBorder3D(g, rect, sunken ? Border3DStyle.Sunken : Border3DStyle.Raised);

        Rectangle textRect = rect;
        if (sunken) textRect.Offset(1, 1);

        Color textColor = Enabled ? ForeColor : SystemColors.GrayText;
        TextRenderer.DrawText(g, Text, Font, textRect, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

        if (Focused || ShowFocus)
        {
            ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(rect, -4, -4));
        }
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (mevent.Button == MouseButtons.Left)
        {
            pressed = true;
            Invalidate();
        }
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        pressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }
}
