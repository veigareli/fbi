namespace Web.Models;

public class PlayerRoundPoints
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    
    public int Round { get; set; }
    
    // Individual player statistics
    public int Points { get; set; }
    public int Rebounds { get; set; }
    public int Assists { get; set; }
    public int Steals { get; set; }
    public int Blocks { get; set; }
    public int Turnovers { get; set; }
    public bool TeamWin { get; set; }
    public string Score { get; set; } = "L"; // W or L
    
    // Calculated fantasy points
    public int FantasyPoints { get; set; }
    public int TotalPoints { get; set; }
    
    // Navigation property
    public Players Player { get; set; } = null!;
}
