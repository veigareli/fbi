namespace Web.Models;

public class MyTeamViewModel
{
    public Users User { get; set; } = null!;
    public List<FantasyTeam> CurrentRoundTeam { get; set; } = new();
    public List<Players> AvailablePlayers { get; set; } = new();
    public UserRoundTeam? CurrentRoundInfo { get; set; }
    public int CurrentRound { get; set; } = 1;
    public int MaxRound { get; set; } = 1; // Maximum round allowed (from CurrentRound table)
    public int TotalBudget { get; set; } = 100;
    public int UsedBudget { get; set; } = 0;
    public int RemainingBudget { get; set; } = 100;
    public int SelectedPlayersCount { get; set; } = 0;
    public int RoundPoints { get; set; } = 0; // Points for the selected round
    public bool IsHistoricalRound { get; set; } = false; // True if viewing a past round
    public Dictionary<int, int> PlayerRoundPoints { get; set; } = new(); // PlayerId -> Points for the round
    public Dictionary<int, PlayerRoundPoints> PlayerDetailedStats { get; set; } = new(); // PlayerId -> Detailed stats for the round
    public Dictionary<int, bool> PlayerIsChosen { get; set; } = new(); // PlayerId -> Whether player's points count (chosen)
}
