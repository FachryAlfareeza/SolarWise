namespace SolarWise;

// PROTOTYPE: organizations are not in the supplied ERD.
public class Organization
{
    // Objects loaded from the database are new instances each time, so saved ones are equal by ID.
    public override bool Equals(object obj) =>
        obj is Organization other && (OrganizationID > 0 ? other.OrganizationID == OrganizationID : ReferenceEquals(this, other));
    public override int GetHashCode() => OrganizationID > 0 ? OrganizationID.GetHashCode() : base.GetHashCode();
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
