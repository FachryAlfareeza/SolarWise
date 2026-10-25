namespace SolarWise;

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // never displayed
    public virtual string Role => "User";
    public bool IsAdmin => this is Admin;
    public override string ToString() => Username;
}
