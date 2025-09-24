using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.Data;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Home()
    {
        // Check if user is logged in
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Authentication");
        }

        var user = await _context.Users
            .Include(u => u.FantasyTeams)
                .ThenInclude(ft => ft.Player)
                    .ThenInclude(p => p.Team)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
        {
            return RedirectToAction("Login", "Authentication");
        }

        var roundPoints = await _context.UserRoundPoints
            .Where(urp => urp.UserId == user.Id)
            .OrderBy(urp => urp.Round)
            .ToListAsync();

        // Get leaderboard data
        var allUsers = await _context.Users
            .OrderByDescending(u => u.TotalPoints)
            .ToListAsync();

        var leaderboard = allUsers.Select((u, index) => new LeaderboardEntry
        {
            Rank = index + 1,
            UserName = u.Name,
            TotalPoints = u.TotalPoints,
            UserId = u.Id
        }).ToList();

        var userRank = allUsers.FindIndex(u => u.Id == user.Id) + 1;
        var totalPlayers = allUsers.Count;

        var viewModel = new HomeViewModel
        {
            User = user,
            FantasyTeams = user.FantasyTeams.ToList(),
            RoundPoints = roundPoints,
            Leaderboard = leaderboard,
            UserRank = userRank,
            TotalPlayers = totalPlayers
        };

        return View(viewModel);
    }

    public async Task<IActionResult> MyTeam(int? round = null)
    {
        // Check if user is logged in
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Authentication");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
        {
            return RedirectToAction("Login", "Authentication");
        }

        // Get the maximum allowed round from CurrentRound table
        var maxRoundRecord = await _context.CurrentRound.FirstOrDefaultAsync();
        var maxRound = maxRoundRecord?.RoundNumber ?? 1;

        // Determine which round to display
        var selectedRound = round ?? maxRound; // Default to current round if no round specified
        
        // Ensure round doesn't exceed maximum
        if (selectedRound > maxRound)
        {
            selectedRound = maxRound;
        }
        
        // Ensure round is at least 1
        if (selectedRound < 1)
        {
            selectedRound = 1;
        }

        // Get team for the selected round
        var currentRoundTeam = await _context.FantasyTeams
            .Include(ft => ft.Player)
                .ThenInclude(p => p.Team)
            .Where(ft => ft.UserId == user.Id && ft.Round == selectedRound && ft.IsActive)
            .ToListAsync();

        // Get round info for the selected round
        var currentRoundInfo = await _context.UserRoundTeams
            .FirstOrDefaultAsync(urt => urt.UserId == user.Id && urt.Round == selectedRound);

        // If no round info exists and it's the current round, create it
        if (currentRoundInfo == null && selectedRound == maxRound)
        {
            currentRoundInfo = new UserRoundTeam
            {
                UserId = user.Id,
                Round = selectedRound,
                TotalBudget = 100,
                UsedBudget = 0,
                IsLocked = false
            };
            _context.UserRoundTeams.Add(currentRoundInfo);
            await _context.SaveChangesAsync();
        }

        // Calculate used budget
        var usedBudget = currentRoundTeam.Sum(ft => ft.Player.Cost);

        // Get round points for the selected round
        var roundPoints = 0;
        var playerRoundPoints = new Dictionary<int, int>();
        var playerDetailedStats = new Dictionary<int, PlayerRoundPoints>();
        var playerIsChosen = new Dictionary<int, bool>();
        
        if (selectedRound < maxRound) // Historical round
        {
            var userRoundPoints = await _context.UserRoundPoints
                .FirstOrDefaultAsync(urp => urp.UserId == user.Id && urp.Round == selectedRound);
            roundPoints = userRoundPoints?.Points ?? 0;
            
            // Get player round points for this round
            var playerPoints = await _context.PlayerRoundPoints
                .Where(prp => prp.Round == selectedRound)
                .ToListAsync();
            
            foreach (var pp in playerPoints)
            {
                playerRoundPoints[pp.PlayerId] = pp.TotalPoints;
                playerDetailedStats[pp.PlayerId] = pp;
            }
            
            // Calculate which players are chosen (starters + top 3 bench) vs not chosen (bottom 2 bench)
            if (selectedRound < maxRound)
            {
                // Get the user's fantasy team for this round
                var fantasyTeam = await _context.FantasyTeams
                    .Where(ft => ft.UserId == user.Id && ft.Round == selectedRound && ft.IsActive)
                    .ToListAsync();
                
                if (fantasyTeam.Count == 10) // Complete team
                {
                    // Get starters (all count)
                    var starters = fantasyTeam.Where(ft => ft.IsOnCourt).ToList();
                    
                    // Get bench players
                    var benchPlayers = fantasyTeam.Where(ft => !ft.IsOnCourt).ToList();
                    
                    // All starters are chosen
                    foreach (var starter in starters)
                    {
                        playerIsChosen[starter.PlayerId] = true;
                    }
                    
                    // Top 3 bench players are chosen
                    var top3Bench = benchPlayers
                        .OrderByDescending(bp => playerRoundPoints.ContainsKey(bp.PlayerId) ? playerRoundPoints[bp.PlayerId] : 0)
                        .Take(3)
                        .ToList();
                    
                    foreach (var benchPlayer in top3Bench)
                    {
                        playerIsChosen[benchPlayer.PlayerId] = true;
                    }
                    
                    // Bottom 2 bench players are not chosen
                    var bottom2Bench = benchPlayers
                        .OrderByDescending(bp => playerRoundPoints.ContainsKey(bp.PlayerId) ? playerRoundPoints[bp.PlayerId] : 0)
                        .Skip(3)
                        .ToList();
                    
                    foreach (var benchPlayer in bottom2Bench)
                    {
                        playerIsChosen[benchPlayer.PlayerId] = false;
                    }
                }
            }
        }
        else // Current round - calculate from player performance
        {
            // For current round, we'll show 0 points since no games have been played yet
            roundPoints = 0;
        }

        var availablePlayers = await _context.Players
            .Include(p => p.Team)
            .ToListAsync();

        var viewModel = new MyTeamViewModel
        {
            User = user,
            CurrentRoundTeam = currentRoundTeam,
            AvailablePlayers = availablePlayers,
            CurrentRoundInfo = currentRoundInfo,
            CurrentRound = selectedRound,
            MaxRound = maxRound,
            TotalBudget = currentRoundInfo?.TotalBudget ?? 100,
            UsedBudget = usedBudget,
            RemainingBudget = (currentRoundInfo?.TotalBudget ?? 100) - usedBudget,
            SelectedPlayersCount = currentRoundTeam.Count,
            RoundPoints = roundPoints,
            IsHistoricalRound = selectedRound < maxRound,
            PlayerRoundPoints = playerRoundPoints,
            PlayerDetailedStats = playerDetailedStats,
            PlayerIsChosen = selectedRound < maxRound ? playerIsChosen : new Dictionary<int, bool>()
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Rules()
    {
        return View();
    }

    public IActionResult TestPlayerHistory(int userId)
    {
        return Content($"PlayerHistory test for user {userId}");
    }

    public IActionResult SimplePlayerHistory(int id)
    {
        return Content($"Simple PlayerHistory for user {id}");
    }

    public IActionResult TestAction()
    {
        return Content("Test action works!");
    }

    public IActionResult TestWithParam(int id)
    {
        return Content($"Test with param: {id}");
    }

    public async Task<IActionResult> PlayerHistory(int id, int? round = null)
    {
        // Check if user is logged in
        var currentUserId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return RedirectToAction("Login", "Authentication");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (targetUser == null)
        {
            return NotFound();
        }

        // Get current round to determine available historical rounds
        var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
        var maxRound = currentRound?.RoundNumber ?? 0;

        // Get available rounds for this user
        var availableRounds = await _context.UserRoundPoints
            .Where(urp => urp.UserId == id && urp.Round < maxRound)
            .OrderBy(urp => urp.Round)
            .Select(urp => urp.Round)
            .ToListAsync();

        if (!availableRounds.Any())
        {
            return View("PlayerHistory", new PlayerHistoryViewModel 
            { 
                TargetUser = targetUser, 
                AvailableRounds = new List<int>(),
                SelectedRound = 0,
                TeamData = new List<object>(),
                RoundPoints = 0
            });
        }

        // Determine which round to show
        var selectedRound = round ?? availableRounds.Last();
        if (!availableRounds.Contains(selectedRound))
        {
            selectedRound = availableRounds.Last();
        }

        // Get team data for the selected round
        var teamForRound = await _context.FantasyTeams
            .Where(ft => ft.UserId == id && ft.Round == selectedRound && ft.IsActive)
            .Include(ft => ft.Player)
                .ThenInclude(p => p.Team)
            .ToListAsync();

        // Get individual player points for this round
        var playerRoundPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.Round == selectedRound)
            .Include(prp => prp.Player)
            .ToListAsync();

        // Get total points for this round
        var roundPoints = await _context.UserRoundPoints
            .FirstOrDefaultAsync(urp => urp.UserId == id && urp.Round == selectedRound);

        // Create dictionaries for detailed stats and chosen status
        var playerRoundPointsDict = new Dictionary<int, int>();
        var playerDetailedStats = new Dictionary<int, PlayerRoundPoints>();
        var playerIsChosen = new Dictionary<int, bool>();

        foreach (var prp in playerRoundPoints)
        {
            playerRoundPointsDict[prp.PlayerId] = prp.TotalPoints;
            playerDetailedStats[prp.PlayerId] = prp;
        }

        // Calculate which players are chosen (starters + top 3 bench) vs not chosen (bottom 2 bench)
        var teamPlayerIds = teamForRound.Select(ft => ft.PlayerId).ToList();
        var teamPlayerPoints = playerRoundPoints
            .Where(prp => teamPlayerIds.Contains(prp.PlayerId))
            .ToList();

        if (teamPlayerPoints.Count == 10)
        {
            var starters = teamForRound.Where(ft => ft.IsOnCourt).Select(ft => ft.PlayerId).ToList();
            var benchPlayers = teamForRound.Where(ft => !ft.IsOnCourt).Select(ft => ft.PlayerId).ToList();

            // All starters are chosen
            foreach (var starterId in starters)
            {
                playerIsChosen[starterId] = true;
            }

            // Top 3 bench players are chosen
            var benchPoints = benchPlayers
                .Select(benchId => new { PlayerId = benchId, Points = playerRoundPointsDict.GetValueOrDefault(benchId, 0) })
                .OrderByDescending(x => x.Points)
                .ToList();

            for (int i = 0; i < benchPoints.Count; i++)
            {
                playerIsChosen[benchPoints[i].PlayerId] = i < 3; // Top 3 are chosen
            }
        }

        // Build team data similar to MyTeam page
        var teamData = new List<object>();
        var positions = new[] { "PG", "SG", "SF", "PF", "C" };
        
        foreach (var position in positions)
        {
            var courtPlayer = teamForRound.FirstOrDefault(ft => ft.Player.Position == position && ft.IsOnCourt);
            var benchPlayer = teamForRound.FirstOrDefault(ft => ft.Player.Position == position && !ft.IsOnCourt);
            
            var courtPlayerData = courtPlayer != null ? new
            {
                PlayerId = courtPlayer.Player.Id,
                Name = courtPlayer.Player.Name,
                Position = courtPlayer.Player.Position,
                TeamName = courtPlayer.Player.Team?.Name ?? "Unknown",
                Cost = courtPlayer.Player.Cost,
                RoundPoints = playerRoundPoints.FirstOrDefault(prp => prp.PlayerId == courtPlayer.Player.Id)?.TotalPoints ?? 0
            } : null;

            var benchPlayerData = benchPlayer != null ? new
            {
                PlayerId = benchPlayer.Player.Id,
                Name = benchPlayer.Player.Name,
                Position = benchPlayer.Player.Position,
                TeamName = benchPlayer.Player.Team?.Name ?? "Unknown",
                Cost = benchPlayer.Player.Cost,
                RoundPoints = playerRoundPoints.FirstOrDefault(prp => prp.PlayerId == benchPlayer.Player.Id)?.TotalPoints ?? 0
            } : null;

            teamData.Add(new
            {
                Position = position,
                CourtPlayer = courtPlayerData,
                BenchPlayer = benchPlayerData
            });
        }

        var viewModel = new PlayerHistoryViewModel
        {
            TargetUser = targetUser,
            AvailableRounds = availableRounds,
            SelectedRound = selectedRound,
            TeamData = teamData,
            RoundPoints = roundPoints?.Points ?? 0,
            PlayerRoundPoints = playerRoundPointsDict,
            PlayerDetailedStats = playerDetailedStats,
            PlayerIsChosen = playerIsChosen
        };

        return View("PlayerHistory", viewModel);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
