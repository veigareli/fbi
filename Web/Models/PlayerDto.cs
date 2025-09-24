namespace Web.Models;

public class PlayerDto
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int Cost { get; set; } = 0;
    public int TotalPoints { get; set; } = 0;
}
