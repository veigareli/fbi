namespace Web.Models;

public class Teams
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    // Navigation property
    public List<Players> Players { get; set; } = new();

}
