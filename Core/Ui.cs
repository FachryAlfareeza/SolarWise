using System.Globalization;
using System.Text.RegularExpressions;

namespace SolarWise;

// Small factory for the classic controls, so every screen is built the same way.
public static class Ui
{
    public static Label NewLabel(string text, Point at, Font font = null, Color? color = null) => new()
    {
        Text = text,
        Location = at,
        AutoSize = true,
        Font = font ?? Theme.UiFont,
        ForeColor = color ?? Color.Black,
        BackColor = Color.Transparent
    };

    public static TextBox NewText(Point at, int width, bool password = false) => new()
    {
        Location = at,
        Width = width,
        BorderStyle = BorderStyle.Fixed3D,
        BackColor = Color.White,
        UseSystemPasswordChar = password,
        Font = Theme.UiFont
    };

    public static TextBox NewReadOnly(Point at, int width)
    {
        var box = NewText(at, width);
        box.ReadOnly = true;
        box.BackColor = Theme.LightGray;
        return box;
    }

    public static ClassicButton NewButton(string text, Point at, EventHandler onClick, int width = 100, bool primary = false)
    {
        var button = new ClassicButton { Text = text, Location = at, Size = new Size(width, 26), IsPrimary = primary };
        button.Click += onClick;
        return button;
    }

    public static ComboBox NewCombo(Point at, int width) => new()
    {
        Location = at,
        Width = width,
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor = Color.White,
        Font = Theme.UiFont
    };

    public static ListView NewList(Point at, Size size, params (string Header, int Width)[] columns)
    {
        var list = new ListView
        {
            Location = at,
            Size = size,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            HideSelection = false,
            MultiSelect = false,
            BorderStyle = BorderStyle.Fixed3D,
            BackColor = Color.White,
            Font = Theme.UiFont
        };
        foreach (var (header, width) in columns) list.Columns.Add(header, width);
        return list;
    }

    public static ListViewItem AddRow(ListView list, object tag, params string[] cells)
    {
        var item = new ListViewItem(cells) { Tag = tag };
        list.Items.Add(item);
        return item;
    }

    public static T Selected<T>(ListView list) where T : class =>
        list.SelectedItems.Count == 0 ? null : list.SelectedItems[0].Tag as T;

    public static void SelectRow(ListView list, object tag)
    {
        foreach (ListViewItem item in list.Items)
        {
            if (item.Tag == tag)
            {
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                return;
            }
        }
    }

    public static GroupBox NewGroup(string title, Rectangle bounds) => new()
    {
        Text = title,
        Bounds = bounds,
        Font = Theme.BoldFont
    };

    // Label on the left, value on the right; advances y by one row.
    public static void AddPair(Control parent, int x, ref int y, string key, string value)
    {
        parent.Controls.Add(NewLabel(key, new Point(x, y), Theme.BoldFont));
        parent.Controls.Add(NewLabel(value, new Point(x + 190, y)));
        y += 22;
    }

    public static bool TryNumber(string text, out double value) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    public static bool IsValidEmail(string email) =>
        Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool IsStrongPassword(string password) =>
        password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);

    public static string DateText(DateTime date) => date.ToString("dd MMM yyyy", Theme.IdCulture);

    public static string StatusOf(SolarCalculation calc) => !calc.IsSaved ? "Draft" : calc.IsSample ? "Sample" : "Saved";

    // Shows a validation message and puts the cursor in the offending field.
    public static bool Fail(string message, Control field)
    {
        ClassicMessage.Error(field.FindForm(), message, "Validation");
        field.Focus();
        return false;
    }
}
