namespace Web.Models;

public class PlayerHistoryViewModel
{
    public Users TargetUser { get; set; } = null!;
    public List<int> AvailableRounds { get; set; } = new();
    public int SelectedRound { get; set; }
    public List<object> TeamData { get; set; } = new();
    public int RoundPoints { get; set; }
    public Dictionary<int, int> PlayerRoundPoints { get; set; } = new(); // PlayerId -> Points for the round
    public Dictionary<int, PlayerRoundPoints> PlayerDetailedStats { get; set; } = new(); // PlayerId -> Detailed stats for the round
    public Dictionary<int, bool> PlayerIsChosen { get; set; } = new(); // PlayerId -> Whether player's points count (chosen)
}
