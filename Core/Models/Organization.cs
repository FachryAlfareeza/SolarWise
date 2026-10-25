namespace SolarWise;

// PROTOTYPE: organizations are not in the supplied ERD.
public class Organization
{
    public int OrganizationID { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string JoinCode { get; set; } = "";
    public override string ToString() => Name;
}

// PROTOTYPE: organization membership join table.
public class OrgMembership
{
    public int UserID { get; set; }
    public int OrganizationID { get; set; }
    public string Role { get; set; } = "Member"; // "Admin" or "Member"
}
