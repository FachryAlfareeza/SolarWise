using System.Security.Cryptography;

namespace SolarWise;

// In-memory data for the prototype. Everything resets when the application closes.
public static class DataStore
{
    public static readonly List<User> Users = new();
    public static readonly List<SolarPanel> Panels = new();
    public static readonly List<SiteLocation> Locations = new();
    public static readonly List<WeatherData> WeatherRecords = new();
    public static readonly List<SolarCalculation> Calculations = new();
    public static readonly List<Recommendation> Recommendations = new();
    public static readonly List<Organization> Organizations = new();   // PROTOTYPE
    public static readonly List<OrgMembership> Memberships = new();    // PROTOTYPE

    public static User CurrentUser { get; set; }
    public static Organization ActiveOrganization { get; set; }
    public static SiteLocation SelectedLocation { get; set; }

    private static int lastId = 100;

    static DataStore()
    {
        Seed();
    }

    public static int NextId() => ++lastId;

    public static int ActiveOrganizationId => ActiveOrganization?.OrganizationID ?? 0;

    // Accounts (logic lives in AuthService; these forward to it)

    public static string Hash(string value) => AuthService.Hash(value);

    public static User Authenticate(string login, string password) => AuthService.Authenticate(login, password);

    public static bool EmailTaken(string email, User except = null) => AuthService.EmailTaken(email, except);

    public static bool UsernameTaken(string username, User except = null) => AuthService.UsernameTaken(username, except);

    public static User Register(string fullName, string username, string email, string password) =>
        AuthService.Register(fullName, username, email, password);

    // Organizations (PROTOTYPE)

    public static List<Organization> OrganizationsFor(User user) =>
        Organizations.Where(o => Memberships.Any(m => m.UserID == user.UserID && m.OrganizationID == o.OrganizationID)).ToList();

    public static string RoleIn(User user, Organization org) =>
        Memberships.FirstOrDefault(m => m.UserID == user.UserID && m.OrganizationID == org.OrganizationID)?.Role ?? "None";

    public static int MemberCount(Organization org) =>
        Memberships.Count(m => m.OrganizationID == org.OrganizationID);

    public static bool CanManage(User user, Organization org) =>
        user.IsAdmin || RoleIn(user, org) == "Admin";

    public static Organization FindByJoinCode(string code) =>
        Organizations.FirstOrDefault(o => string.Equals(o.JoinCode, code?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static Organization AddOrganization(Organization org, User creator)
    {
        org.OrganizationID = NextId();
        if (string.IsNullOrEmpty(org.JoinCode))
        {
            org.JoinCode = "ORG-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
        }
        Organizations.Add(org);
        Memberships.Add(new OrgMembership { UserID = creator.UserID, OrganizationID = org.OrganizationID, Role = "Admin" });
        return org;
    }

    public static void JoinOrganization(Organization org, User user) =>
        Memberships.Add(new OrgMembership { UserID = user.UserID, OrganizationID = org.OrganizationID, Role = "Member" });

    public static void DeleteOrganization(Organization org)
    {
        Organizations.Remove(org);
        Memberships.RemoveAll(m => m.OrganizationID == org.OrganizationID);
        if (ActiveOrganization == org) ActiveOrganization = null;
    }

    // Locations

    public static List<SiteLocation> LocationsFor(User user) =>
        Locations.Where(l => l.OwnerUserID == user.UserID).ToList();

    public static SiteLocation AddLocation(SiteLocation site, User owner)
    {
        site.LocationID = NextId();
        site.OwnerUserID = owner.UserID;
        Locations.Add(site);
        return site;
    }

    public static void DeleteLocation(SiteLocation site)
    {
        Locations.Remove(site);
        if (SelectedLocation == site) SelectedLocation = null;
    }

    // Solar panel catalog

    public static SolarPanel AddPanel(SolarPanel panel)
    {
        panel.PanelID = NextId();
        Panels.Add(panel);
        return panel;
    }

    public static void DeletePanel(SolarPanel panel) => Panels.Remove(panel);

    // Weather (lookups live in WeatherService)

    public static WeatherData WeatherFor(SiteLocation site, DateTime date) => WeatherService.WeatherFor(site, date);

    // Calculations (scoped to the current user and active organization)

    public static List<SolarCalculation> CalculationsFor(User user) =>
        Calculations
            .Where(c => c.OwnerUserID == user.UserID && c.OrganizationID == ActiveOrganizationId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

    public static void SaveCalculation(SolarCalculation calc)
    {
        if (calc.IsSaved) return;
        calc.CalculationID = NextId();
        Calculations.Add(calc);
    }

    public static void DeleteCalculation(SolarCalculation calc)
    {
        Calculations.Remove(calc);
        Recommendations.RemoveAll(r => r.CalculationID == calc.CalculationID);
    }

    public static SolarCalculation CalculationFor(Recommendation rec) =>
        Calculations.FirstOrDefault(c => c.CalculationID == rec.CalculationID);

    // Recommendations

    public static Recommendation RecommendationFor(SolarCalculation calc) =>
        Recommendations.FirstOrDefault(r => r.CalculationID == calc.CalculationID);

    public static List<Recommendation> RecommendationsFor(User user)
    {
        var ids = CalculationsFor(user).Select(c => c.CalculationID).ToHashSet();
        return Recommendations.Where(r => ids.Contains(r.CalculationID)).ToList();
    }

    public static void AddRecommendation(Recommendation rec)
    {
        rec.RecommendationID = NextId();
        Recommendations.Add(rec);
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
            PasswordHash = Hash("ilham1234")
        };
        var admin = new Admin
        {
            UserID = 2,
            AdminID = 1,
            Username = "admin",
            FullName = "System Administrator",
            EmailAddress = "admin@example.com",
            PasswordHash = Hash("admin1234")
        };
        Users.Add(ilham);
        Users.Add(admin);

        var mono = AddPanel(new SolarPanel { Name = "[Sample] Monocrystalline 550 Wp", Efficiency = 21.2, PricePerMeter = 450000, Wattage = 550, Description = "Sample catalog entry" });
        var poly = AddPanel(new SolarPanel { Name = "[Sample] Polycrystalline 450 Wp", Efficiency = 18.5, PricePerMeter = 380000, Wattage = 450, Description = "Sample catalog entry" });
        var bifacial = AddPanel(new SolarPanel { Name = "[Sample] Bifacial 600 Wp", Efficiency = 22.0, PricePerMeter = 520000, Wattage = 600, Description = "Sample catalog entry" });

        var sleman = AddLocation(new SiteLocation { Address = "Jl. Melati No. 12, Sleman, Yogyakarta", Latitude = "-7.7523", Longitude = "110.3620" }, ilham);
        var kota = AddLocation(new SiteLocation { Address = "Malioboro, Yogyakarta City", Latitude = "-7.7926", Longitude = "110.3658" }, ilham);
        var bantul = AddLocation(new SiteLocation { Address = "Bantul, Yogyakarta", Latitude = "-7.8881", Longitude = "110.3289" }, ilham);

        WeatherRecords.Add(new WeatherData
        {
            WeatherDataID = NextId(),
            Location = sleman,
            Date = new DateTime(2026, 10, 1),
            SolarIrradiance = 4.8,
            Temperature = 29.5,
            SunshineHours = 6.4,
            IsSample = true
        });

        var vayu = AddOrganization(new Organization { Name = "Vayuastra", Description = "Sample organization", JoinCode = "VAYU-2026" }, ilham);
        var ugm = AddOrganization(new Organization { Name = "UGM Research", Description = "Sample research group", JoinCode = "UGM-RSCH" }, admin);
        AddOrganization(new Organization { Name = "Project Internal", Description = "Sample internal projects", JoinCode = "PROJ-INT" }, ilham);
        JoinOrganization(ugm, ilham);
        JoinOrganization(vayu, admin);

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
            SaveCalculation(calc);
        }
        AddRecommendation(SolarCalculator.GenerateFromCalculation(Calculations[0]));
    }
}
