namespace SolarWise;

// In-memory data for the prototype. Everything resets when the application closes.
// Holds the shared ID counter and the sample data seed for all repositories.
public static class InMemoryDatabase
{
    private static int lastId = 100;
    private static bool seeded;

    public static int NextId() => ++lastId;

    public static void EnsureSeeded()
    {
        if (seeded) return;
        seeded = true;
        Seed();
    }

    // Sample data: Indonesian names and places, clearly marked as sample.

    private static void Seed()
    {
        var ilham = new User
        {
            UserID = 1,
            Username = "ilham",
            FullName = "Ilham Abdillah",
            EmailAddress = "ilham@example.com",
            PasswordHash = AuthService.Hash("ilham1234")
        };
        var admin = new Admin
        {
            UserID = 2,
            AdminID = 1,
            Username = "admin",
            FullName = "System Administrator",
            EmailAddress = "admin@example.com",
            PasswordHash = AuthService.Hash("admin1234")
        };
        UserRepository.Users.Add(ilham);
        UserRepository.Users.Add(admin);

        var mono = SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Monocrystalline 550 Wp", Efficiency = 21.2, PricePerMeter = 450000, Wattage = 550, Description = "Sample catalog entry" });
        var poly = SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Polycrystalline 450 Wp", Efficiency = 18.5, PricePerMeter = 380000, Wattage = 450, Description = "Sample catalog entry" });
        var bifacial = SolarPanelRepository.AddPanel(new SolarPanel { Name = "[Sample] Bifacial 600 Wp", Efficiency = 22.0, PricePerMeter = 520000, Wattage = 600, Description = "Sample catalog entry" });

        var sleman = LocationRepository.AddLocation(new SiteLocation { Address = "Jl. Melati No. 12, Sleman, Yogyakarta", Latitude = "-7.7523", Longitude = "110.3620" }, ilham);
        var kota = LocationRepository.AddLocation(new SiteLocation { Address = "Malioboro, Yogyakarta City", Latitude = "-7.7926", Longitude = "110.3658" }, ilham);
        var bantul = LocationRepository.AddLocation(new SiteLocation { Address = "Bantul, Yogyakarta", Latitude = "-7.8881", Longitude = "110.3289" }, ilham);

        WeatherRepository.WeatherRecords.Add(new WeatherData
        {
            WeatherDataID = NextId(),
            Location = sleman,
            Date = new DateTime(2026, 10, 1),
            SolarIrradiance = 4.8,
            Temperature = 29.5,
            SunshineHours = 6.4,
            IsSample = true
        });

        var vayu = OrganizationRepository.AddOrganization(new Organization { Name = "Vayuastra", Description = "Sample organization", JoinCode = "VAYU-2026" }, ilham);
        var ugm = OrganizationRepository.AddOrganization(new Organization { Name = "UGM Research", Description = "Sample research group", JoinCode = "UGM-RSCH" }, admin);
        OrganizationRepository.AddOrganization(new Organization { Name = "Project Internal", Description = "Sample internal projects", JoinCode = "PROJ-INT" }, ilham);
        MembershipRepository.JoinOrganization(ugm, ilham);
        MembershipRepository.JoinOrganization(vayu, admin);

        var samples = new (string Project, SiteLocation Site, SolarPanel Panel, double Area, int DaysAgo)[]
        {
            ("Residential Property", sleman, mono, 20, 2),
            ("Office Building", kota, bifacial, 60, 5),
            ("Production Warehouse", bantul, poly, 90, 12),
            ("Branch Office", sleman, mono, 15, 18)
        };
        foreach (var s in samples)
        {
            var calc = SolarCalculator.GenerateCalculation(s.Project, s.Site, s.Panel, s.Area, ilham, vayu.OrganizationID);
            calc.CreatedAt = DateTime.Now.AddDays(-s.DaysAgo);
            calc.IsSample = true;
            CalculationRepository.SaveCalculation(calc);
        }
        RecommendationRepository.AddRecommendation(SolarCalculator.GenerateFromCalculation(CalculationRepository.Calculations[0]));
    }
}
