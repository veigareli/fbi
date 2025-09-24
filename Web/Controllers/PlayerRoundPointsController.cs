using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerRoundPointsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlayerRoundPointsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/playerroundpoints
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerRoundPoints>>> GetPlayerRoundPoints()
    {
        return await _context.PlayerRoundPoints
            .Include(prp => prp.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();
    }

    // GET: api/playerroundpoints/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerRoundPoints>> GetPlayerRoundPoint(int id)
    {
        var playerRoundPoint = await _context.PlayerRoundPoints
            .Include(prp => prp.Player)
            .ThenInclude(p => p.Team)
            .FirstOrDefaultAsync(prp => prp.Id == id);

        if (playerRoundPoint == null)
        {
            return NotFound();
        }

        return playerRoundPoint;
    }

    // POST: api/playerroundpoints
    [HttpPost]
    public async Task<ActionResult<PlayerRoundPoints>> PostPlayerRoundPoint(PlayerRoundPoints playerRoundPoint)
    {
        _context.PlayerRoundPoints.Add(playerRoundPoint);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerRoundPoint", new { id = playerRoundPoint.Id }, playerRoundPoint);
    }

    // PUT: api/playerroundpoints/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerRoundPoint(int id, PlayerRoundPoints playerRoundPoint)
    {
        if (id != playerRoundPoint.Id)
        {
            return BadRequest();
        }

        _context.Entry(playerRoundPoint).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlayerRoundPointExists(id))
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

    // DELETE: api/playerroundpoints/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerRoundPoint(int id)
    {
        var playerRoundPoint = await _context.PlayerRoundPoints.FindAsync(id);
        if (playerRoundPoint == null)
        {
            return NotFound();
        }

        _context.PlayerRoundPoints.Remove(playerRoundPoint);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/playerroundpoints/player/{playerId}
    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<IEnumerable<PlayerRoundPoints>>> GetPlayerRoundPointsByPlayer(int playerId)
    {
        var playerRoundPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.PlayerId == playerId)
            .Include(prp => prp.Player)
            .ThenInclude(p => p.Team)
            .OrderBy(prp => prp.Round)
            .ToListAsync();

        return playerRoundPoints;
    }

    // GET: api/playerroundpoints/round/{round}
    [HttpGet("round/{round}")]
    public async Task<ActionResult<IEnumerable<PlayerRoundPoints>>> GetPlayerRoundPointsByRound(int round)
    {
        var playerRoundPoints = await _context.PlayerRoundPoints
            .Where(prp => prp.Round == round)
            .Include(prp => prp.Player)
            .ThenInclude(p => p.Team)
            .OrderByDescending(prp => prp.TotalPoints)
            .ToListAsync();

        return playerRoundPoints;
    }

    // POST: api/playerroundpoints/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<PlayerRoundPoints>>> PostPlayerRoundPointsBulk(IEnumerable<PlayerRoundPoints> playerRoundPoints)
    {
        _context.PlayerRoundPoints.AddRange(playerRoundPoints);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerRoundPoints", playerRoundPoints);
    }

    private bool PlayerRoundPointExists(int id)
    {
        return _context.PlayerRoundPoints.Any(e => e.Id == id);
    }
}
