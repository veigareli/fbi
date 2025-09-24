using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/statistics/leaderboard
    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<object>>> GetLeaderboard()
    {
        var leaderboard = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.TotalPoints,
                FantasyTeamsCount = _context.FantasyTeams.Count(ft => ft.UserId == u.Id)
            })
            .OrderByDescending(u => u.TotalPoints)
            .ToListAsync();

        return Ok(leaderboard);
    }

    // GET: api/statistics/leaderboard/round/{round}
    [HttpGet("leaderboard/round/{round}")]
    public async Task<ActionResult<IEnumerable<object>>> GetLeaderboardByRound(int round)
    {
        var leaderboard = await _context.UserRoundPoints
            .Where(urp => urp.Round == round)
            .Include(urp => urp.User)
            .Select(urp => new
            {
                urp.User.Id,
                urp.User.Name,
                urp.Points,
                Round = urp.Round
            })
            .OrderByDescending(urp => urp.Points)
            .ToListAsync();

        return Ok(leaderboard);
    }

    // GET: api/statistics/top-players
    [HttpGet("top-players")]
    public async Task<ActionResult<IEnumerable<object>>> GetTopPlayers()
    {
        var players = await _context.Players
            .Include(p => p.Team)
            .ToListAsync();

        var topPlayers = players
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Position,
                p.TotalPoints,
                TeamName = p.Team?.Name ?? "Unknown",
                AveragePoints = _context.PlayerRoundPoints
                    .Where(prp => prp.PlayerId == p.Id)
                    .Average(prp => (double?)prp.TotalPoints) ?? 0.0
            })
            .OrderByDescending(p => p.TotalPoints)
            .Take(20)
            .ToList();

        return Ok(topPlayers);
    }

    // GET: api/statistics/player-stats/{playerId}
    [HttpGet("player-stats/{playerId}")]
    public async Task<ActionResult<object>> GetPlayerStats(int playerId)
    {
        var player = await _context.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null)
        {
            return NotFound();
        }

        var roundPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.PlayerId == playerId)
            .OrderBy(prp => prp.Round)
            .ToListAsync();

        var stats = new
        {
            player.Id,
            player.Name,
            player.Position,
            player.TotalPoints,
            TeamName = player.Team?.Name ?? "Unknown",
            RoundsPlayed = roundPoints.Count,
            AveragePoints = roundPoints.Any() ? roundPoints.Average(rp => rp.TotalPoints) : 0.0,
            HighestScore = roundPoints.Any() ? roundPoints.Max(rp => rp.TotalPoints) : 0,
            LowestScore = roundPoints.Any() ? roundPoints.Min(rp => rp.TotalPoints) : 0,
            RoundPoints = roundPoints.Select(rp => new { rp.Round, Points = rp.TotalPoints })
        };

        return Ok(stats);
    }

    // GET: api/statistics/team-stats/{teamId}
    [HttpGet("team-stats/{teamId}")]
    public async Task<ActionResult<object>> GetTeamStats(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            return NotFound();
        }

        var teamStats = new
        {
            team.Id,
            team.Name,
            PlayerCount = team.Players.Count,
            TotalTeamPoints = team.Players.Sum(p => p.TotalPoints),
            AveragePlayerPoints = team.Players.Any() ? team.Players.Average(p => p.TotalPoints) : 0,
            TopPlayer = team.Players.OrderByDescending(p => p.TotalPoints).FirstOrDefault()
        };

        return Ok(teamStats);
    }

    // GET: api/statistics/round-summary/{round}
    [HttpGet("round-summary/{round}")]
    public async Task<ActionResult<object>> GetRoundSummary(int round)
    {
        var playerPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.Round == round)
            .Include(prp => prp.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();

        var userPoints = await _context.UserRoundPoints
            .Where(urp => urp.Round == round)
            .Include(urp => urp.User)
            .ToListAsync();

        var summary = new
        {
            Round = round,
            TotalPlayersScored = playerPoints.Count,
            TotalUsersScored = userPoints.Count,
            HighestPlayerScore = playerPoints.Any() ? playerPoints.Max(pp => pp.TotalPoints) : 0,
            HighestUserScore = userPoints.Any() ? userPoints.Max(up => up.Points) : 0,
            AveragePlayerScore = playerPoints.Any() ? playerPoints.Average(pp => pp.TotalPoints) : 0.0,
            AverageUserScore = userPoints.Any() ? userPoints.Average(up => up.Points) : 0.0,
            TopPlayers = playerPoints
                .OrderByDescending(pp => pp.TotalPoints)
                .Take(5)
                .Select(pp => new { PlayerName = pp.Player.Name, TeamName = pp.Player.Team?.Name ?? "Unknown", Points = pp.TotalPoints }),
            TopUsers = userPoints
                .OrderByDescending(up => up.Points)
                .Take(5)
                .Select(up => new { up.User.Name, up.Points })
        };

        return Ok(summary);
    }

    // GET: api/statistics/player-history/{userId}
    [HttpGet("player-history/{userId}")]
    public async Task<ActionResult<object>> GetPlayerHistory(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        // Get current round to exclude it from history
        var currentRound = await _context.CurrentRound.FirstOrDefaultAsync();
        var maxRound = currentRound?.RoundNumber ?? 0;

        var roundPoints = await _context.UserRoundPoints
            .Where(urp => urp.UserId == userId && urp.Round < maxRound) // Only past rounds
            .OrderBy(urp => urp.Round)
            .ToListAsync();

        // Get detailed team information for each round
        var detailedRounds = new List<object>();
        
        foreach (var roundPoint in roundPoints)
        {
            var teamForRound = await _context.FantasyTeams
                .Where(ft => ft.UserId == userId && ft.Round == roundPoint.Round && ft.IsActive)
                .Include(ft => ft.Player)
                    .ThenInclude(p => p.Team)
                .ToListAsync();

            // Get individual player points for this round
            var playerRoundPoints = await _context.PlayerRoundPoints
                .Where(prp => prp.Round == roundPoint.Round)
                .Include(prp => prp.Player)
                .ToListAsync();

            var courtPlayers = teamForRound.Where(ft => ft.IsOnCourt).ToList();
            var benchPlayers = teamForRound.Where(ft => !ft.IsOnCourt).ToList();

            // Calculate points for each player in the team
            var playerDetails = new List<object>();
            
            foreach (var fantasyTeam in teamForRound)
            {
                var playerPoints = playerRoundPoints.FirstOrDefault(prp => prp.PlayerId == fantasyTeam.PlayerId);
                var points = playerPoints?.TotalPoints ?? 0;
                
                playerDetails.Add(new
                {
                    playerName = fantasyTeam.Player.Name,
                    position = fantasyTeam.Player.Position,
                    teamName = fantasyTeam.Player.Team?.Name ?? "Unknown",
                    isOnCourt = fantasyTeam.IsOnCourt,
                    points = points
                });
            }

            detailedRounds.Add(new
            {
                round = roundPoint.Round,
                totalPoints = roundPoint.Points,
                courtPlayers = courtPlayers.Select(ft => new
                {
                    playerName = ft.Player.Name,
                    position = ft.Player.Position,
                    teamName = ft.Player.Team?.Name ?? "Unknown",
                    points = playerRoundPoints.FirstOrDefault(prp => prp.PlayerId == ft.PlayerId)?.TotalPoints ?? 0
                }),
                benchPlayers = benchPlayers.Select(ft => new
                {
                    playerName = ft.Player.Name,
                    position = ft.Player.Position,
                    teamName = ft.Player.Team?.Name ?? "Unknown",
                    points = playerRoundPoints.FirstOrDefault(prp => prp.PlayerId == ft.PlayerId)?.TotalPoints ?? 0
                }),
                allPlayers = playerDetails
            });
        }

        var result = new
        {
            playerName = user.Name,
            rounds = detailedRounds
        };

        return Ok(result);
    }
}
