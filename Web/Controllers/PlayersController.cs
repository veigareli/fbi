using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlayersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Players>>> GetPlayers()
    {
        return await _context.Players
            .Include(p => p.Team)
            .ToListAsync();
    }

    // GET: api/players/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Players>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return NotFound();
        }

        return player;
    }

    // POST: api/players
    [HttpPost]
    public async Task<ActionResult<Players>> PostPlayer(PlayerDto playerDto)
    {
        // Verify team exists
        var team = await _context.Teams.FindAsync(playerDto.TeamId);
        if (team == null)
        {
            return BadRequest("Team not found");
        }

        var player = new Players
        {
            TeamId = playerDto.TeamId,
            Name = playerDto.Name,
            Position = playerDto.Position,
            Cost = playerDto.Cost,
            TotalPoints = playerDto.TotalPoints
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
    }

    // PUT: api/players/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayer(int id, Players player)
    {
        if (id != player.Id)
        {
            return BadRequest();
        }

        _context.Entry(player).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlayerExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/players/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/players/5/round-points
    [HttpGet("{id}/round-points")]
    public async Task<ActionResult<IEnumerable<PlayerRoundPoints>>> GetPlayerRoundPoints(int id)
    {
        var roundPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.PlayerId == id)
            .OrderBy(prp => prp.Round)
            .ToListAsync();

        return roundPoints;
    }

    // GET: api/players/by-position/{position}
    [HttpGet("by-position/{position}")]
    public async Task<ActionResult<IEnumerable<Players>>> GetPlayersByPosition(string position)
    {
        var players = await _context.Players
            .Where(p => p.Position.ToLower() == position.ToLower())
            .Include(p => p.Team)
            .ToListAsync();

        return players;
    }

    // GET: api/players/by-team/{teamId}
    [HttpGet("by-team/{teamId}")]
    public async Task<ActionResult<IEnumerable<Players>>> GetPlayersByTeam(int teamId)
    {
        var players = await _context.Players
            .Where(p => p.TeamId == teamId)
            .Include(p => p.Team)
            .ToListAsync();

        return players;
    }

    private bool PlayerExists(int id)
    {
        return _context.Players.Any(e => e.Id == id);
    }
}
