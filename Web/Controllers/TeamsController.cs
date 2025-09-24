using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/teams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Teams>>> GetTeams()
    {
        return await _context.Teams
            .Include(t => t.Players)
            .ToListAsync();
    }

    // GET: api/teams/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Teams>> GetTeam(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return team;
    }

    // POST: api/teams
    [HttpPost]
    public async Task<ActionResult<Teams>> PostTeam(Teams team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTeam", new { id = team.Id }, team);
    }

    // PUT: api/teams/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTeam(int id, Teams team)
    {
        if (id != team.Id)
        {
            return BadRequest();
        }

        _context.Entry(team).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TeamExists(id))
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

    // DELETE: api/teams/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/teams/5/players
    [HttpGet("{id}/players")]
    public async Task<ActionResult<IEnumerable<Players>>> GetTeamPlayers(int id)
    {
        var players = await _context.Players
            .Where(p => p.TeamId == id)
            .ToListAsync();

        return players;
    }

    private bool TeamExists(int id)
    {
        return _context.Teams.Any(e => e.Id == id);
    }
}
