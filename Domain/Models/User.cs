namespace SolarWise;

public class User
{
    // Objects loaded from the database are new instances each time, so saved ones are equal by ID.
    public override bool Equals(object obj) =>
        obj is User other && (UserID > 0 ? other.UserID == UserID : ReferenceEquals(this, other));
    public override int GetHashCode() => UserID > 0 ? UserID.GetHashCode() : base.GetHashCode();
    public int UserID { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // never displayed
    public virtual string Role => "User";
    public bool IsAdmin => this is Admin;
    public override string ToString() => Username;
}
