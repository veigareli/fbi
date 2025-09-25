using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin
        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Get current round
            var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRoundNumber = currentRound?.RoundNumber ?? 1;

            // Get round statistics
            var totalUsers = await _context.Users.CountAsync();
            var usersWithTeams = await _context.FantasyTeams
                .Where(ft => ft.Round == currentRoundNumber && ft.IsActive)
                .Select(ft => ft.UserId)
                .Distinct()
                .CountAsync();

            var playerStatsCount = await _context.PlayerRoundPoints
                .Where(prp => prp.Round == currentRoundNumber)
                .CountAsync();

            var viewModel = new AdminViewModel
            {
                CurrentRound = currentRoundNumber,
                TotalUsers = totalUsers,
                UsersWithTeams = usersWithTeams,
                PlayerStatsCount = playerStatsCount
            };

            return View(viewModel);
        }

        // GET: /admin/round
        public async Task<IActionResult> Round()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRoundNumber = currentRound?.RoundNumber ?? 1;

            return View(new AdminRoundViewModel { CurrentRound = currentRoundNumber });
        }

        // POST: /admin/round
        [HttpPost]
        public async Task<IActionResult> UpdateRound(int roundNumber)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (roundNumber < 1)
            {
                TempData["Error"] = "Round number must be at least 1";
                return RedirectToAction("Round");
            }

            var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
            if (currentRound == null)
            {
                _context.CurrentRound.Add(new CurrentRound { RoundNumber = roundNumber });
            }
            else
            {
                currentRound.RoundNumber = roundNumber;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Current round updated to {roundNumber}";
            return RedirectToAction("Index");
        }

        // GET: /admin/player-stats
        public async Task<IActionResult> PlayerStats(int? round = null, int? teamId = null)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
            var selectedRound = round ?? currentRound?.RoundNumber ?? 1;

            var teams = await _context.Teams.ToListAsync();
            var selectedTeamId = teamId ?? teams.FirstOrDefault()?.Id;

            var players = await _context.Players
                .Include(p => p.Team)
                .Where(p => selectedTeamId == null || p.TeamId == selectedTeamId)
                .OrderBy(p => p.Position)
                .ThenBy(p => p.Name)
                .ToListAsync();

            var existingStats = await _context.PlayerRoundPoints
                .Where(prp => prp.Round == selectedRound)
                .ToListAsync();

            var viewModel = new AdminPlayerStatsViewModel
            {
                Round = selectedRound,
                Players = players,
                ExistingStats = existingStats,
                Teams = teams,
                SelectedTeamId = selectedTeamId
            };

            return View(viewModel);
        }

        // POST: /admin/player-stats
        [HttpPost]
        public async Task<IActionResult> AddPlayerStats(int playerId, int round, int points, int rebounds, int assists, int steals, int blocks, int turnovers, bool teamWin)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Calculate fantasy points
            var fantasyPoints = points + rebounds + assists + (steals * 2) + (blocks * 2) - turnovers + (teamWin ? 5 : -3);

            var playerStats = new PlayerRoundPoints
            {
                PlayerId = playerId,
                Round = round,
                Points = points,
                Rebounds = rebounds,
                Assists = assists,
                Steals = steals,
                Blocks = blocks,
                Turnovers = turnovers,
                TeamWin = teamWin,
                Score = teamWin ? "W" : "L",
                FantasyPoints = fantasyPoints,
                TotalPoints = fantasyPoints
            };

            _context.PlayerRoundPoints.Add(playerStats);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Player statistics added successfully";
            return RedirectToAction("PlayerStats", new { round });
        }

        // POST: /admin/player-stats/bulk-team
        [HttpPost]
        public async Task<IActionResult> AddTeamStats(int teamId, int round, bool teamWin)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Get all players from the team
            var players = await _context.Players
                .Where(p => p.TeamId == teamId)
                .ToListAsync();

            if (!players.Any())
            {
                TempData["Error"] = "No players found for this team";
                return RedirectToAction("PlayerStats", new { round });
            }

            // Generate random stats for each player
            var random = new Random();
            var playerStats = new List<PlayerRoundPoints>();

            foreach (var player in players)
            {
                // Generate realistic basketball statistics
                var points = random.Next(0, 36); // 0-35 points
                var rebounds = random.Next(0, 16); // 0-15 rebounds
                var assists = random.Next(0, 13); // 0-12 assists
                var steals = random.Next(0, 5); // 0-4 steals
                var blocks = random.Next(0, 6); // 0-5 blocks
                var turnovers = random.Next(0, 7); // 0-6 turnovers

                // Calculate fantasy points
                var fantasyPoints = points + rebounds + assists + (steals * 2) + (blocks * 2) - turnovers + (teamWin ? 5 : -3);

                playerStats.Add(new PlayerRoundPoints
                {
                    PlayerId = player.Id,
                    Round = round,
                    Points = points,
                    Rebounds = rebounds,
                    Assists = assists,
                    Steals = steals,
                    Blocks = blocks,
                    Turnovers = turnovers,
                    TeamWin = teamWin,
                    Score = teamWin ? "W" : "L",
                    FantasyPoints = fantasyPoints,
                    TotalPoints = fantasyPoints
                });
            }

            // Bulk insert all player stats
            _context.PlayerRoundPoints.AddRange(playerStats);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Added statistics for {players.Count} players from team";
            return RedirectToAction("PlayerStats", new { round });
        }

        // POST: /admin/player-stats/bulk-team-table
        [HttpPost]
        public async Task<IActionResult> AddTeamStatsTable(int teamId, int round, bool teamWin, List<PlayerStatsEntry> playerStats)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (playerStats == null || !playerStats.Any())
            {
                TempData["Error"] = "No player statistics provided";
                return RedirectToAction("PlayerStats", new { round, teamId });
            }

            var playerStatsList = new List<PlayerRoundPoints>();

            foreach (var stat in playerStats)
            {
                if (stat.PlayerId > 0) // Only process valid player IDs
                {
                    // Calculate fantasy points
                    var fantasyPoints = stat.Points + stat.Rebounds + stat.Assists + 
                                     (stat.Steals * 2) + (stat.Blocks * 2) - stat.Turnovers + 
                                     (teamWin ? 5 : -3);

                    playerStatsList.Add(new PlayerRoundPoints
                    {
                        PlayerId = stat.PlayerId,
                        Round = round,
                        Points = stat.Points,
                        Rebounds = stat.Rebounds,
                        Assists = stat.Assists,
                        Steals = stat.Steals,
                        Blocks = stat.Blocks,
                        Turnovers = stat.Turnovers,
                        TeamWin = teamWin,
                        Score = teamWin ? "W" : "L",
                        FantasyPoints = fantasyPoints,
                        TotalPoints = fantasyPoints
                    });
                }
            }

            // Bulk insert all player stats
            _context.PlayerRoundPoints.AddRange(playerStatsList);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Added statistics for {playerStatsList.Count} players from team";
            return RedirectToAction("PlayerStats", new { round, teamId });
        }

        // POST: /admin/player-stats/bulk-all
        [HttpPost]
        public async Task<IActionResult> AddAllTeamsStats(int round)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Get all teams
            var teams = await _context.Teams.ToListAsync();
            var random = new Random();
            var allPlayerStats = new List<PlayerRoundPoints>();

            foreach (var team in teams)
            {
                // Randomly determine if team wins or loses
                var teamWin = random.Next(0, 2) == 1;

                // Get all players from the team
                var players = await _context.Players
                    .Where(p => p.TeamId == team.Id)
                    .ToListAsync();

                foreach (var player in players)
                {
                    // Generate realistic basketball statistics
                    var points = random.Next(0, 36);
                    var rebounds = random.Next(0, 16);
                    var assists = random.Next(0, 13);
                    var steals = random.Next(0, 5);
                    var blocks = random.Next(0, 6);
                    var turnovers = random.Next(0, 7);

                    // Calculate fantasy points
                    var fantasyPoints = points + rebounds + assists + (steals * 2) + (blocks * 2) - turnovers + (teamWin ? 5 : -3);

                    allPlayerStats.Add(new PlayerRoundPoints
                    {
                        PlayerId = player.Id,
                        Round = round,
                        Points = points,
                        Rebounds = rebounds,
                        Assists = assists,
                        Steals = steals,
                        Blocks = blocks,
                        Turnovers = turnovers,
                        TeamWin = teamWin,
                        Score = teamWin ? "W" : "L",
                        FantasyPoints = fantasyPoints,
                        TotalPoints = fantasyPoints
                    });
                }
            }

            // Bulk insert all player stats
            _context.PlayerRoundPoints.AddRange(allPlayerStats);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Added statistics for all {allPlayerStats.Count} players for round {round}";
            return RedirectToAction("PlayerStats", new { round });
        }

        // GET: /admin/calculate
        public async Task<IActionResult> Calculate()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
            var currentRoundNumber = currentRound?.RoundNumber ?? 1;

            return View(new AdminCalculateViewModel { Round = currentRoundNumber });
        }

        // POST: /admin/calculate
        [HttpPost]
        public async Task<IActionResult> CalculateScores(int round)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Authentication");
            }

            try
            {
                await CalculateUserRoundPoints(round);
                TempData["Success"] = $"User scores calculated for round {round}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error calculating scores: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private async Task CalculateUserRoundPoints(int round)
        {
            // Get all users
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                // Get user's fantasy team for this round
                var fantasyTeam = await _context.FantasyTeams
                    .Where(ft => ft.UserId == user.Id && ft.Round == round && ft.IsActive)
                    .ToListAsync();

                if (fantasyTeam.Count == 10) // Complete team
                {
                    // Get player stats for this round
                    var playerStats = await _context.PlayerRoundPoints
                        .Where(prp => prp.Round == round)
                        .ToListAsync();

                    // Calculate score: all starters + top 3 bench players
                    var starters = fantasyTeam.Where(ft => ft.IsOnCourt).ToList();
                    var benchPlayers = fantasyTeam.Where(ft => !ft.IsOnCourt).ToList();

                    var roundPoints = 0;

                    // Add all starters
                    foreach (var starter in starters)
                    {
                        var stats = playerStats.FirstOrDefault(ps => ps.PlayerId == starter.PlayerId);
                        if (stats != null)
                        {
                            roundPoints += stats.FantasyPoints;
                        }
                    }

                    // Add top 3 bench players
                    var benchPoints = new List<int>();
                    foreach (var bench in benchPlayers)
                    {
                        var stats = playerStats.FirstOrDefault(ps => ps.PlayerId == bench.PlayerId);
                        if (stats != null)
                        {
                            benchPoints.Add(stats.FantasyPoints);
                        }
                    }

                    // Sort bench points and take top 3
                    benchPoints.Sort((a, b) => b.CompareTo(a));
                    roundPoints += benchPoints.Take(3).Sum();

                    // Update or create UserRoundPoints
                    var existingUserRoundPoints = await _context.UserRoundPoints
                        .FirstOrDefaultAsync(urp => urp.UserId == user.Id && urp.Round == round);

                    if (existingUserRoundPoints != null)
                    {
                        existingUserRoundPoints.Points = roundPoints;
                    }
                    else
                    {
                        _context.UserRoundPoints.Add(new UserRoundPoints
                        {
                            UserId = user.Id,
                            Round = round,
                            Points = roundPoints
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return false;
            }

            // Check for hardcoded admin (UserId = "0")
            if (userId == "0" && userEmail == "admin@gmail.com")
            {
                return true;
            }

            // Check for database admin user
            if (int.TryParse(userId, out int userIdInt))
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userIdInt);
                return user?.Email == "admin@gmail.com";
            }

            return false;
        }
    }
}
