namespace Web.Models;

public class Users
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int TotalPoints { get; set; } = 0;
    
    // Navigation properties
    public List<FantasyTeam> FantasyTeams { get; set; } = new();

}
