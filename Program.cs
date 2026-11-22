namespace SolarWise;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (_, e) => ShowDatabaseError(e.Exception);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        if (!SolarWise.Database.SupabaseConnectionFactory.TestConnection(out string message))
        {
            MessageBox.Show("Cannot connect to the Supabase database. Check the .env file and your internet connection.\n\n" + message,
                "SolarWise", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        try { DatabaseSeeder.EnsureSeeded(); }
        catch (Exception ex) { ShowDatabaseError(ex); return; }

        // Login -> main window; logging out returns to the login screen.
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK) break;

            using var main = new MainForm();
            main.ShowDialog();
            if (!main.LogoutRequested) break;
        }
    }

    // Any database failure (no network, constraint violation) is shown instead of crashing the app.
    private static void ShowDatabaseError(Exception ex) =>
        MessageBox.Show("Something went wrong:\n\n" + ex.Message, "SolarWise", MessageBoxButtons.OK, MessageBoxIcon.Error);
}