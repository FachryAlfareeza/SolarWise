namespace SolarWise;

// Admin inherits from User (class diagram).
public class Admin : User
{
    public int AdminID { get; set; }
    public override string Role => "Admin";
}
