namespace SolarWise;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetCompatibleTextRenderingDefault(false);
        InMemoryDatabase.EnsureSeeded();

        SolarWise.Database.SupabaseConnectionFactory.TestConnection(out string pesan);
        MessageBox.Show(pesan);

        var repo = new SolarWise.Database.SolarPanelDbRepository();
        var panel = repo.Insert(new SolarPanel { Name = "Tes Panel", Efficiency = 21, PricePerMeter = 400000, Wattage = 500 });
        MessageBox.Show($"INSERT ok, id = {panel.PanelID}.");
        panel.Name = "Tes Panel (Tes Update)";
        MessageBox.Show($"UPDATE = {repo.Update(panel)}, jumlah baris = {repo.GetAll().Count}");
        MessageBox.Show($"DELETE = {repo.Delete(panel)}");

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
}
