using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SampleDataController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/sampledata/create-sample-data
    [HttpPost("create-sample-data")]
    public async Task<ActionResult<object>> CreateSampleData()
    {
        try
        {
            // Create teams
            var lakers = new Teams { Name = "Los Angeles Lakers" };
            var warriors = new Teams { Name = "Golden State Warriors" };
            var celtics = new Teams { Name = "Boston Celtics" };
            var heat = new Teams { Name = "Miami Heat" };
            var bucks = new Teams { Name = "Milwaukee Bucks" };

            _context.Teams.AddRange(lakers, warriors, celtics, heat, bucks);
            await _context.SaveChangesAsync();

            // Create players
            var players = new List<Players>
            {
                new Players { TeamId = lakers.Id, Name = "LeBron James", Position = "SF", Cost = 100, TotalPoints = 0 },
                new Players { TeamId = lakers.Id, Name = "Anthony Davis", Position = "PF", Cost = 95, TotalPoints = 0 },
                new Players { TeamId = lakers.Id, Name = "Russell Westbrook", Position = "PG", Cost = 85, TotalPoints = 0 },
                new Players { TeamId = warriors.Id, Name = "Stephen Curry", Position = "PG", Cost = 100, TotalPoints = 0 },
                new Players { TeamId = warriors.Id, Name = "Klay Thompson", Position = "SG", Cost = 90, TotalPoints = 0 },
                new Players { TeamId = warriors.Id, Name = "Draymond Green", Position = "PF", Cost = 80, TotalPoints = 0 },
                new Players { TeamId = celtics.Id, Name = "Jayson Tatum", Position = "SF", Cost = 95, TotalPoints = 0 },
                new Players { TeamId = celtics.Id, Name = "Jaylen Brown", Position = "SG", Cost = 90, TotalPoints = 0 },
                new Players { TeamId = celtics.Id, Name = "Marcus Smart", Position = "PG", Cost = 75, TotalPoints = 0 },
                new Players { TeamId = heat.Id, Name = "Jimmy Butler", Position = "SF", Cost = 90, TotalPoints = 0 },
                new Players { TeamId = heat.Id, Name = "Bam Adebayo", Position = "C", Cost = 85, TotalPoints = 0 },
                new Players { TeamId = bucks.Id, Name = "Giannis Antetokounmpo", Position = "PF", Cost = 100, TotalPoints = 0 },
                new Players { TeamId = bucks.Id, Name = "Khris Middleton", Position = "SF", Cost = 85, TotalPoints = 0 },
                new Players { TeamId = bucks.Id, Name = "Jrue Holiday", Position = "PG", Cost = 80, TotalPoints = 0 },
                new Players { TeamId = lakers.Id, Name = "Austin Reaves", Position = "SG", Cost = 70, TotalPoints = 0 }
            };

            _context.Players.AddRange(players);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Sample data created successfully", 
                teamsCreated = 5, 
                playersCreated = players.Count 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating sample data", error = ex.Message });
        }
    }

    // POST: api/sampledata/create-fantasy-team/{userId}
    [HttpPost("create-fantasy-team/{userId}")]
    public async Task<ActionResult<object>> CreateFantasyTeam(int userId)
    {
        try
        {
            // Get first 10 players
            var players = await _context.Players.Take(10).ToListAsync();
            
            if (players.Count() < 10)
            {
                return BadRequest("Not enough players available. Create sample data first.");
            }

            // Create fantasy team for round 1
            var fantasyTeams = players.Select((player, index) => new FantasyTeam
            {
                UserId = userId,
                PlayerId = player.Id,
                Round = 1,
                IsActive = true
            }).ToList();

            _context.FantasyTeams.AddRange(fantasyTeams);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Fantasy team created successfully", 
                playersAdded = fantasyTeams.Count(),
                round = 1
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating fantasy team", error = ex.Message });
        }
    }
}
