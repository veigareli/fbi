namespace Web.Models
{
    public class AdminViewModel
    {
        public int CurrentRound { get; set; }
        public int TotalUsers { get; set; }
        public int UsersWithTeams { get; set; }
        public int PlayerStatsCount { get; set; }
    }

    public class AdminRoundViewModel
    {
        public int CurrentRound { get; set; }
    }

    public class AdminPlayerStatsViewModel
    {
        public int Round { get; set; }
        public List<Players> Players { get; set; } = new();
        public List<PlayerRoundPoints> ExistingStats { get; set; } = new();
        public List<Teams> Teams { get; set; } = new();
        public int? SelectedTeamId { get; set; }
    }

    public class AdminCalculateViewModel
    {
        public int Round { get; set; }
    }

    public class PlayerStatsEntry
    {
        public int PlayerId { get; set; }
        public int Points { get; set; }
        public int Rebounds { get; set; }
        public int Assists { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public int Turnovers { get; set; }
    }
}
