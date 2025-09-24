namespace Web.Models;

public class Players
{
    public int Id { get; set; }

    public int TeamId { get; set; }

    public string Name { get; set; } = null!;

    public string Position { get; set; } = null!;

    public int Cost { get; set; } = 0;

    public int TotalPoints { get; set; } = 0;

    // Navigation property
    public Teams? Team { get; set; }

}
