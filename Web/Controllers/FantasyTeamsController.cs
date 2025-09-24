using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FantasyTeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FantasyTeamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/fantasyteams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FantasyTeam>>> GetFantasyTeams()
    {
        return await _context.FantasyTeams
            .Include(ft => ft.User)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();
    }

    // GET: api/fantasyteams/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FantasyTeam>> GetFantasyTeam(int id)
    {
        var fantasyTeam = await _context.FantasyTeams
            .Include(ft => ft.User)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .FirstOrDefaultAsync(ft => ft.Id == id);

        if (fantasyTeam == null)
        {
            return NotFound();
        }

        return fantasyTeam;
    }

    // POST: api/fantasyteams
    [HttpPost]
    public async Task<ActionResult<FantasyTeam>> PostFantasyTeam(FantasyTeam fantasyTeam)
    {
        _context.FantasyTeams.Add(fantasyTeam);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetFantasyTeam", new { id = fantasyTeam.Id }, fantasyTeam);
    }

    // PUT: api/fantasyteams/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFantasyTeam(int id, FantasyTeam fantasyTeam)
    {
        if (id != fantasyTeam.Id)
        {
            return BadRequest();
        }

        _context.Entry(fantasyTeam).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FantasyTeamExists(id))
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

    // DELETE: api/fantasyteams/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFantasyTeam(int id)
    {
        var fantasyTeam = await _context.FantasyTeams.FindAsync(id);
        if (fantasyTeam == null)
        {
            return NotFound();
        }

        _context.FantasyTeams.Remove(fantasyTeam);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/fantasyteams/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<FantasyTeam>>> GetFantasyTeamsByUser(int userId)
    {
        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.UserId == userId)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();

        return fantasyTeams;
    }

    // GET: api/fantasyteams/user/{userId}/round/{round}
    [HttpGet("user/{userId}/round/{round}")]
    public async Task<ActionResult<IEnumerable<FantasyTeam>>> GetFantasyTeamsByUserAndRound(int userId, int round)
    {
        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.UserId == userId && ft.Round == round)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();

        return fantasyTeams;
    }

    // GET: api/fantasyteams/round/{round}
    [HttpGet("round/{round}")]
    public async Task<ActionResult<IEnumerable<FantasyTeam>>> GetFantasyTeamsByRound(int round)
    {
        var fantasyTeams = await _context.FantasyTeams
            .Where(ft => ft.Round == round)
            .Include(ft => ft.User)
            .Include(ft => ft.Player)
            .ThenInclude(p => p.Team)
            .ToListAsync();

        return fantasyTeams;
    }

    // POST: api/fantasyteams/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<FantasyTeam>>> PostFantasyTeamsBulk(IEnumerable<FantasyTeam> fantasyTeams)
    {
        _context.FantasyTeams.AddRange(fantasyTeams);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetFantasyTeams", fantasyTeams);
    }

    private bool FantasyTeamExists(int id)
    {
        return _context.FantasyTeams.Any(e => e.Id == id);
    }
}
