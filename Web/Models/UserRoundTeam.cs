namespace Web.Models;

public class UserRoundTeam
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int Round { get; set; }
    
    public int TotalBudget { get; set; } = 100; // Fixed budget per round
    
    public int UsedBudget { get; set; } = 0; // How much budget is used
    
    public bool IsLocked { get; set; } = false; // Whether the team is locked for this round
    
    // Navigation properties
    public Users User { get; set; } = null!;
}
