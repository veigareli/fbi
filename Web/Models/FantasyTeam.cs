namespace Web.Models;

public class FantasyTeam
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int PlayerId { get; set; }
    
    public int Round { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsOnCourt { get; set; } = true; // true = court, false = bench
    
    // Navigation properties
    public Users User { get; set; } = null!;
    public Players Player { get; set; } = null!;
}
