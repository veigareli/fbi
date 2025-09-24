namespace Web.Models;

public class UserRoundPoints
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int Round { get; set; }
    
    public int Points { get; set; }
    
    // Navigation property
    public Users User { get; set; } = null!;
}
