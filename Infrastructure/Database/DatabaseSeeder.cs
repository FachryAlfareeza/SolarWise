namespace SolarWise;

// First-run sample data for an empty Supabase database: the two demo accounts, a small panel
// catalog and a few organizations. Does nothing when the users table already has rows.
public static class DatabaseSeeder
{
    public static void EnsureSeeded()
    {
        if (UserRepository.HasAnyUser()) return;

        var ilham = UserRepository.AddUser(new User
        {
            Username = "ilham",
            FullName = "Ilham Abdillah",
            EmailAddress = "ilham@example.com",
            PasswordHash = AuthService.Hash("ilham1234")
        });
        var admin = UserRepository.AddUser(new User
        {
            Username = "admin",
            FullName = "System Administrator",
            EmailAddress = "admin@example.com",
            PasswordHash = AuthService.Hash("admin1234")
        });
        UserRepository.GrantAdmin(admin);

        if (SolarPanelRepository.Panels.Count == 0)
        {
            SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Monocrystalline 550 Wp", Efficiency = 21.2, PricePerMeter = 450000, Wattage = 550, Description = "Sample catalog entry" });
            SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Polycrystalline 450 Wp", Efficiency = 18.5, PricePerMeter = 380000, Wattage = 450, Description = "Sample catalog entry" });
            SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Bifacial 600 Wp", Efficiency = 22.0, PricePerMeter = 520000, Wattage = 600, Description = "Sample catalog entry" });
        }

        var vayu = OrganizationRepository.AddOrganization(new Organization { Name = "Vayuastra", Description = "Sample organization", JoinCode = "VAYU-2026" }, ilham);
        var ugm = OrganizationRepository.AddOrganization(new Organization { Name = "UGM Research", Description = "Sample research group", JoinCode = "UGM-RSCH" }, admin);
        OrganizationRepository.AddOrganization(new Organization { Name = "Project Internal", Description = "Sample internal projects", JoinCode = "PROJ-INT" }, ilham);
        MembershipRepository.JoinOrganization(ugm, ilham);
        MembershipRepository.JoinOrganization(vayu, admin);
    }
}
