using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FantasyTeamManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FantasyTeamManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/fantasyteammanagement/create-team
    [HttpPost("create-team")]
    public async Task<ActionResult<object>> CreateFantasyTeam([FromBody] CreateFantasyTeamRequest request)
    {
        // Validate that all players exist
        var players = await _context.Players
            .Where(p => request.PlayerIds.Contains(p.Id))
            .ToListAsync();

        if (players.Count != request.PlayerIds.Count)
        {
            return BadRequest("One or more players not found");
        }

        // Check if user exists
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        // Check if user already has a team for this round
        var existingTeam = await _context.FantasyTeams
            .AnyAsync(ft => ft.UserId == request.UserId && ft.Round == request.Round);

        if (existingTeam)
        {
            return BadRequest("User already has a fantasy team for this round");
        }

        // Create fantasy team entries
        var fantasyTeams = request.PlayerIds.Select(playerId => new FantasyTeam
        {
            UserId = request.UserId,
            PlayerId = playerId,
            Round = request.Round,
            IsActive = true
        }).ToList();

        _context.FantasyTeams.AddRange(fantasyTeams);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Fantasy team created successfully", TeamCount = fantasyTeams.Count });
    }

    // POST: api/fantasyteammanagement/calculate-round-points
    [HttpPost("calculate-round-points")]
    public async Task<ActionResult<object>> CalculateRoundPoints([FromBody] CalculateRoundPointsRequest request)
    {
        // Get all fantasy teams for the round
        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.Round == request.Round && ft.IsActive)
            .Include(ft => ft.Player)
            .ToListAsync();

        // Get player points for the round
        var playerPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.Round == request.Round)
            .ToDictionaryAsync(prp => prp.PlayerId, prp => prp.TotalPoints);

        // Calculate user points
        var userPoints = fantasyTeams
            .GroupBy(ft => ft.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalPoints = g.Sum(ft => playerPoints.GetValueOrDefault(ft.PlayerId, 0))
            })
            .ToList();

        // Update or create user round points
        foreach (var userPoint in userPoints)
        {
            var existingUserRoundPoint = await _context.UserRoundPoints
                .FirstOrDefaultAsync(urp => urp.UserId == userPoint.UserId && urp.Round == request.Round);

            if (existingUserRoundPoint != null)
            {
                existingUserRoundPoint.Points = userPoint.TotalPoints;
            }
            else
            {
                _context.UserRoundPoints.Add(new UserRoundPoints
                {
                    UserId = userPoint.UserId,
                    Round = request.Round,
                    Points = userPoint.TotalPoints
                });
            }
        }

        // Update user total points
        foreach (var userPoint in userPoints)
        {
            var user = await _context.Users.FindAsync(userPoint.UserId);
            if (user != null)
            {
                user.TotalPoints = await _context.UserRoundPoints
                    .Where(urp => urp.UserId == userPoint.UserId)
                    .SumAsync(urp => urp.Points);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Round points calculated successfully", UsersUpdated = userPoints.Count });
    }

    // GET: api/fantasyteammanagement/user/{userId}/team-summary
    [HttpGet("user/{userId}/team-summary")]
    public async Task<ActionResult<object>> GetUserTeamSummary(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.UserId == userId)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();

        var userRoundPoints = await _context.UserRoundPoints
            .Where(urp => urp.UserId == userId)
            .OrderBy(urp => urp.Round)
            .ToListAsync();

        var summary = new
        {
            User = new { user.Id, user.Name, user.TotalPoints },
            TotalRounds = fantasyTeams.Select(ft => ft.Round).Distinct().Count(),
            FantasyTeams = fantasyTeams
                .GroupBy(ft => ft.Round)
                .Select(g => new
                {
                    Round = g.Key,
                    Players = g.Select(ft => new
                    {
                        ft.Player.Id,
                        ft.Player.Name,
                        ft.Player.Position,
                        TeamName = ft.Player.Team?.Name ?? "Unknown"
                    }).ToList()
                }).ToList(),
            RoundPoints = userRoundPoints.Select(urp => new { urp.Round, urp.Points })
        };

        return Ok(summary);
    }

    // DELETE: api/fantasyteammanagement/user/{userId}/round/{round}
    [HttpDelete("user/{userId}/round/{round}")]
    public async Task<IActionResult> DeleteUserFantasyTeam(int userId, int round)
    {
        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.UserId == userId && ft.Round == round)
            .ToListAsync();

        if (!fantasyTeams.Any())
        {
            return NotFound("Fantasy team not found for this user and round");
        }

        _context.FantasyTeams.RemoveRange(fantasyTeams);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// Request models
public class CreateFantasyTeamRequest
{
    public int UserId { get; set; }
    public int Round { get; set; }
    public List<int> PlayerIds { get; set; } = new();
}

public class CalculateRoundPointsRequest
{
    public int Round { get; set; }
}
