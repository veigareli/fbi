namespace Web.Models;

public class HomeViewModel
{
    public Users User { get; set; } = null!;
    public List<FantasyTeam> FantasyTeams { get; set; } = new();
    public List<UserRoundPoints> RoundPoints { get; set; } = new();
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();
    public int UserRank { get; set; }
    public int TotalPlayers { get; set; }
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int UserId { get; set; }
}
